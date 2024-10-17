// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmunterlagen.Core.Models;

namespace Voting.Stimmunterlagen.Core.Managers.VotingExport.RenderServices;

public interface IVotingRenderService
{
    Task<FileModel> Render(VotingExportRenderContext context, CancellationToken ct);
}
