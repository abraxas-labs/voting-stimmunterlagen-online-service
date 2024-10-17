// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using ContestState = Voting.Stimmunterlagen.Data.Models.ContestState;
using Step = Voting.Stimmunterlagen.Data.Models.Step;

namespace Voting.Stimmunterlagen.IntegrationTest.ManualVotingCardGeneratorJobTests;

public class CreateManualVotingCardGeneratorJobTest : BaseWriteableDbGrpcTest<ManualVotingCardGeneratorJobsService.ManualVotingCardGeneratorJobsServiceClient>
{
    public CreateManualVotingCardGeneratorJobTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await SetStepState(DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid, Step.GenerateVotingCards, true);
    }

    [Fact]
    public async Task ShouldWork()
    {
        var resp = await GemeindeArneggElectionAdminClient.CreateAsync(NewValidRequest());
        resp.Should().NotBeNull();
        resp.Pdf.ShouldBeAPdf();

        var job = await RunOnDb(db => db.ManualVotingCardGeneratorJobs
            .Where(x => x.Created == MockedClock.UtcNowDate
                        && x.Layout.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .Include(x => x.Voter)
            .Include(x => x.Layout)
            .SingleAsync());

        job.Id = Guid.Empty;
        job.LayoutId = Guid.Empty;
        job.Layout.Id = Guid.Empty;
        job.Voter.Id = Guid.Empty;
        job.Voter.ManualJobId = Guid.Empty;
        job.MatchSnapshot();
    }

    [Fact]
    public async Task ShouldUpdateDomainOfInfluenceLastVoterUpdate()
    {
        var doi = await RunOnDb(db => db.ContestDomainOfInfluences.FirstAsync(doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid));
        doi.LastVoterUpdate.Should().BeNull();

        await GemeindeArneggElectionAdminClient.CreateAsync(NewValidRequest());
        doi = await RunOnDb(db => db.ContestDomainOfInfluences.FirstAsync(doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid));
        doi.LastVoterUpdate.Should().Be(MockedClock.GetDate());
    }

    [Fact]
    public async Task ShouldWorkWithOnlyRequiredFields()
    {
        var resp = await GemeindeArneggElectionAdminClient.CreateAsync(NewValidRequest(x =>
        {
            x.Voter.DateOfBirth = null;
        }));
        resp.Should().NotBeNull();
        resp.Pdf.ShouldBeAPdf();

        var job = await RunOnDb(db => db.ManualVotingCardGeneratorJobs
            .Where(x => x.Created == MockedClock.UtcNowDate
                        && x.Layout.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .Include(x => x.Voter)
            .Include(x => x.Layout)
            .SingleAsync());

        job.Id = Guid.Empty;
        job.LayoutId = Guid.Empty;
        job.Layout.Id = Guid.Empty;
        job.Voter.Id = Guid.Empty;
        job.Voter.ManualJobId = Guid.Empty;
        job.MatchSnapshot();
    }

    [Fact]
    public async Task ShouldThrowWithNonNumericPersonId()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.CreateAsync(
                NewValidRequest(x => x.Voter.PersonId = "101a3")),
            StatusCode.InvalidArgument,
            "Invalid PersonId");
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await SetContestState(ContestMockData.BundFutureApprovedGuid, ContestState.Archived);
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.CreateAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfVotingCardsNotGenerated()
    {
        await SetStepState(DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid, Step.GenerateVotingCards, false);
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.CreateAsync(NewValidRequest()),
            StatusCode.InvalidArgument,
            "GenerateVotingCards not found or has not the correct state");
    }

    [Fact]
    public Task ShouldThrowIfVoterInvalid()
    {
        var req = NewValidRequest();
        req.Voter.Country.Iso2 = "test";
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.CreateAsync(req),
            StatusCode.InvalidArgument,
            "Iso2");
    }

    protected override async Task AuthorizationTestCall(ManualVotingCardGeneratorJobsService.ManualVotingCardGeneratorJobsServiceClient service)
    {
        await service.CreateAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private CreateManualVotingCardGeneratorJobRequest NewValidRequest(Action<CreateManualVotingCardGeneratorJobRequest>? action = null)
    {
        var req = new CreateManualVotingCardGeneratorJobRequest()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
            Voter = new()
            {
                FirstName = "Anke",
                LastName = "Ritter",
                Street = "Kappelergasse",
                HouseNumber = "61",
                Town = "Arnegg",
                SwissZipCode = 9200,
                Country = new() { Iso2 = "CH", Name = "Schweiz" },
                LanguageOfCorrespondence = Languages.German,
                VotingCardType = VotingCardType.Swiss,
                PersonId = "10123",
                DateOfBirth = new DateTime(1991, 10, 18, 0, 0, 0, DateTimeKind.Utc).ToTimestamp(),
                Salutation = Salutation.Mistress,
            },
        };
        action?.Invoke(req);
        return req;
    }
}
