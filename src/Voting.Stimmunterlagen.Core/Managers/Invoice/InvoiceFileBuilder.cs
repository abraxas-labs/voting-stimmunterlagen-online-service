// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Core.Managers.Invoice.Mapping;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Managers.Invoice;

public class InvoiceFileBuilder
{
    private readonly CsvService _csvService;
    private readonly InvoiceFileEntriesBuilder _invoiceFileEntriesBuilder;
    private readonly IClock _clock;

    public InvoiceFileBuilder(CsvService csvService, InvoiceFileEntriesBuilder invoiceFileEntriesBuilder, IClock clock)
    {
        _csvService = csvService;
        _invoiceFileEntriesBuilder = invoiceFileEntriesBuilder;
        _clock = clock;
    }

    public async Task<byte[]> BuildInvoiceFile(List<PrintJob> printJobs)
    {
        var timestamp = _clock.UtcNow;
        var entries = printJobs.SelectMany(p => _invoiceFileEntriesBuilder.BuildEntries(p, timestamp));

        return await _csvService.Render(
            entries,
            writer => writer.Context.RegisterClassMap<InvoiceEntryMap>());
    }
}
