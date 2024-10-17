// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Voting.Lib.Rest.Files;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Models;
using Voting.Stimmunterlagen.Models.Request;

namespace Voting.Stimmunterlagen.Controller;

[Route("v1/invoice-export")]
[ApiController]
[AuthorizePrintJobManager]
public class InvoiceExportController : ControllerBase
{
    private readonly InvoiceExportManager _invoiceExportManager;

    public InvoiceExportController(InvoiceExportManager invoiceExportManager)
    {
        _invoiceExportManager = invoiceExportManager;
    }

    [HttpPost]
    public async Task<FileResult> GenerateExport([FromBody] GenerateInvoiceExportRequest request, CancellationToken ct)
    {
        var file = await _invoiceExportManager.GenerateExport(request.ContestId);
        return SingleFileResult.Create(new FileModelWrapper(file), ct);
    }
}
