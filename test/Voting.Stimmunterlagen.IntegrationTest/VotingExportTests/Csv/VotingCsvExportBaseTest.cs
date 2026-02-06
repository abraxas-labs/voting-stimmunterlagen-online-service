// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Models.Request;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.VotingExportTests.Csv;

public abstract class VotingCsvExportBaseTest : BaseWriteableDbRestTest
{
    private const string ExportEndpoint = "v1/voting-export";
    private const string CsvExtension = ".csv";

    protected VotingCsvExportBaseTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    protected abstract string NewRequestExpectedFileName { get; }

    protected virtual HttpClient TestClient => GemeindeArneggClient;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await RunOnDb(async db =>
        {
            var voter1 = await db.Voters.SingleAsync(v => v.PersonId == "1"
                && v.ListId == VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid);

            var voter2 = await db.Voters.SingleAsync(v => v.PersonId == "2"
                && v.ListId == VoterListMockData.BundFutureApprovedGemeindeArneggSwissElectoralRegisterGuid);

            var voterDuplicate = new DomainOfInfluenceVoterDuplicate
            {
                Id = Guid.Parse("0d30142f-5f40-461d-a637-1919018be783"),
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            };

            voter1.VotingCardPrintDisabled = true;
            voter1.VoterDuplicateId = voterDuplicate.Id;
            voter2.VoterDuplicateId = voterDuplicate.Id;

            db.DomainOfInfluenceVoterDuplicates.Add(voterDuplicate);
            db.Voters.UpdateRange(new[] { voter1, voter2 });
            await db.SaveChangesAsync();
        });
    }

    [Fact]
    public virtual Task TestCsv() => TestCsvSnapshot(NewRequest(), NewRequestExpectedFileName);

    protected abstract GenerateVotingExportRequest NewRequest();

    protected async Task TestCsvSnapshot(GenerateVotingExportRequest request, string expectedFileName, string? name = null)
    {
        var response = await AssertStatus(
            () => TestClient.PostAsJsonAsync(ExportEndpoint, request),
            HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be(MimeTypes.CsvMimeType);

        var contentDisposition = response.Content.Headers.ContentDisposition;
        contentDisposition!.FileNameStar.Should().EndWith(CsvExtension);
        contentDisposition.FileNameStar.Should().Be(expectedFileName);
        contentDisposition.DispositionType.Should().Be("attachment");

        var csv = await response.Content.ReadAsStringAsync();
        if (name != null)
        {
            name = "_" + name;
        }

        csv.MatchRawSnapshot("VotingExportTests", "Csv", "_snapshots", GetType().Name + name + ".csv");
    }
}
