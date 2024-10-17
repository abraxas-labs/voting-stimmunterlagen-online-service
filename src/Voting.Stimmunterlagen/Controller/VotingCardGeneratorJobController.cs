// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voting.Stimmunterlagen.Core.Managers;

namespace Voting.Stimmunterlagen.Controller;

[Route("v1/voting-card-generator-job")]
[ApiController]

public class VotingCardGeneratorJobController : ControllerBase
{
    private readonly VotingCardGeneratorJobManager _votingCardGeneratorJobManager;

    public VotingCardGeneratorJobController(VotingCardGeneratorJobManager votingCardGeneratorJobManager)
    {
        _votingCardGeneratorJobManager = votingCardGeneratorJobManager;
    }

    // Note: DmDoc currently does not support authorization in webhooks.
    // We use a unique token per callback to make sure that the requests come from DmDoc.
    [HttpPost]
    [AllowAnonymous]
    public async Task PdfFinishedCallback(string token)
    {
        // DmDoc uses snake_case naming in JSON, handle that separately since we use camelCase
        using var streamReader = new StreamReader(Request.Body);
        var callbackData = await streamReader.ReadToEndAsync();
        await _votingCardGeneratorJobManager.HandleCallback(callbackData, token, HttpContext.RequestAborted);
    }
}
