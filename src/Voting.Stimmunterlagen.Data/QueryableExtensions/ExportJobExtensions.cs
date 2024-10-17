// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.QueryableExtensions;

public static class ExportJobExtensions
{
    public static IQueryable<TExportJob> WhereInState<TExportJob>(
        this IQueryable<TExportJob> q,
        params ExportJobState[] states)
        where TExportJob : ExportJobBase
    {
        return q.Where(x => states.Contains(x.State));
    }
}
