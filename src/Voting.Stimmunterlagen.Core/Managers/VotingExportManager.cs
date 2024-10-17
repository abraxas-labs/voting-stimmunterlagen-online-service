// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Managers.VotingExport;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers;

public class VotingExportManager
{
    private readonly VotingExportRenderServiceAdapter _renderServiceAdapter;
    private readonly ContestDomainOfInfluenceRepo _doiRepo;
    private readonly IAuth _auth;

    public VotingExportManager(VotingExportRenderServiceAdapter renderServiceAdapter, ContestDomainOfInfluenceRepo doiRepo, IAuth auth)
    {
        _renderServiceAdapter = renderServiceAdapter;
        _doiRepo = doiRepo;
        _auth = auth;
    }

    public async Task<FileModel> GenerateExport(string key, Guid domainOfInfluenceId, CancellationToken ct)
    {
        var context = await BuildContext(key, domainOfInfluenceId);
        return await _renderServiceAdapter.Render(context, ct);
    }

    private async Task<VotingExportRenderContext> BuildContext(string key, Guid domainOfInfluenceId)
    {
        var doi = await _doiRepo.Query()
            .Where(doi => doi.Id == domainOfInfluenceId)
            .WhereIsManager(_auth.Tenant.Id)
            .FirstOrDefaultAsync()
            ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), domainOfInfluenceId);

        return new(key, doi);
    }
}
