// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using CsvHelper.Configuration.Attributes;

namespace Voting.Stimmunterlagen.Core.Models.Invoice;

public class InvoiceFileEntry
{
    [Index(1)]
    [Name("WORKDATE")]
    public DateTime CurrentDate { get; set; }

    [Index(3)]
    [Name("/PPA/LSTNR")]
    public string SapMaterialNumber { get; set; } = string.Empty;

    [Index(4)]
    [Name("/PPA/MENGE")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the "SAP Kundenauftragsnummer".
    /// This is managed by the <see cref="Data.Models.DomainOfInfluence"/> in VOTING Basis.
    /// </summary>
    [Index(10)]
    [Name("RKDAUF")]
    public string SapCustomerNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the "SAP Auftragsposition".
    /// </summary>
    [Index(11)]
    [Name("RKDPOS")]
    public string SapOrderPosition { get; set; } = string.Empty;

    [Index(22)]
    [Name("LANGTEXT")]
    public string Comment { get; set; } = string.Empty;

    [Index(32)]
    [Name("/PPA/MAKTX2")]
    public string SapMaterialText { get; set; } = string.Empty;
}
