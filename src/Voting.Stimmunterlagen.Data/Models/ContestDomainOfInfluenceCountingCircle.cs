// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.Models;

public class ContestDomainOfInfluenceCountingCircle : BaseDomainOfInfluenceCountingCircle
{
    public ContestDomainOfInfluence? DomainOfInfluence { get; set; }

    public ContestCountingCircle? CountingCircle { get; set; }
}
