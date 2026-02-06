// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Snapper;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.IntegrationTest.TestFiles;
using Voting.Stimmunterlagen.Models.Request;
using Voting.Stimmunterlagen.Models.Response;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.VoterListImportTests;

public class UpdateVoterListImportTest : BaseVoterListImportRestTest
{
    public UpdateVoterListImportTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWorkWithFile()
    {
        var fileName = Ech0045TestFiles.File1Name;
        var attachment = await RunOnDb(db => db.Attachments
            .Include(x => x.DomainOfInfluenceAttachmentCounts)
            .FirstAsync(x => x.Id == AttachmentMockData.BundFutureApprovedBund2Guid));
        attachment.TotalRequiredForVoterListsCount.Should().Be(13);
        attachment.DomainOfInfluenceAttachmentCounts!
            .Single(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .RequiredForVoterListsCount
            .Should()
            .Be(9);

        var request = NewRequest();
        CreateUpdateVoterListImportResponse? responseContent = null;
        await WithRequest(Ech0045TestFiles.GetTestFilePath(fileName), request, async content =>
        {
            using var response = await GemeindeArneggClient.PutAsync(UpdateUrl(VoterListImportMockData.BundFutureApprovedGemeindeArneggGuid), content);
            response.EnsureSuccessStatusCode();
            responseContent = await DeserializeHttpResponse(response);
            responseContent.ImportId.Should().NotBeEmpty();
            foreach (var voterList in responseContent.VoterLists!)
            {
                voterList.Id.Should().NotBeEmpty();
                voterList.Id = Guid.Empty;
            }
        });
        responseContent.MatchSnapshot("response");

        var voterListImport = await GetByName("my-file-001");
        voterListImport.Source.Should().Be(VoterListSource.ManualEch45Upload);
        voterListImport.SourceId.Should().Be(fileName);
        voterListImport.SourceId = string.Empty;
        voterListImport.MatchSnapshot("data");

        // Start: 6 + 7 from the Import (3 Swiss, 4 EVoting)
        // End: 6 + 4 from the Import (4 Swiss)
        attachment = await RunOnDb(db => db.Attachments
            .Include(x => x.DomainOfInfluenceAttachmentCounts)
            .FirstAsync(x => x.Id == AttachmentMockData.BundFutureApprovedBund2Guid));
        attachment.TotalRequiredForVoterListsCount.Should().Be(10);
        attachment.DomainOfInfluenceAttachmentCounts!
            .Single(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .RequiredForVoterListsCount
            .Should()
            .Be(6);

        // test if existing political business entries on voter list got applied to the new voter list
        (await RunOnDb(db => db.VoterLists.AnyAsync(vl => vl.Id == VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid)))
            .Should().BeFalse();

        var newSwissVoterList = await RunOnDb(db => db.VoterLists
            .Include(vl => vl.PoliticalBusinessEntries!.OrderBy(x => x.Id))
            .SingleOrDefaultAsync(vl => vl.ImportId == VoterListImportMockData.BundFutureApprovedGemeindeArneggGuid && vl.VotingCardType == VotingCardType.Swiss));
        newSwissVoterList!.PoliticalBusinessEntries!.Any().Should().BeTrue();
    }

    [Fact]
    public async Task ShouldWorkWithoutFile()
    {
        var request = NewRequest();
        CreateUpdateVoterListImportResponse? responseContent = null;

        await WithRequest(null, request, async content =>
        {
            using var response = await GemeindeArneggClient.PutAsync(UpdateUrl(VoterListImportMockData.BundFutureApprovedGemeindeArneggGuid), content);
            response.EnsureSuccessStatusCode();
            responseContent = await DeserializeHttpResponse(response);
        });

        responseContent.Should().NotBeNull();
        responseContent!.ShouldMatchSnapshot();

        var voterListImport = await GetByName("my-file-001");
        voterListImport.Source.Should().Be(VoterListSource.ManualEch45Upload);
        voterListImport.SourceId.Should().Be("arnegg-ech-0045-swiss.xml");
        voterListImport.DomainOfInfluenceId.Should().Be(DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid);

        voterListImport.VoterLists.Should().HaveCount(2);
        var swissVoterList = voterListImport.VoterLists!.FirstOrDefault(x => x.VotingCardType == VotingCardType.Swiss);
        swissVoterList.Should().NotBeNull();
        swissVoterList!.DomainOfInfluenceId.Should().Be(DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid);
        swissVoterList.NumberOfVoters.Should().Be(3);
        swissVoterList.Voters!.Count.Should().Be(3);

        var voter = swissVoterList.Voters.First();
        voter.FirstName.Should().Be("Marco");
        voter.LastName.Should().Be("Koch");
        voter.VotingCardType.Should().Be(VotingCardType.Swiss);

        var attachment = await RunOnDb(db => db.Attachments
            .Include(x => x.DomainOfInfluenceAttachmentCounts)
            .FirstAsync(x => x.Id == AttachmentMockData.BundFutureApprovedBund2Guid));
        attachment.TotalRequiredForVoterListsCount.Should().Be(13);
        attachment.DomainOfInfluenceAttachmentCounts!
            .Single(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .RequiredForVoterListsCount
            .Should()
            .Be(9);
    }

    [Fact]
    public async Task ShouldUpdateDomainOfInfluenceLastVoterUpdate()
    {
        var doi = await RunOnDb(db => db.ContestDomainOfInfluences.FirstAsync(doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid));
        doi.LastVoterUpdate.Should().BeNull();

        await WithRequest(Ech0045TestFiles.GetTestFilePath(Ech0045TestFiles.File1Name), NewRequest(), async content =>
        {
            using var response = await GemeindeArneggClient.PutAsync(UpdateUrl(VoterListImportMockData.BundFutureApprovedGemeindeArneggGuid), content);
        });

        doi = await RunOnDb(db => db.ContestDomainOfInfluences.FirstAsync(doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid));
        doi.LastVoterUpdate.Should().Be(MockedClock.GetDate());
    }

    [Fact]
    public async Task ShouldBeNoSuccessWithInternalDuplicates()
    {
        var request = NewRequest();
        CreateUpdateVoterListImportResponse? responseContent = null;

        await WithRequest(Ech0045TestFiles.FileDuplicates, request, async content =>
        {
            using var response = await GemeindeArneggClient.PutAsync(UpdateUrl(VoterListImportMockData.BundFutureApprovedGemeindeArneggGuid), content);
            response.EnsureSuccessStatusCode();
            responseContent = await DeserializeHttpResponse(response);
            responseContent.ImportId.Should().NotBeEmpty();
            responseContent.ImportId = Guid.Empty;
            foreach (var voterList in responseContent.VoterLists!)
            {
                voterList.Id.Should().NotBeEmpty();
                voterList.Id = Guid.Empty;
            }
        });
        responseContent.MatchSnapshot("response");
        (await ExistsByName("my-file-001")).Should().BeFalse();
    }

    [Fact]
    public async Task ShouldBeSuccessWithExternalDuplicatesAndEnabledMultipleElectoralRegisters()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            x => x.ElectoralRegisterMultipleEnabled = true);

        await ModifyDbEntities<Voter>(
            x => x.PersonId == "2",
            x =>
            {
                x.FirstName = "Dirk";
                x.LastName = "Berg";
                x.DateOfBirth = "1971-05";
                x.Street = "Mattenstrasse";
                x.HouseNumber = "71";
            });

        var existingVoterListId = VoterListMockData.BundFutureApprovedGemeindeArneggSwissElectoralRegisterGuid;
        (await RunOnDb(db => db.VoterLists.SingleAsync(vl => vl.Id == existingVoterListId)))
            .CountOfVotingCards.Should().Be(2);

        var request = NewRequest();
        CreateUpdateVoterListImportResponse? responseContent = null;

        await WithRequest(Ech0045TestFiles.File1, request, async content =>
        {
            using var response = await GemeindeArneggClient.PutAsync(UpdateUrl(VoterListImportMockData.BundFutureApprovedGemeindeArneggGuid), content);
            response.EnsureSuccessStatusCode();
            responseContent = await DeserializeHttpResponse(response);
            responseContent.Error.Should().BeNull();

            responseContent.ImportId.Should().NotBeEmpty();
            responseContent.ImportId = Guid.Empty;
            foreach (var voterList in responseContent.VoterLists!)
            {
                voterList.Id.Should().NotBeEmpty();
                voterList.Id = Guid.Empty;
            }
        });

        responseContent.MatchSnapshot("response");
        (await ExistsByName("my-file-001")).Should().BeTrue();

        // Existing voter list should reduce the number of voting cards
        // (because the newly added voter is considered for print and not the existing one).
        (await RunOnDb(db => db.VoterLists.SingleAsync(vl => vl.Id == existingVoterListId)))
            .CountOfVotingCards.Should().Be(1);

        var voterDuplicate = await RunOnDb(db => db.DomainOfInfluenceVoterDuplicates
            .Include(d => d.Voters)
            .SingleAsync(d => d.Voters!.Any(v => v.ListId == existingVoterListId)));

        voterDuplicate.FirstName.Should().Be("Dirk");
        voterDuplicate.LastName.Should().Be("Berg");
        voterDuplicate.DateOfBirth.Should().Be("1971-05");

        // The new voter and the existing voter should be included.
        voterDuplicate.Voters.Should().HaveCount(2);

        voterDuplicate.Voters!.Single(v => v.ListId == existingVoterListId).VotingCardPrintDisabled.Should().BeTrue();
        voterDuplicate.Voters!.Single(v => v.ListId != existingVoterListId).VotingCardPrintDisabled.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldBeNoSuccessWithExternalDuplicatesAndDisabledMultipleElectoralRegisters()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            x => x.ElectoralRegisterMultipleEnabled = false);

        await ModifyDbEntities<Voter>(
            x => x.PersonId == "2",
            x =>
            {
                x.FirstName = "Dirk";
                x.LastName = "Berg";
                x.DateOfBirth = "1971-05";
                x.Street = "Mattenstrasse";
                x.HouseNumber = "71";
            });

        var request = NewRequest();
        CreateUpdateVoterListImportResponse? responseContent = null;

        await WithRequest(Ech0045TestFiles.File1, request, async content =>
        {
            using var response = await GemeindeArneggClient.PutAsync(UpdateUrl(VoterListImportMockData.BundFutureApprovedGemeindeArneggGuid), content);
            response.EnsureSuccessStatusCode();
            responseContent = await DeserializeHttpResponse(response);
            responseContent.Error.Should().NotBeNull();
            responseContent.Error!.VoterDuplicates.Should().HaveCount(1);
            responseContent.Error.VoterDuplicatesCount.Should().Be(1);

            responseContent.ImportId.Should().NotBeEmpty();
            responseContent.ImportId = Guid.Empty;
            foreach (var voterList in responseContent.VoterLists!)
            {
                voterList.Id.Should().NotBeEmpty();
                voterList.Id = Guid.Empty;
            }
        });

        responseContent.MatchSnapshot("response");
        (await ExistsByName("my-file-001")).Should().BeFalse();
    }

