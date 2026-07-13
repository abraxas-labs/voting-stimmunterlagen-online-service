// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.Models;

public class DomainOfInfluenceCountingCircle : BaseDomainOfInfluenceCountingCircle
{
    public DomainOfInfluence? DomainOfInfluence { get; set; }

    public CountingCircle? CountingCircle { get; set; }
}
