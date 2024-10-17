// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace Voting.Stimmunterlagen.Core.Utils;

public class CsvService
{
    private static readonly CsvConfiguration CsvConfiguration = NewCsvConfig();

    public async Task<byte[]> Render<TRow>(IEnumerable<TRow> records, Action<IWriter>? configure = null, CancellationToken ct = default)
    {
        using var ms = new MemoryStream();

        // use utf8 with bom (excel requires bom)
        using (var streamWriter = new StreamWriter(ms, Encoding.UTF8))
        {
            using var csvWriter = new CsvWriter(streamWriter, CsvConfiguration);
            configure?.Invoke(csvWriter);
            await csvWriter.WriteRecordsAsync(records, ct);
        }

        return ms.ToArray();
    }

    private static CsvConfiguration NewCsvConfig() =>
        new(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
        };
}
