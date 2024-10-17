// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.EVoting.Models;

public class DomainOfInfluence
{
    public string Bfs { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Logo { get; set; }

    public DomainOfInfluenceVotingCardReturnAddress ReturnAddress { get; set; } = new();

    public DomainOfInfluenceVotingCardPrintData PrintData { get; set; } = new();

    /// <summary>
    /// Gets or sets the attachment stations for all e-voting voter lists of the domain of influence.
    /// </summary>
    public string AttachmentStations { get; set; } = string.Empty;
}
