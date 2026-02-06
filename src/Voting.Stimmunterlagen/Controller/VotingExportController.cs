// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Voting.Lib.Rest.Files;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Models;
using Voting.Stimmunterlagen.Models.Request;

namespace Voting.Stimmunterlagen.Controller;

[Route("v1/voting-export")]
[ApiController]
[AuthorizeElectionAdmin]
public class VotingExportController : ControllerBase
{
    private readonly VotingExportManager _votingExportManager;

    public VotingExportController(VotingExportManager votingExportManager)
    {
        _votingExportManager = votingExportManager;
    }

    [HttpPost]
    public async Task<FileResult> GenerateExport([FromBody] GenerateVotingExportRequest request, CancellationToken ct)
    {
        var file = await _votingExportManager.GenerateExport(request.Key, request.DomainOfInfluenceId, request.VoterListId, ct);
        return SingleFileResult.Create(new FileModelWrapper(file), ct);
    }
}
