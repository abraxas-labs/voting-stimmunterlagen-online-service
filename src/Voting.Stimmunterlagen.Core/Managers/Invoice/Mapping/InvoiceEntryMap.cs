// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CsvHelper.Configuration;
using Voting.Stimmunterlagen.Core.Models.Invoice;

namespace Voting.Stimmunterlagen.Core.Managers.Invoice.Mapping;

public class InvoiceEntryMap : ClassMap<InvoiceFileEntry>
{
    private static readonly string[] ColumnNames = new string[]
    {
        "PERNR",
        "WORKDATE",
        "SKOSTL",
        "/PPA/LSTNR",
        "/PPA/MENGE",
        "RKOSTL",
        "RPROJ",
        "RAUFNR",
        "RAUFPL",
        "RAPLZL",
        "RKDAUF",
        "RKDPOS",
        "EXTSYSTEM",
        "EXTAPPLICATION",
        "EXTDOCUMENTNO",
        "AWART",
        "WAERS",
        "BEGUZ",
        "ENDUZ",
        "/PPA/ACTVY",
        "/PPA/PLOSP",
        "/PPA/TICNR",
        "LANGTEXT",
        "/PPA/TAXKM",
        "/PPA/NFAKT1",
        "/PPA/MAFAZ1",
        "/PPA/MAKTX1",
        "/PPA/MENGE1",
        "/PPA/PREIS1",
        "/PPA/WERT1",
        "/PPA/WAERS1",
        "/PPA/MAFAZ2",
        "/PPA/MAKTX2",
        "/PPA/PREIS2",
        "/PPA/WERT2",
        "/PPA/WAERS2",
        "/PPA/ANSNR",
    };

    private static readonly Dictionary<string, string> ConstantValueByColumnName = new()
    {
        { "/PPA/MAFAZ2", "x" },
    };

    public InvoiceEntryMap()
    {
        AutoMap(CultureInfo.InvariantCulture);

        Map(m => m.CurrentDate).TypeConverterOption.Format("dd.MM.yyyy");

        // the mapped column names which where included by auto mapping the InvoiceFileEntry
        var mappedColumnNames = MemberMaps.Select(m => m.Data.Names.Single()).ToHashSet();

        for (var i = 0; i < ColumnNames.Length; i++)
        {
            var columnName = ColumnNames[i];

            if (mappedColumnNames.Contains(columnName))
            {
                continue;
            }

            // constant value is necessary,
            // otherwise it will not include the delimiter for the column value and the csv will get rendered wrong.
            Map().Index(i)
                .Name(ColumnNames[i])
                .Constant(ConstantValueByColumnName.GetValueOrDefault(columnName) ?? string.Empty);
        }
    }
}
