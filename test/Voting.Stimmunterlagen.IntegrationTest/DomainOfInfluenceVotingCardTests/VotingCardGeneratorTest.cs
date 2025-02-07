// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Snapper;
using Voting.Lib.DmDoc.Serialization;
using Voting.Lib.Iam.Store;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Core.Managers.Generator;
using Voting.Stimmunterlagen.Core.Managers.Templates;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceVotingCardTests;

public class VotingCardGeneratorTest : BaseWriteableDbTest
{
    private const string DefaultMessageId = "voting-card-generator-message-id-mock";
    private readonly VotingCardStoreMock _storeMock;

    public VotingCardGeneratorTest(TestApplicationFactory factory)
        : base(factory)
    {
        _storeMock = GetService<VotingCardStoreMock>();
        _storeMock.Clear();

        GetService<VotingCardGeneratorThrottlerMock>().ShouldBlock = false;
    }

    [Fact]
    public async Task ShouldWork()
    {
        await StartRun(VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid);

        var job = await GetDbEntity<VotingCardGeneratorJob>(x =>
            x.Id == VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid);
        job.Started.Should().Be(MockedClock.UtcNowDate);
        job.Runner.Should().Be(Environment.MachineName);
        job.State.Should().Be(VotingCardGeneratorJobState.Running);
        job.Completed.Should().Be(null);

        await Complete(VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid);

        job = await GetDbEntity<VotingCardGeneratorJob>(x =>
            x.Id == VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid);
        job.Completed.Should().Be(MockedClock.UtcNowDate);
        job.State.Should().Be(VotingCardGeneratorJobState.Completed);
        _storeMock.AssertFileWritten(DefaultMessageId, job.FileName);
    }

    [Fact]
    public async Task ShouldWorkWithFailure()
    {
        await StartRun(VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid);

        var job = await GetDbEntity<VotingCardGeneratorJob>(x =>
            x.Id == VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid);
        job.Started.Should().Be(MockedClock.UtcNowDate);
        job.Runner.Should().Be(Environment.MachineName);
        job.State.Should().Be(VotingCardGeneratorJobState.Running);
        job.Completed.Should().Be(null);

        await Fail(VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid);

        job = await GetDbEntity<VotingCardGeneratorJob>(x =>
            x.Id == VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid);
        job.Completed.Should().Be(null);
        job.Failed.Should().Be(MockedClock.UtcNowDate);
        job.State.Should().Be(VotingCardGeneratorJobState.Failed);
    }

    [Fact]
    public async Task ShouldWorkWithExternalPrintingCenter()
    {
        var messageType = "message-type";

        await ModifyDbEntities<ContestDomainOfInfluence>(
            doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            doi =>
            {
                doi.ExternalPrintingCenter = true;
                doi.ExternalPrintingCenterEaiMessageType = messageType;
            });

        await StartRun(VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid);
        await Complete(VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid);
        var job = await GetDbEntity<VotingCardGeneratorJob>(x =>
            x.Id == VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid);

        _storeMock.AssertFileWritten(messageType, job.FileName);
    }

    [Fact]
    public async Task AlreadyLockedShouldThrow()
    {
        using var scope = GetService<IServiceScopeFactory>().CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var jobRepo = scope.ServiceProvider.GetRequiredService<IDbRepository<VotingCardGeneratorJob>>();
        var locked = await jobRepo.TryLockForUpdate(VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid);
        locked.Should().BeTrue();

        await AssertException<ValidationException>(
            async () => await StartRun(VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid),
            "since it is locked");
    }

    [Fact]
    public async Task ShouldSerializeTemplateBag()
    {
        var jobId = VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid;
        var voterId = VoterListMockData.BundArchivedGemeindeArneggSwiss.Voters!.ElementAt(0).Id;
        await ModifyDbEntities<Voter>(
            v => voterId == v.Id,
            v =>
            {
                v.JobId = jobId;
                v.SendVotingCardsToDomainOfInfluenceReturnAddress = true;
            });

        await ModifyDbEntities<VoterList>(
            vl => vl.Id == VoterListMockData.BundArchivedGemeindeArneggSwissGuid,
            vl => vl.SendVotingCardsToDomainOfInfluenceReturnAddress = false);

        using var scope = GetService<IServiceScopeFactory>().CreateScope();
        var dataSerializer = scope.ServiceProvider.GetRequiredService<IDmDocDataSerializer>();
        var templateDataBuilder = scope.ServiceProvider.GetRequiredService<TemplateDataBuilder>();

        var job = await RunOnDb(
            db => db.VotingCardGeneratorJobs
                .IncludeLayoutEntities()
                .Include(x => x.Voter)
                .ThenInclude(x => x.List)
                .FirstAsync(x => x.Id == jobId));

        var templateBag = await templateDataBuilder.BuildBag(
            null,
            job.Layout!.DomainOfInfluence!.Contest!,
            job.Layout!.DomainOfInfluence!,
            job.Voter,
            job.Layout!.TemplateDataFieldValues!);

        var serializedData = dataSerializer.Serialize(templateBag);
        serializedData.ShouldMatchSnapshot();
    }

    [Theory]
    [InlineData(VotingCardGeneratorJobState.Running, "job is currently running")]
    [InlineData(VotingCardGeneratorJobState.Completed, "job is already completed")]
    [InlineData(VotingCardGeneratorJobState.ReadyToRunOffline, "cannot run job online which can only run offline")]
    public async Task InvalidStatesShouldThrow(VotingCardGeneratorJobState state, string ex)
    {
        await SetState(VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid, state);
        await AssertException<ValidationException>(
            async () => await StartRun(VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid),
            ex);
        await AssertIsFailed(VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid);
    }

    private async Task AssertIsFailed(Guid jobId)
    {
        var job = await GetDbEntity<VotingCardGeneratorJob>(x => x.Id == jobId);
        _storeMock.AssertFileNotWritten(DefaultMessageId, job.FileName);
        job.State.Should().Be(VotingCardGeneratorJobState.Failed);
        job.Failed.Should().Be(MockedClock.UtcNowDate);
    }

    private Task SetState(Guid jobId, VotingCardGeneratorJobState state)
    {
        return RunOnDb(async db =>
        {
            var job = await db.VotingCardGeneratorJobs.FindAsync(jobId);
            job!.State = state;
            db.Update(job);
            await db.SaveChangesAsync();
        });
    }

    private async Task StartRun(Guid jobId)
    {
        using var scope = GetService<IServiceScopeFactory>().CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthStore>();
        auth.SetValues(
            "mock-token",
            "mock-data-seeder",
            "SC-ABX",
            Enumerable.Empty<string>());
        var generator = scope.ServiceProvider.GetRequiredService<VotingCardGenerator>();
        await generator.StartJob(jobId, CancellationToken.None);
    }

    private async Task Complete(Guid jobId)
    {
        using var scope = GetService<IServiceScopeFactory>().CreateScope();
        var generator = scope.ServiceProvider.GetRequiredService<VotingCardGenerator>();
        await generator.Complete(jobId, 0, CancellationToken.None);
    }

    private async Task Fail(Guid jobId)
    {
        using var scope = GetService<IServiceScopeFactory>().CreateScope();
        var generator = scope.ServiceProvider.GetRequiredService<VotingCardGenerator>();
        await generator.Fail(jobId);
    }
}
