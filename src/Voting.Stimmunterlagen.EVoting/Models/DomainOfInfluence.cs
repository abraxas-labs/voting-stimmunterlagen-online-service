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

    public bool StistatMunicipality { get; set; }
}
