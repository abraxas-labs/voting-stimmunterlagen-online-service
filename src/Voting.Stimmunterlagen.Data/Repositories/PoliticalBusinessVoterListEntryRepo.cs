// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Database.Repositories;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.Repositories;

public class PoliticalBusinessVoterListEntryRepo : DbRepository<DataContext, PoliticalBusinessVoterListEntry>
{
    public PoliticalBusinessVoterListEntryRepo(DataContext context)
        : base(context)
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.", Justification = "Referencing hardened inerpolated string parameters.")]
    public Task DeleteAll(IReadOnlyCollection<Guid> voterListIds)
    {
        var listIdColName = GetDelimitedColumnName(x => x.VoterListId);
        return Context.Database.ExecuteSqlRawAsync($"DELETE FROM {DelimitedSchemaAndTableName} WHERE {listIdColName} = ANY({{0}})", voterListIds);
    }
}
