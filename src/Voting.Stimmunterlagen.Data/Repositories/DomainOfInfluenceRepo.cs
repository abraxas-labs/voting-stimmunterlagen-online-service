// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Database.Repositories;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.Repositories;

public class DomainOfInfluenceRepo : DbRepository<DataContext, DomainOfInfluence>
{
    public DomainOfInfluenceRepo(DataContext context)
        : base(context)
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.", Justification = "Referencing hardened inerpolated string parameters.")]
    public async Task<DomainOfInfluenceCanton> GetRootCanton(Guid domainOfInfluenceId)
    {
        var idColumnName = GetDelimitedColumnName(x => x.Id);
        var parentIdColumnName = GetDelimitedColumnName(x => x.ParentId);
        var cantonColumnName = GetDelimitedColumnName(x => x.Canton);

        return await Context.DomainOfInfluences.FromSqlRaw(
                $@"
                WITH RECURSIVE parents_or_self AS (
                    SELECT {idColumnName}, {parentIdColumnName}, {cantonColumnName}
                    FROM {DelimitedSchemaAndTableName}
                    WHERE {idColumnName} = {{0}}
                    UNION
                    SELECT x.{idColumnName}, x.{parentIdColumnName}, x.{cantonColumnName}
                    FROM {DelimitedSchemaAndTableName} x
                    JOIN parents_or_self p ON x.{idColumnName} = p.{parentIdColumnName}
                )
                SELECT * FROM parents_or_self
                WHERE {parentIdColumnName} IS NULL",
                domainOfInfluenceId)
            .Select(doi => doi.Canton)
            .FirstOrDefaultAsync();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.", Justification = "Referencing hardened inerpolated string parameters.")]
    public async Task<List<Guid>> GetHierarchicalParentIds(Guid domainOfInfluenceId)
    {
        var idColumnName = GetDelimitedColumnName(x => x.Id);
        var parentIdColumnName = GetDelimitedColumnName(x => x.ParentId);

        var ids = await Context.DomainOfInfluences.FromSqlRaw(
                $@"
                WITH RECURSIVE parents_or_self AS (
                    SELECT {idColumnName}, {parentIdColumnName}
                    FROM {DelimitedSchemaAndTableName}
                    WHERE {idColumnName} = {{0}}
                    UNION
                    SELECT x.{idColumnName}, x.{parentIdColumnName}
                    FROM {DelimitedSchemaAndTableName} x
                    JOIN parents_or_self p ON x.{idColumnName} = p.{parentIdColumnName}
                )
                SELECT * FROM parents_or_self",
                domainOfInfluenceId)
            .Select(doi => doi.Id)
            .ToListAsync();
        ids.Remove(domainOfInfluenceId);
        return ids;
    }
}
