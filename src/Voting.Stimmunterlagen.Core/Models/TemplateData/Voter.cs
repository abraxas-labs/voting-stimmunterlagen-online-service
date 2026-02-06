// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Text.Json.Serialization;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Models.TemplateData;

public class Voter
{
    public Guid Id { get; set; }

    public Salutation Salutation { get; set; }

    // currently we set all optional fields to empty strings due to dmDoc
    // once the templates are fixed we can adjust.
    public string Title { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("address_line_1")]
    public string AddressLine1 { get; set; } = string.Empty;

    [JsonPropertyName("address_line_2")]
    public string AddressLine2 { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string HouseNumber { get; set; } = string.Empty;

    public string PostOfficeBoxText { get; set; } = string.Empty;

    public string DwellingNumber { get; set; } = string.Empty;

    public string Locality { get; set; } = string.Empty;

    public string Town { get; set; } = string.Empty;

    public int SwissZipCode { get; set; }

    public string ForeignZipCode { get; set; } = string.Empty;

    public string ZipCode => SwissZipCode == 0 ? ForeignZipCode : SwissZipCode.ToString();

    public string? DateOfBirth { get; set; }

    public string? PersonId { get; set; }

    public string? Religion { get; set; }

    public bool? IsHouseholder { get; set; }

    public string? DomainOfInfluenceIdentificationsChurch { get; set; }

    public string? DomainOfInfluenceIdentificationsSchool { get; set; }

    public Country Country { get; set; } = new();

    public string ShipmentNumber { get; set; } = string.Empty;

    public bool SendVotingCardsToDomainOfInfluenceReturnAddress { get; set; }
}
