// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.Models;

public interface IHasContestDomainOfInfluence
{
    ContestDomainOfInfluence? DomainOfInfluence { get; }
}
