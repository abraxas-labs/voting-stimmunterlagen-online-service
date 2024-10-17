// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
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

    [Fact]
    public virtual Task TestCsv() => TestCsvSnapshot(NewRequest(), NewRequestExpectedFileName);

    protected abstract GenerateVotingExportRequest NewRequest();

    private async Task TestCsvSnapshot(GenerateVotingExportRequest request, string expectedFileName, string? name = null)
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
