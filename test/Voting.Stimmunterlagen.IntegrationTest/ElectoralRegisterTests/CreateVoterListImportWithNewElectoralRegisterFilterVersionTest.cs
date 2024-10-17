// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.IntegrationTest.MockData.Stimmregister;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using VoterListImport = Voting.Stimmunterlagen.Data.Models.VoterListImport;

namespace Voting.Stimmunterlagen.IntegrationTest.ElectoralRegisterTests;

public class CreateVoterListImportWithNewElectoralRegisterFilterVersionTest : BaseWriteableDbGrpcTest<ElectoralRegisterService.ElectoralRegisterServiceClient>
{
    public CreateVoterListImportWithNewElectoralRegisterFilterVersionTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var response = await GemeindeArneggElectionAdminClient.CreateVoterListImportWithNewFilterVersionAsync(NewRequest());
        var importId = response.ImportId;
        response.ImportId.Should().NotBeEmpty();
        response.VoterLists.Any(vl => vl.Id == string.Empty).Should().BeFalse();

        response.ImportId = string.Empty;
        foreach (var voterList in response.VoterLists)
        {
            voterList.Id = string.Empty;
        }

        response.FilterVersionId.Should().Be(StimmregisterFilterMockData.SwissWithVotingRightVersion4NewCreatedId);
        response.MatchSnapshot("response");

        var voterListImport = await GetVoterListImport(importId);
        voterListImport.MatchSnapshot("data");
    }

    [Fact]
    public Task ShouldThrowOnUnknownFilterVersionId()
    {
        var request = NewRequest(x => x.FilterId = "c94106ea-d4f4-4ca9-8a45-7d90c88c91c6");
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.CreateVoterListImportWithNewFilterVersionAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public Task ShouldThrowOnUnknownDomainOfInfluenceId()
    {
        var request = NewRequest(x => x.DomainOfInfluenceId = "25a20f60-46ba-4226-ad5e-b2d7f70517a2");
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.CreateVoterListImportWithNewFilterVersionAsync(request),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public Task ShouldThrowOtherTenant()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.CreateVoterListImportWithNewFilterVersionAsync(NewRequest()),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ShouldThrowElectoralRegisterDisabled()
    {
        await ModifyDbEntities(
            (ContestDomainOfInfluence doi) => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            doi => doi.ElectoralRegistrationEnabled = false);
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.CreateVoterListImportWithNewFilterVersionAsync(NewRequest()),
            StatusCode.PermissionDenied);
    }

    protected override async Task AuthorizationTestCall(ElectoralRegisterService.ElectoralRegisterServiceClient service)
        => await service.CreateVoterListImportWithNewFilterVersionAsync(NewRequest());

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private async Task<VoterListImport> GetVoterListImport(string id)
    {
        var voterListImport = await RunOnDb(db => db.VoterListImports
            .Include(x => x.VoterLists!.OrderBy(vl => vl.VotingCardType))
            .ThenInclude(x => x.Voters!.OrderBy(y => y.LastName))
            .Include(x => x.VoterLists!)
            .ThenInclude(x => x.VoterDuplicates)
            .Include(x => x.VoterLists!)
            .ThenInclude(x => x.PoliticalBusinessEntries!.OrderBy(y => y.PoliticalBusinessId))
            .SingleAsync(x => x.Id == Guid.Parse(id)));

        // clean data for snapshot
        voterListImport.Id = Guid.Empty;
        foreach (var voterList in voterListImport.VoterLists!)
        {
            voterList.Import = null;
            voterList.ImportId = Guid.Empty;
            voterList.Id = Guid.Empty;

            foreach (var voter in voterList.Voters!)
            {
                voter.Id = Guid.Empty;
                voter.ListId = Guid.Empty;
                voter.List = null;

                foreach (var placeOfOrigin in voter.PlacesOfOrigin!)
                {
                    placeOfOrigin.Voter = null!;
                    placeOfOrigin.VoterId = Guid.Empty;
                }

                voter.ContestIndex.Should().NotBe(0);
                voter.ContestIndex = 0;
            }

            foreach (var voterDuplicate in voterList.VoterDuplicates!)
            {
                voterDuplicate.Id = Guid.Empty;
                voterDuplicate.ListId = Guid.Empty;
                voterDuplicate.List = null;
            }

            foreach (var pbEntry in voterList.PoliticalBusinessEntries!)
            {
                pbEntry.Id = Guid.Empty;
                pbEntry.VoterListId = Guid.Empty;
                pbEntry.VoterList = null;
            }
        }

        return voterListImport;
    }

    private CreateVoterListImportWithNewElectoralRegisterFilterVersionRequest NewRequest(Action<CreateVoterListImportWithNewElectoralRegisterFilterVersionRequest>? customizer = null)
    {
        var request = new CreateVoterListImportWithNewElectoralRegisterFilterVersionRequest
        {
            FilterId = StimmregisterFilterMockData.SwissWithVotingRightId,
            FilterVersionDeadline = GrpcTestUtils.CreateTimestamp(2023, 12, 1),
            FilterVersionName = "V99",
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        };
        customizer?.Invoke(request);
        return request;
    }
}
