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
    private readonly VoterListRepo _voterListRepo;
    private readonly IAuth _auth;

    public VotingExportManager(
        VotingExportRenderServiceAdapter renderServiceAdapter,
        ContestDomainOfInfluenceRepo doiRepo,
        VoterListRepo voterListRepo,
        IAuth auth)
    {
        _renderServiceAdapter = renderServiceAdapter;
        _doiRepo = doiRepo;
        _voterListRepo = voterListRepo;
        _auth = auth;
    }

    public async Task<FileModel> GenerateExport(string key, Guid domainOfInfluenceId, Guid? voterListId, CancellationToken ct)
    {
        var context = await BuildContextAndEnsureHasPermission(key, domainOfInfluenceId, voterListId);
        return await _renderServiceAdapter.Render(context, ct);
    }

    private async Task<VotingExportRenderContext> BuildContextAndEnsureHasPermission(string key, Guid domainOfInfluenceId, Guid? voterListId)
    {
        var doi = await _doiRepo.Query()
            .Where(doi => doi.Id == domainOfInfluenceId)
            .Include(x => x.Contest)
            .WhereIsManager(_auth.Tenant.Id)
            .FirstOrDefaultAsync()
            ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), domainOfInfluenceId);

        var voterList = await _voterListRepo
            .Query()
            .Include(vl => vl.Import)
            .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
            .FirstOrDefaultAsync(vl => vl.Id == voterListId && vl.DomainOfInfluenceId == domainOfInfluenceId);

        if (voterListId.HasValue && voterList == null)
        {
            throw new EntityNotFoundException(nameof(VoterList), voterListId);
        }

        return new(key, doi, voterList);
    }
}
