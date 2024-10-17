// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.Extensions;

public static class DomainOfInfluenceTypeExtensions
{
    public static bool IsCommunal(this DomainOfInfluenceType doiType) => doiType >= DomainOfInfluenceType.Mu;
}
