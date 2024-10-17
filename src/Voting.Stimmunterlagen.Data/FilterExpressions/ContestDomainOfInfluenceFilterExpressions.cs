// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Linq.Expressions;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.FilterExpressions;

public static class ContestDomainOfInfluenceFilterExpressions
{
    public static Expression<Func<ContestDomainOfInfluence, bool>> InEVotingExportFilter => doi => doi.ResponsibleForVotingCards
                    && doi.LogoRef != null
                    && doi.ReturnAddress != null
                    && doi.PrintData != null
                    && doi.PoliticalBusinessPermissionEntries!.Any(e => e.Role == PoliticalBusinessRole.Attendee)
                    && doi.CountingCircles!.Any(cc => cc.CountingCircle!.EVoting);
}
