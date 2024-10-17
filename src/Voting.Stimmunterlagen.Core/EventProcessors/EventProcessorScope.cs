// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Data;
using System.Threading.Tasks;
using EventStore.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Voting.Lib.Eventing.Subscribe;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public sealed class EventProcessorScope : IEventProcessorScope, IDisposable
{
    private readonly IDbRepository<EventProcessingState> _repo;
    private readonly DataContext _dbContext;
    private readonly ILogger<EventProcessorScope> _logger;
    private IDbContextTransaction? _transaction;
    private bool? _isInReplay;

    public EventProcessorScope(IDbRepository<EventProcessingState> repo, DataContext dbContext, ILogger<EventProcessorScope> logger)
    {
        _repo = repo;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Gets a value indicating whether the event, which is processed currently was processed before (the system is in replay).
    /// False if the event was never processed by the system before (the system is not in replay).
    /// </summary>
    /// <exception cref="InvalidOperationException">If no event is currently processed by this scope.</exception>
    public bool IsInReplay =>
        _isInReplay ?? throw new InvalidOperationException("currently not processing an event");

    public async Task Begin(Position position, StreamPosition streamPosition)
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
        await SetLastProcessedPosition(position, streamPosition);
    }

    public async Task Complete(Position position, StreamPosition streamPosition)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Event processing cannot complete when transaction is null");
        }

        await _transaction.CommitAsync();
        _isInReplay = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _isInReplay = null;
    }

    public async Task<(Position, StreamPosition)?> GetSnapshotPosition()
    {
        var state = await _repo.GetByKey(EventProcessingState.StaticId);
        return state is not { LastProcessedEventCommitPosition: { }, LastProcessedEventPreparePosition: { }, LastProcessedEventNumber: { } }
            ? null
            : (new Position(state.LastProcessedEventCommitPosition.Value, state.LastProcessedEventPreparePosition.Value), state.LastProcessedEventNumber.Value);
    }

    private async Task SetLastProcessedPosition(Position position, StreamPosition streamPosition)
    {
        var existingEventProcessingState = await _repo.GetByKey(EventProcessingState.StaticId);
        if (existingEventProcessingState == null)
        {
            await _repo.Create(new EventProcessingState
            {
                LastProcessedEventCommitPosition = position.CommitPosition,
                LastProcessedEventPreparePosition = position.PreparePosition,
                LatestEverProcessedEventNumber = streamPosition,
                LastProcessedEventNumber = streamPosition,
            });
            _isInReplay = false;
            return;
        }

        if (streamPosition <= existingEventProcessingState.LatestEverProcessedEventNumber)
        {
            _logger.LogCritical(
                "Received event with number {EventNumber} which seems to be out of order in consideration of current snapshot event number {SnapshotEventNumber}",
                streamPosition,
                existingEventProcessingState.LatestEverProcessedEventNumber);
        }

        existingEventProcessingState.LastProcessedEventCommitPosition = position.CommitPosition;
        existingEventProcessingState.LastProcessedEventPreparePosition = position.PreparePosition;
        existingEventProcessingState.LastProcessedEventNumber = streamPosition;

        _isInReplay = streamPosition <= existingEventProcessingState.LatestEverProcessedEventNumber;
        if (streamPosition > existingEventProcessingState.LatestEverProcessedEventNumber)
        {
            existingEventProcessingState.LatestEverProcessedEventNumber = streamPosition;
        }

        await _repo.Update(existingEventProcessingState);
    }
}
