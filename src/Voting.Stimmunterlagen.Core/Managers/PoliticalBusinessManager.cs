// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers;

public class PoliticalBusinessManager
{
    private readonly IAuth _auth;
    private readonly IDbRepository<PoliticalBusiness> _politicalBusinessRepo;

    public PoliticalBusinessManager(IAuth auth, IDbRepository<PoliticalBusiness> politicalBusinessRepo)
    {
        _auth = auth;
        _politicalBusinessRepo = politicalBusinessRepo;
    }

    public Task<List<PoliticalBusiness>> List(Guid? contestId, Guid? domainOfInfluenceId)
    {
        var tenantId = _auth.Tenant.Id;
        return _politicalBusinessRepo.Query()
            .Include(x => x.DomainOfInfluence)
            .Include(x => x.Translations)
            .Where(x => (contestId == null || x.ContestId == contestId)
                && x.PermissionEntries!.Any(p => p.SecureConnectId == tenantId
                    && (domainOfInfluenceId == null || p.DomainOfInfluenceId == domainOfInfluenceId)))
            .OrderBy(x => x.DomainOfInfluence!.Type)
            .ThenBy(x => x.PoliticalBusinessNumber)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }
}
