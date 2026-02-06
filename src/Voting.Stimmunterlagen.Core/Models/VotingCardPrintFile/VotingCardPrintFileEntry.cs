// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using CsvHelper.Configuration.Attributes;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Models.VotingCardPrintFile;

public class VotingCardPrintFileEntry
{
    [Name("DOCID")]
    public string DocId { get; set; } = string.Empty;

    [Name("BFSNUMMER")]
    public string Bfs { get; set; } = string.Empty;

    [Name("FORMULAR")]
    public string Form { get; set; } = string.Empty;

    [Name("DUPLEX-SIMPLEX")]
    public bool IsDuplexPrint { get; set; }

    [Name("ADRESSBUENDELUNG1")]
    public string PersonId { get; set; } = string.Empty;

    [Name("ADRESSBUENDELUNG2")]
    public string FullName { get; set; } = string.Empty;

    [Name("SENDUNGSSORT")]
    public string SendSort { get; set; } = string.Empty;

    [Name("KUNDE_UNTERTEILUNG1")]
    public string CustomerSubdivision { get; set; } = string.Empty;

    [Name("TOTALPAGES")]
    public int TotalPages { get; set; }

    [Name("PAGEFROM")]
    public int PageFrom { get; set; }

    [Name("PAGETO")]
    public int PageTo { get; set; }

    [Name("DRUCKEN-PP")]
    public bool PrintPP { get; set; }

    [Name("BEILAGE")]
    public string AttachmentStations { get; set; } = string.Empty;

    [Name("INSTREAMBEILAGE1")]
    public string InStreamAttachment1 { get; set; } = string.Empty;

    [Name("INSTREAMBEILAGE2")]
    public string InStreamAttachment2 { get; set; } = string.Empty;

    [Name("INSTREAMBEILAGE3")]
    public string InStreamAttachment3 { get; set; } = string.Empty;

    [Name("ADRESSLINE1")]
    public string AddressLine1 { get; set; } = string.Empty;

    [Name("ADRESSLINE2")]
    public string AddressLine2 { get; set; } = string.Empty;

    [Name("ADRESSLINE3")]
    public string AddressLine3 { get; set; } = string.Empty;

    [Name("ADRESSLINE4")]
    public string AddressLine4 { get; set; } = string.Empty;

    [Name("ADRESSLINE5")]
    public string AddressLine5 { get; set; } = string.Empty;

    [Name("ADRESSLINE6")]
    public string AddressLine6 { get; set; } = string.Empty;

    [Name("COUNTRY")]
    public string CountryIso2 { get; set; } = string.Empty;

    [Name("VERSAND_HINWEG")]
    public VotingCardShippingFranking ShippingAway { get; set; }

    [Name("VERSAND_ART")]
    public string ShippingMethodCode { get; set; } = string.Empty;

    [Name("VERSAND_RUECKWEG")]
    public VotingCardShippingFranking ShippingReturn { get; set; }

    [Name("SPRACHE")]
    public string Language { get; set; } = string.Empty;

    [Name("ZUSATZ_BARCODE")]
    public string AdditionalBarcode { get; set; } = string.Empty;

    [Name("AuftrP")]
    public string ContestOrderNumber { get; set; } = string.Empty;

    [Name("RELIGION")]
    public string ReligionCode { get; set; } = string.Empty;
}
