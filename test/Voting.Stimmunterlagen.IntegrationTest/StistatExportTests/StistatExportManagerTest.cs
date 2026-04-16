// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.IntegrationTest.MockData.Stimmregister;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.StistatExportTests;

public class StistatExportManagerTest : BaseWriteableDbGrpcTest<ElectoralRegisterService.ElectoralRegisterServiceClient>
{
    private const string MessageType = "STISTAT-SG";

    private readonly StistatFileStoreMock _storeMock;

    public StistatExportManagerTest(TestApplicationFactory factory)
        : base(factory)
    {
        _storeMock = GetService<StistatFileStoreMock>();
        _storeMock.Clear();
    }

    [Fact]
    public async Task ShouldExportFilteredCsvOnImport()
    {
        await SetGuidSourceIds();
        _storeMock.SaveFileInMemory = true;

        var response = await GemeindeArneggElectionAdminClient.CreateVoterListImportWithNewFilterVersionAsync(
            NewRequest(DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId));
        response.ImportId.Should().NotBeEmpty();

        var expectedFileName = $"stistat_export_{ContestMockData.BundFutureApprovedGuid}.csv";
        _storeMock.AssertFileWritten(MessageType, expectedFileName);

        var fileContent = Encoding.UTF8.GetString(_storeMock.GetFile(MessageType));
        fileContent.MatchSnapshot();
    }

    [Fact]
    public async Task ShouldSkipWhenNotStistatMunicipality()
    {
        await SetGuidSourceIds();
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            x => x.StistatMunicipality = false);

        var response = await GemeindeArneggElectionAdminClient.CreateVoterListImportWithNewFilterVersionAsync(
            NewRequest(DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId));
        response.ImportId.Should().NotBeEmpty();

        _storeMock.AssertNoFileWritten(MessageType);
    }

    [Fact]
    public async Task ShouldSkipWhenStistatExportNotEnabled()
    {
        await SetGuidSourceIds();
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedBundGuid,
            x => x.StistatExportEaiMessageType = string.Empty);

        var response = await GemeindeArneggElectionAdminClient.CreateVoterListImportWithNewFilterVersionAsync(
            NewRequest(DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId));
        response.ImportId.Should().NotBeEmpty();

        _storeMock.AssertNoFileWritten(MessageType);
    }

    [Fact]
    public async Task ShouldSkipWhenNotAllMunicipalitiesHaveImports()
    {
        await RunOnDb(async db =>
        {
            var imports = await db.VoterListImports
                .AsTracking()
                .Where(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid)
                .ToListAsync();
            db.VoterListImports.RemoveRange(imports);

            // Also remove Gossau voter lists and voters to avoid FK violations
            var voterLists = await db.VoterLists
                .AsTracking()
                .Where(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid)
                .ToListAsync();
            db.VoterLists.RemoveRange(voterLists);

            await db.SaveChangesAsync();
        });

        var response = await GemeindeArneggElectionAdminClient.CreateVoterListImportWithNewFilterVersionAsync(
            NewRequest(DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId));
        response.ImportId.Should().NotBeEmpty();

        _storeMock.AssertNoFileWritten(MessageType);
    }

    protected override async Task AuthorizationTestCall(ElectoralRegisterService.ElectoralRegisterServiceClient service)
        => await service.CreateVoterListImportWithNewFilterVersionAsync(
            NewRequest(DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId));

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
    }

    private static CreateVoterListImportWithNewElectoralRegisterFilterVersionRequest NewRequest(string domainOfInfluenceId)
    {
        return new CreateVoterListImportWithNewElectoralRegisterFilterVersionRequest
        {
            FilterId = StimmregisterFilterMockData.SwissWithVotingRightId,
            FilterVersionDeadline = GrpcTestUtils.CreateTimestamp(2023, 12, 1),
            FilterVersionName = "V99-Stistat",
            DomainOfInfluenceId = domainOfInfluenceId,
        };
    }

    private Task SetGuidSourceIds()
    {
        return ModifyDbEntities<VoterListImport>(
            x => x.Id == VoterListImportMockData.BundFutureApprovedGemeindeArneggGuid
                 || x.Id == VoterListImportMockData.BundFutureApprovedStadtGossauGuid,
            x => x.SourceId = Guid.NewGuid().ToString());
    }
}
