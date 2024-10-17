// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Database.Repositories;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.Repositories;

public class VoterRepo : DbRepository<DataContext, Voter>
{
    public VoterRepo(DataContext context)
        : base(context)
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.", Justification = "Referencing hardened inerpolated string parameters.")]
    public Task UpdateSendVotingCardsToDomainOfInfluenceReturnAddress(Guid listId, bool sendVotingCardsToDomainOfInfluenceReturnAddress)
    {
        var listIdColName = GetDelimitedColumnName(x => x.ListId);
        var sendVotingCardsToDomainOfInfluenceReturnAddressColName = GetDelimitedColumnName(x => x.SendVotingCardsToDomainOfInfluenceReturnAddress);
        return Context.Database.ExecuteSqlRawAsync($"UPDATE {DelimitedSchemaAndTableName} SET {sendVotingCardsToDomainOfInfluenceReturnAddressColName} = {{1}} WHERE {listIdColName} = {{0}}", listId, sendVotingCardsToDomainOfInfluenceReturnAddress);
    }
}
