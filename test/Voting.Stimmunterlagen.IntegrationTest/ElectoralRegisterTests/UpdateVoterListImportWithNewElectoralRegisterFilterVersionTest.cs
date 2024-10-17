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
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.IntegrationTest.MockData.Stimmregister;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using VoterListImport = Voting.Stimmunterlagen.Data.Models.VoterListImport;

namespace Voting.Stimmunterlagen.IntegrationTest.ElectoralRegisterTests;

public class UpdateVoterListImportWithNewElectoralRegisterFilterVersionTest : BaseWriteableDbGrpcTest<ElectoralRegisterService.ElectoralRegisterServiceClient>
{
    public UpdateVoterListImportWithNewElectoralRegisterFilterVersionTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var response = await GemeindeArneggElectionAdminClient.UpdateVoterListImportWithNewFilterVersionAsync(NewRequest());
        response.VoterLists.Any(vl => vl.Id == string.Empty).Should().BeFalse();
        foreach (var voterList in response.VoterLists)
        {
            voterList.Id = string.Empty;
        }

        response.FilterVersionId.Should().Be(StimmregisterFilterMockData.SwissWithVotingRightVersion4NewCreatedId);
        response.MatchSnapshot("response");

        var voterListImport = await GetVoterListImport(VoterListImportMockData.BundFutureApprovedGemeindeArneggElectoralRegisterGuid);
        voterListImport.Name.Should().Be("CH OK / V4 01.01.2024");
        voterListImport.MatchSnapshot("data");

        // test if existing political business entries on voter list got applied to the new voter list
        (await RunOnDb(db => db.VoterLists.AnyAsync(vl => vl.Id == VoterListMockData.BundFutureApprovedGemeindeArneggSwissElectoralRegisterGuid)))
            .Should().BeFalse();

        var newSwissVoterList = await RunOnDb(db => db.VoterLists
            .Include(vl => vl.PoliticalBusinessEntries!.OrderBy(x => x.Id))
            .SingleOrDefaultAsync(vl => vl.ImportId == VoterListImportMockData.BundFutureApprovedGemeindeArneggElectoralRegisterGuid && vl.VotingCardType == Data.Models.VotingCardType.Swiss));
        newSwissVoterList!.PoliticalBusinessEntries!.Any().Should().BeTrue();
    }

    [Fact]
    public Task ShouldThrowOnUnknownFilterVersionId()
    {
        var request = NewRequest(x => x.FilterId = "4ddfdfcf-418d-4865-b744-97dbe4220cce");
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateVoterListImportWithNewFilterVersionAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public Task ShouldThrowOnUnknownVoterListId()
    {
        var request = NewRequest(x => x.ImportId = "ba46ba4d-0cad-4350-b8ba-01f3d06a9702");
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateVoterListImportWithNewFilterVersionAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public Task ShouldThrowOnOtherVoterListSource()
    {
        var request = NewRequest(x => x.ImportId = VoterListImportMockData.BundFutureApprovedGemeindeArneggId);
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateVoterListImportWithNewFilterVersionAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public Task ShouldThrowOtherTenant()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.UpdateVoterListImportWithNewFilterVersionAsync(NewRequest()),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(ElectoralRegisterService.ElectoralRegisterServiceClient service)
        => await service.UpdateVoterListImportWithNewFilterVersionAsync(NewRequest());

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private async Task<VoterListImport> GetVoterListImport(Guid id)
    {
        var voterListImport = await RunOnDb(db => db.VoterListImports
            .Include(x => x.VoterLists!.OrderBy(vl => vl.VotingCardType))
            .ThenInclude(x => x.Voters!.OrderBy(y => y.LastName))
            .Include(x => x.VoterLists!.OrderBy(vl => vl.VotingCardType))
            .ThenInclude(x => x.VoterDuplicates)
            .SingleAsync(x => x.Id == id));

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
        }

        return voterListImport;
    }

    private UpdateVoterListImportWithNewElectoralRegisterFilterVersionRequest NewRequest(Action<UpdateVoterListImportWithNewElectoralRegisterFilterVersionRequest>? customizer = null)
    {
        var request = new UpdateVoterListImportWithNewElectoralRegisterFilterVersionRequest
        {
            ImportId = VoterListImportMockData.BundFutureApprovedGemeindeArneggElectoralRegisterId,
            FilterId = StimmregisterFilterMockData.SwissWithVotingRightId,
            FilterVersionDeadline = GrpcTestUtils.CreateTimestamp(2023, 12, 1),
            FilterVersionName = "V99",
        };
        customizer?.Invoke(request);
        return request;
    }
}
