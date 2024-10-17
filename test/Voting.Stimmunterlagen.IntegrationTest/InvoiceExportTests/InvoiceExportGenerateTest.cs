// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Models.Request;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.InvoiceExportTests;

public class InvoiceExportGenerateTest : BaseWriteableDbRestTest
{
    private const string Url = "v1/invoice-export";

    private const string CsvExtension = ".csv";

    public InvoiceExportGenerateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task TestExport()
    {
        // only attachments which are delivered and required for voter lists are included in the invoice.
        await ModifyDbEntities<Attachment>(
            x => x.DomainOfInfluence!.ContestId == ContestMockData.BundFutureApprovedGuid,
            x => x.State = AttachmentState.Delivered);

        await ModifyDbEntities<DomainOfInfluenceAttachmentCount>(
            x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            x => x.RequiredForVoterListsCount = 15);

        var response = await AssertStatus(
            () => AbraxasPrintJobManagerClient.PostAsJsonAsync(Url, new GenerateInvoiceExportRequest
            {
                ContestId = ContestMockData.BundFutureApprovedGuid,
            }),
            HttpStatusCode.OK);

        await CsvShouldMatchSnapshot(response, "Invoice_2020_01_12.csv", "InvoiceTest");
    }

    [Fact]
    public async Task ShouldThrowAsElectionAdmin()
    {
        await AssertStatus(
            () => AbraxasElectionAdminClient.PostAsJsonAsync(Url, new GenerateInvoiceExportRequest
            {
                ContestId = ContestMockData.BundFutureApprovedGuid,
            }),
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShouldThrowIfExternalPrintingCenter()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.ContestId == ContestMockData.BundFutureApprovedGuid,
            x => x.ExternalPrintingCenter = true);
        await AssertStatus(
            () => AbraxasPrintJobManagerClient.PostAsJsonAsync(Url, new GenerateInvoiceExportRequest
            {
                ContestId = ContestMockData.BundFutureApprovedGuid,
            }),
            HttpStatusCode.BadRequest);
    }

    private async Task CsvShouldMatchSnapshot(HttpResponseMessage response, string expectedFileName, string name)
    {
        response.Content.Headers.ContentType!.MediaType.Should().Be(MimeTypes.CsvMimeType);
        var csvString = await response.Content.ReadAsStringAsync();

        var contentDisposition = response.Content.Headers.ContentDisposition;
        contentDisposition!.FileNameStar.Should().EndWith(CsvExtension);
        contentDisposition.FileNameStar.Should().Be(expectedFileName);
        contentDisposition.DispositionType.Should().Be("attachment");

        csvString.MatchRawSnapshot("InvoiceExportTests", "Csv", "_snapshots", GetType().Name + "_" + name + CsvExtension);
    }
}
