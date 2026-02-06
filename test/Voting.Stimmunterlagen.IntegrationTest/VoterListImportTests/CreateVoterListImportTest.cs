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

public class CreateVoterListImportTest : BaseVoterListImportRestTest
{
    public CreateVoterListImportTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var fileName = Ech0045TestFiles.File1Name;
        var importCount = await RunOnDb(db => db.VoterListImports
            .Where(i => i.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .CountAsync());

        var request = NewRequest();
        CreateUpdateVoterListImportResponse? responseContent = null;

        await WithRequest(Ech0045TestFiles.GetTestFilePath(fileName), request, async content =>
        {
            using var response = await GemeindeArneggClient.PostAsync(Url, content);
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

        var voterListImport = await GetByName("my-file-001");
        voterListImport.Id = Guid.Empty;

        voterListImport.SourceId.Should().Be(fileName);
        voterListImport.SourceId = string.Empty;
        voterListImport.MatchSnapshot("data");

        // No import should be deleted with a file import instead of a electoral register import.
        (await RunOnDb(db => db.VoterListImports
            .Where(i => i.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .CountAsync())).Should().Be(importCount + 1);
    }

    [Fact]
    public async Task ShouldWorkDomainOfInfluenceImport()
    {
        var fileName = Ech0045TestFiles.File3NameDomainOfInfluence;
        var importCount = await RunOnDb(db => db.VoterListImports
            .Where(i => i.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .CountAsync());

        var request = NewRequest();
        CreateUpdateVoterListImportResponse? responseContent = null;

        await WithRequest(Ech0045TestFiles.GetTestFilePath(fileName), request, async content =>
        {
            using var response = await GemeindeArneggClient.PostAsync(Url, content);
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

        var voterListImport = await GetByName("my-file-001");
        voterListImport.Id = Guid.Empty;

        voterListImport.SourceId.Should().Be(fileName);
        voterListImport.SourceId = string.Empty;
        voterListImport.MatchSnapshot("data");

        // No import should be deleted with a file import instead of a electoral register import.
        (await RunOnDb(db => db.VoterListImports
            .Where(i => i.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .CountAsync())).Should().Be(importCount + 1);
    }

    [Fact]
    public async Task ShouldWorkWithMixedEVoting()
    {
        await ModifyDbEntities<ContestCountingCircle>(
            x => x.Id == CountingCircleMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            x => x.EVoting = true);

        var request = NewRequest();
        var fileName = Ech0045TestFiles.File4MixedEVotingName;
        CreateUpdateVoterListImportResponse? responseContent = null;

        await WithRequest(Ech0045TestFiles.GetTestFilePath(fileName), request, async content =>
        {
            using var response = await GemeindeArneggClient.PostAsync(Url, content);
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

        var voterListImport = await GetByName("my-file-001");
        voterListImport.Id = Guid.Empty;

        voterListImport.SourceId.Should().Be(fileName);
        voterListImport.SourceId = string.Empty;
        voterListImport.MatchSnapshot("data");
    }

    [Fact]
    public async Task ShouldWorkWithEVotingVotersAndEVotingDisabled()
    {
        var request = NewRequest();
        var fileName = Ech0045TestFiles.File4MixedEVotingName;
        CreateUpdateVoterListImportResponse? responseContent = null;

        await WithRequest(Ech0045TestFiles.GetTestFilePath(fileName), request, async content =>
        {
            using var response = await GemeindeArneggClient.PostAsync(Url, content);
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

        var voterListImport = await GetByName("my-file-001");
        voterListImport.Id = Guid.Empty;

        voterListImport.SourceId.Should().Be(fileName);
        voterListImport.SourceId = string.Empty;
        voterListImport.MatchSnapshot("data");
    }

    [Fact]
    public async Task ShouldWorkWithMissingExtensions()
    {
        var request = NewRequest();
        var fileName = Ech0045TestFiles.File5MissingExtensionsName;
        CreateUpdateVoterListImportResponse? responseContent = null;

        await WithRequest(Ech0045TestFiles.GetTestFilePath(fileName), request, async content =>
        {
            using var response = await GemeindeArneggClient.PostAsync(Url, content);
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

        var voterListImport = await GetByName("my-file-001");
        voterListImport.Id = Guid.Empty;

        voterListImport.SourceId.Should().Be(fileName);
        voterListImport.SourceId = string.Empty;
        voterListImport.MatchSnapshot("data");
    }

    [Fact]
    public async Task ShouldWorkWithShippingVotingCardsToDeliveryAddress()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            x => x.PrintData!.ShippingVotingCardsToDeliveryAddress = true);

        var request = NewRequest();
        await WithRequest(Ech0045TestFiles.GetTestFilePath(Ech0045TestFiles.File1Name), request, async content =>
        {
            using var response = await GemeindeArneggClient.PostAsync(Url, content);
            response.EnsureSuccessStatusCode();
        });

        var voterListImport = await GetByName("my-file-001");
        voterListImport.Id = Guid.Empty;
        voterListImport.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldUpdateDomainOfInfluenceLastVoterUpdate()
    {
        var doi = await RunOnDb(db => db.ContestDomainOfInfluences.FirstAsync(doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid));
        doi.LastVoterUpdate.Should().BeNull();

        await WithRequest(Ech0045TestFiles.GetTestFilePath(Ech0045TestFiles.File1Name), NewRequest(), async content =>
        {
            using var response = await GemeindeArneggClient.PostAsync(Url, content);
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
            using var response = await GemeindeArneggClient.PostAsync(Url, content);
            response.EnsureSuccessStatusCode();
            responseContent = await DeserializeHttpResponse(response);
        });
        responseContent.MatchSnapshot("response");
        (await ExistsByName("my-file-001")).Should().BeFalse();
    }

    [Fact]
    public async Task ShouldBeNoSuccessWithExternalDuplicatesAndDisabledMultipleElectoralRegisters()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            x => x.ElectoralRegisterMultipleEnabled = false);

        await ModifyDbEntities<Voter>(
            x => x.PersonId == "1",
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
            using var response = await GemeindeArneggClient.PostAsync(Url, content);
            response.EnsureSuccessStatusCode();
            responseContent = await DeserializeHttpResponse(response);
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
            x => x.PersonId == "1",
            x =>
            {
                x.FirstName = "Dirk";
                x.LastName = "Berg";
                x.DateOfBirth = "1971-05";
                x.Street = "Mattenstrasse";
                x.HouseNumber = "71";
            });

        var existingVoterListId = VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid;
        (await RunOnDb(db => db.VoterLists.SingleAsync(vl => vl.Id == existingVoterListId)))
            .CountOfVotingCards.Should().Be(3);

        var request = NewRequest();
        CreateUpdateVoterListImportResponse? responseContent = null;

        await WithRequest(Ech0045TestFiles.File1, request, async content =>
        {
            using var response = await GemeindeArneggClient.PostAsync(Url, content);
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
            .CountOfVotingCards.Should().Be(2);

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
    public Task ShouldThrowNotDoiManager()
    {
        var request = NewRequest();
        return WithRequest(Ech0045TestFiles.GetTestFilePath(Ech0045TestFiles.File1Name), request, async content =>
        {
            using var response = await StadtGossauClient.PostAsync(Url, content);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public Task ShouldThrowInvalidFile()
    {
        var request = NewRequest();
        return WithRequest(Ech0045TestFiles.File2Invalid, request, async content =>
        {
            using var response = await GemeindeArneggClient.PostAsync(Url, content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public Task ShouldThrowNoFile()
    {
        var request = NewRequest();
        return WithRequest(null, request, async content =>
        {
            using var response = await GemeindeArneggClient.PostAsync(Url, content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public Task ShouldThrowNoRequest()
    {
        return WithRequest(Ech0045TestFiles.File1, null, async content =>
        {
            using var response = await GemeindeArneggClient.PostAsync(Url, content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public Task ShouldThrowContestLocked()
    {
        var req = new CreateVoterListImportRequest
        {
            Name = "my-file-001",
            LastUpdate = MockedClock.GetDate(-1),
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggGuid,
        };
        return WithRequest(Ech0045TestFiles.File1, req, async content =>
        {
            using var response = await GemeindeArneggClient.PostAsync(Url, content);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public async Task ShouldThrowManualUploadNotAllowed()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            d => d.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            d => d.AllowManualVoterListUpload = false);

        var req = NewRequest();
        await WithRequest(Ech0045TestFiles.File1, req, async content =>
        {
            using var response = await GemeindeArneggClient.PostAsync(Url, content);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        });
    }

    [Fact]
    public async Task ShouldThrowIfPastGenerateVotingCardsDeadline()
    {
        await SetContestBundFutureApprovedToPastGenerateVotingCardsDeadline();
        var req = NewRequest();
        await WithRequest(Ech0045TestFiles.File1, req, async content =>
        {
            using var response = await StadtGossauClient.PostAsync(Url, content);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        });
    }

    private CreateVoterListImportRequest NewRequest()
    {
        return new CreateVoterListImportRequest()
        {
            Name = "my-file-001",
            LastUpdate = MockedClock.GetDate(-1),
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        };
    }
}