    [Fact]
    public Task ShouldThrowOtherSource()
    {
        var request = NewRequest();
        return WithRequest(Ech0045TestFiles.File1, request, async content =>
        {
            using var response = await GemeindeArneggClient.PutAsync(UpdateUrl(VoterListImportMockData.BundFutureApprovedGemeindeArneggElectoralRegisterGuid), content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public Task ShouldThrowNotDoiManager()
    {
        var request = NewRequest();
        return WithRequest(null, request, async content =>
        {
            using var response = await StadtGossauClient.PutAsync(UpdateUrl(VoterListImportMockData.BundFutureApprovedGemeindeArneggGuid), content);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public Task ShouldThrowInvalidFile()
    {
        var request = NewRequest();
        return WithRequest(Ech0045TestFiles.File2Invalid, request, async content =>
        {
            using var response = await GemeindeArneggClient.PutAsync(UpdateUrl(VoterListImportMockData.BundFutureApprovedGemeindeArneggGuid), content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public Task ShouldThrowNoRequest()
    {
        return WithRequest(null, null, async content =>
        {
            using var response = await GemeindeArneggClient.PutAsync(UpdateUrl(VoterListImportMockData.BundFutureApprovedGemeindeArneggGuid), content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public Task ShouldThrowContestLocked()
    {
        var req = new UpdateVoterListImportRequest
        {
            Name = "my-file-001",
            LastUpdate = MockedClock.GetDate(-1),
        };
        return WithRequest(Ech0045TestFiles.File1, req, async content =>
        {
            using var response = await GemeindeArneggClient.PutAsync(UpdateUrl(VoterListImportMockData.BundArchivedGemeindeArneggGuid), content);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public async Task ShouldThrowIfPastGenerateVotingCardsDeadline()
    {
        await SetContestBundFutureApprovedToPastGenerateVotingCardsDeadline();
        var req = NewRequest();
        await WithRequest(Ech0045TestFiles.File1, req, async content =>
        {
            using var response = await GemeindeArneggClient.PutAsync(UpdateUrl(VoterListImportMockData.BundFutureApprovedGemeindeArneggGuid), content);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        });
    }

    private string UpdateUrl(Guid id) => $"{Url}/{id}";

    private UpdateVoterListImportRequest NewRequest(Action<UpdateVoterListImportRequest>? customizer = null)
    {
        var request = new UpdateVoterListImportRequest()
        {
            Name = "my-file-001",
            LastUpdate = MockedClock.GetDate(-1),
        };
        customizer?.Invoke(request);
        return request;
    }
}
