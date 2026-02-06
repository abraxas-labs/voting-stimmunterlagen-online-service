// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.Models.VoterListImport;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Utils;

public class VoterPrintInfoAggregator
{
    private readonly IDbRepository<DomainOfInfluenceVoterDuplicate> _duplicateRepo;

    public VoterPrintInfoAggregator(IDbRepository<DomainOfInfluenceVoterDuplicate> duplicateRepo)
    {
        _duplicateRepo = duplicateRepo;
    }

    public async Task Aggregate(IEnumerable<Voter> voters, Guid domainOfInfluenceId)
    {
        var duplicates = await _duplicateRepo.Query()
            .Where(d => d.DomainOfInfluenceId == domainOfInfluenceId)
            .Include(d => d.Voters!)
            .ThenInclude(v => v.DomainOfInfluences)
            .ToListAsync();

        Aggregate(voters, duplicates);
    }

    internal static void Aggregate(IEnumerable<Voter> voters, IEnumerable<DomainOfInfluenceVoterDuplicate> duplicates)
    {
        var domainOfInfluenceDuplicateByKey = duplicates.ToDictionary(d => new VoterKey(d.FirstName, d.LastName, d.DateOfBirth, d.Street, d.HouseNumber));

        foreach (var voter in voters)
        {
            var key = new VoterKey(voter.FirstName, voter.LastName, voter.DateOfBirth, voter.Street, voter.HouseNumber);

            if (!domainOfInfluenceDuplicateByKey.TryGetValue(key, out var domainOfInfluenceDuplicate))
            {
                continue;
            }

            MergeVoterDuplicateInfos(voter, domainOfInfluenceDuplicate.Voters!.Where(v => v.Id != voter.Id));
        }
    }

    private static void MergeVoterDuplicateInfos(Voter voter, IEnumerable<Voter> voterDuplicates)
    {
        voter.DomainOfInfluences!.AddRange(voterDuplicates.SelectMany(d => d.DomainOfInfluences!));

        voter.DomainOfInfluences = voter.DomainOfInfluences!
            .DistinctBy(d => new { d.DomainOfInfluenceType, d.DomainOfInfluenceIdentification })
            .OrderBy(d => d.DomainOfInfluenceType)
            .ThenBy(d => d.DomainOfInfluenceIdentification)
            .ToList();
    }
}
