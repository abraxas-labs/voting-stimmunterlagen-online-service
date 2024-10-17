// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.DocPipe;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;
using VoterPageInfo = Voting.Stimmunterlagen.Core.Models.VoterPageInfo;

namespace Voting.Stimmunterlagen.Core.Mocks;

/// <summary>
/// Simple mock for the DocPipe service.
/// Can be used for tests or if the Abraxas VPN or DocPipe is not available.
/// Removed from the compilation by ms build for release configurations.
/// </summary>
public class DocPipeServiceMock : IDocPipeService
{
    private readonly IDbRepository<VotingCardGeneratorJob> _jobsRepo;
    private readonly DocPipeConfig _docPipeConfig;

    public DocPipeServiceMock(IDbRepository<VotingCardGeneratorJob> jobsRepo, DocPipeConfig docPipeConfig)
    {
        _jobsRepo = jobsRepo;
        _docPipeConfig = docPipeConfig;
    }

    public async Task<T?> ExecuteJob<T>(string application, Dictionary<string, string> jobVariables, CancellationToken ct = default)
    {
        if (typeof(T) != typeof(VoterPagesInfo))
        {
            return default;
        }

        if (!jobVariables.TryGetValue(_docPipeConfig.DraftIdJobVariable, out var draftIdString) || !int.TryParse(draftIdString, out var draftId))
        {
            return default;
        }

        var voterIds = await _jobsRepo.Query()
            .Where(j => j.DraftId == draftId)
            .SelectMany(j => j.Voter)
            .Select(v => v.Id)
            .OrderBy(x => x)
            .ToListAsync(ct);
        var voterPages = voterIds.Select((id, index) => new VoterPageInfo
        {
            Id = id,
            PageFrom = index + 1,
            PageTo = index + 1,
        }).ToList();
        return (T)(object)new VoterPagesInfo { Pages = voterPages };
    }
}
