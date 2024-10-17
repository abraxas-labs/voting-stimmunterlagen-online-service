// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Utils;

public class ContestOrderNumberStateBuilder
{
    private readonly IDbRepository<ContestOrderNumberState> _contestOrderNumberStateRepo;
    private readonly IDbRepository<Contest> _contestRepo;
    private readonly ContestOrderNumberConfig _contestOrderNumberConfig;

    public ContestOrderNumberStateBuilder(
        IDbRepository<ContestOrderNumberState> contestOrderNumberStateRepo,
        IDbRepository<Contest> contestRepo,
        EventProcessorConfig eventProcessorConfig)
    {
        _contestOrderNumberStateRepo = contestOrderNumberStateRepo;
        _contestOrderNumberConfig = eventProcessorConfig.ContestOrderNumber;
        _contestRepo = contestRepo;
    }

    public async Task<int> NextOrderNumber(DateTime contestDate)
    {
        var contestOrderNumberState = await _contestOrderNumberStateRepo
            .Query()
            .SingleOrDefaultAsync();

        var nextOrderNumber = contestOrderNumberState == null ||
                              contestOrderNumberState.LastSetOrderNumber < _contestOrderNumberConfig.Min ||
                              contestOrderNumberState.LastSetOrderNumber >= _contestOrderNumberConfig.Max
            ? _contestOrderNumberConfig.Min
            : contestOrderNumberState.LastSetOrderNumber + 1;

        await ValidateNextOrderNumber(contestDate, nextOrderNumber);

        if (contestOrderNumberState != null)
        {
            contestOrderNumberState.LastSetOrderNumber = nextOrderNumber;
            await _contestOrderNumberStateRepo.Update(contestOrderNumberState);
        }
        else
        {
            await _contestOrderNumberStateRepo.Create(new() { LastSetOrderNumber = nextOrderNumber });
        }

        return nextOrderNumber;
    }

    private async Task ValidateNextOrderNumber(DateTime contestDate, int nextOrderNumber)
    {
        var latestContestWithSameOrderNumber = await _contestRepo.Query()
            .OrderByDescending(c => c.Date)
            .FirstOrDefaultAsync(c => c.OrderNumber == nextOrderNumber);

        if (latestContestWithSameOrderNumber != null &&
            latestContestWithSameOrderNumber.Date.Subtract(contestDate).Duration() < _contestOrderNumberConfig.OverlapFreeTimespan)
        {
            throw new InvalidOperationException($"Order number {nextOrderNumber} has occured twice within the allowed time span of {_contestOrderNumberConfig.OverlapFreeTimespan}");
        }
    }
}
