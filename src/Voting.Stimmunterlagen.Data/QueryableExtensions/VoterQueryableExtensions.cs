// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.QueryableExtensions;

public static partial class VoterQueryableExtensions
{
    public static IQueryable<Voter> WhereBelongToDomainOfInfluence(this IQueryable<Voter> q, Guid doiId)
        => q.Where(x => x.List!.DomainOfInfluenceId == doiId || x.ManualJob!.Layout.DomainOfInfluenceId == doiId);

    public static IQueryable<Voter> WhereBelongToDomainOfInfluenceOnlyVoterList(this IQueryable<Voter> q, Guid doiId)
    => q.Where(x => x.List!.DomainOfInfluenceId == doiId);

    public static IQueryable<Voter> WhereVotingCardType(this IQueryable<Voter> q, VotingCardType vcType)
        => q.Where(x => x.VotingCardType == vcType);

    public static IQueryable<Voter> WhereVotingCardPrintEnabled(this IQueryable<Voter> q)
        => q.Where(x => !x.VotingCardPrintDisabled);
}
