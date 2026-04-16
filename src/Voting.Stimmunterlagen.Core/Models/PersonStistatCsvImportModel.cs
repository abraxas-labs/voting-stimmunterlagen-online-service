// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using CsvHelper.Configuration.Attributes;

namespace Voting.Stimmunterlagen.Core.Models;

/// <summary>
/// The STISTAT e-voting person domain model.
/// </summary>
public class PersonStistatCsvImportModel
{
    /// <summary>
    /// Gets or sets the municipality id, i.e. '9001'.
    /// </summary>
    [Name("BFS")]
    public int MunicipalityId { get; set; }

    /// <summary>
    /// Gets or sets the source system id, i.e. '466312'.
    /// </summary>
    [Name("Personennummer")]
    public string? SourceSystemId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the person is an e-voter, i.e. 'Ja' / 'Nein'.
    /// </summary>
    [Name("E-Voter")]
    public string EVoting { get; set; } = string.Empty;
}
