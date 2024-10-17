// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Utils;

namespace Voting.Stimmunterlagen.Data.Repositories;

public class ContestRepo : DbRepository<Contest>
{
    // swiss post datamatrix order number supports 9-digits
    private const int ContestIndexSequenceMax = 999999999;

    public ContestRepo(DataContext context)
        : base(context)
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.", Justification = "Referencing hardened inerpolated string parameters.")]
    public async Task CreateVoterContestIndexSequence(Guid contestId)
    {
        var sequenceName = SequenceNames.BuildVoterContestIndexSequenceName(contestId);
        await Context.Database.ExecuteSqlRawAsync($"CREATE SEQUENCE IF NOT EXISTS \"{sequenceName}\" MAXVALUE {ContestIndexSequenceMax}");
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.", Justification = "Referencing hardened inerpolated string parameters.")]
    public async Task DeleteVoterContestIndexSequence(Guid contestId)
    {
        var sequenceName = SequenceNames.BuildVoterContestIndexSequenceName(contestId);
        await Context.Database.ExecuteSqlRawAsync($"DROP SEQUENCE IF EXISTS \"{sequenceName}\"");
    }
}
