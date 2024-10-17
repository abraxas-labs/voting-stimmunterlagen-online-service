// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;
using VoterListSource = Voting.Stimmunterlagen.Data.Models.VoterListSource;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceTests;

public class ListEVotingDomainOfInfluenceTest : BaseReadOnlyGrpcTest<DomainOfInfluenceService.DomainOfInfluenceServiceClient>
{
    public ListEVotingDomainOfInfluenceTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ListByContestManagerShouldReturn()
    {
        await RunOnDb(async db =>
        {
            var bundFutureImport1 = new Data.Models.VoterListImport
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundGuid,
                Source = VoterListSource.ManualEch45Upload,
                VoterLists = new List<Data.Models.VoterList>
                {
                    new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundGuid, NumberOfVoters = 1000, VotingCardType = Data.Models.VotingCardType.EVoting },
                },
            };

            var bundFutureImport2 = new Data.Models.VoterListImport
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundGuid,
                Source = VoterListSource.ManualEch45Upload,
                VoterLists = new List<Data.Models.VoterList>
                {
                    new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundGuid, NumberOfVoters = 1000, VotingCardType = Data.Models.VotingCardType.EVoting },
                },
            };

            var kantonStGallenImport = new Data.Models.VoterListImport
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureKantonStGallenGuid,
                Source = VoterListSource.ManualEch45Upload,
                VoterLists = new List<Data.Models.VoterList>
                {
                    new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureKantonStGallenGuid, NumberOfVoters = 1500, VotingCardType = Data.Models.VotingCardType.EVoting },
                    new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureKantonStGallenGuid, NumberOfVoters = 1500, VotingCardType = Data.Models.VotingCardType.SwissAbroad },
                },
            };
            var stadtGossauImport = new Data.Models.VoterListImport
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid,
                Source = VoterListSource.ManualEch45Upload,
                VoterLists = new List<Data.Models.VoterList>
                {
                    new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid, NumberOfVoters = 5000, VotingCardType = Data.Models.VotingCardType.Swiss },
                },
            };

            db.VoterListImports.AddRange(
                bundFutureImport1,
                bundFutureImport2,
                kantonStGallenImport,
                stadtGossauImport);

            var uzwilStepState = await db.StepStates.AsTracking()
                .SingleAsync(s => s.Step == Data.Models.Step.GenerateVotingCards && s.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureStadtUzwilGuid);
            uzwilStepState.Approved = true;

            var uzwilCc = await db.ContestCountingCircles
                .AsTracking()
                .SingleAsync(cc => cc.Id == CountingCircleMockData.ContestBundFutureStadtUzwilGuid);
            uzwilCc.EVoting = true;

            await db.SaveChangesAsync();
        });

        var dois = await AbraxasElectionAdminClient.ListEVotingAsync(new() { ContestId = ContestMockData.BundFutureId });
        dois.MatchSnapshot();
    }

    [Fact]
    public async Task ListByNonContestManagerShouldReturnEmpty()
    {
        var dois = await StadtGossauElectionAdminClient.ListEVotingAsync(new() { ContestId = ContestMockData.BundFutureId });
        dois.Entries.Should().BeEmpty();
    }

    protected override async Task AuthorizationTestCall(DomainOfInfluenceService.DomainOfInfluenceServiceClient service)
    {
        await service.ListEVotingAsync(new() { ContestId = ContestMockData.BundFutureId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
