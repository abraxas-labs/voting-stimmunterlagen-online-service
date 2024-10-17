// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceVotingCardLayoutTests;

public class SetDomainOfInfluenceVotingCardLayoutTemplateDataTest
    : BaseWriteableDbGrpcTest<DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceClient>
{
    public SetDomainOfInfluenceVotingCardLayoutTemplateDataTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldUpdateValues()
    {
        await GemeindeArneggElectionAdminClient.SetTemplateDataAsync(NewRequest());

        var data = await GemeindeArneggElectionAdminClient.GetTemplateDataAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId });
        data.MatchSnapshot();
    }

    [Fact]
    public Task ShouldThrownIfNotDoiManager()
    {
        return AssertStatus(
            async () => await AbraxasElectionAdminClient.SetTemplateDataAsync(NewRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public Task ShouldThrowWithUnknownValue()
    {
        var req = NewRequest();
        req.Fields.Add(new SetTemplateDataFieldRequest
        {
            Value = "test",
            ContainerKey = "test",
            FieldKey = "test",
        });

        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetTemplateDataAsync(req),
            StatusCode.InvalidArgument,
            "test-test");
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await SetContestState(ContestMockData.BundFutureApprovedGuid, Data.Models.ContestState.Archived);
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetTemplateDataAsync(NewRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfContestPastSignUpDeadline()
    {
        await SetContestBundFutureApprovedToPastSignUpDeadline();
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetTemplateDataAsync(NewRequest()),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(
        DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceClient service)
    {
        await service.SetTemplateDataAsync(NewRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private SetDomainOfInfluenceVotingCardLayoutTemplateDataRequest NewRequest()
    {
        return new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
            Fields =
                {
                    new SetTemplateDataFieldRequest
                    {
                        ContainerKey = "urne",
                        FieldKey = "zeit",
                        Value = "18:00",
                    },
                    new SetTemplateDataFieldRequest
                    {
                        ContainerKey = "urne",
                        FieldKey = "standort",
                        Value = "Turnhalle West",
                    },
                    new SetTemplateDataFieldRequest
                    {
                        ContainerKey = "e_voting",
                        FieldKey = "e_voting",
                        Value = "ArneggSuperVote",
                    },
                    new SetTemplateDataFieldRequest
                    {
                        ContainerKey = "e_voting",
                        FieldKey = "domain",
                        Value = "vote.arnegg.ch",
                    },
                },
        };
    }
}
