// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Voting.Stimmunterlagen.Core.Managers.Templates;
using Voting.Stimmunterlagen.Core.Models;

namespace Voting.Stimmunterlagen.Core.Managers;

public class DomainOfInfluenceVotingCardBrickManager
{
    private readonly TemplateManager _templateManager;

    public DomainOfInfluenceVotingCardBrickManager(TemplateManager templateManager)
    {
        _templateManager = templateManager;
    }

    public async Task<List<TemplateBrick>> List(int templateId)
    {
        return await _templateManager.GetBricksForMyTenant(templateId);
    }

    public async Task<string> GetContentEditorUrl(int brickId, int brickContentId)
    {
        return await _templateManager.GetBrickContentEditorUrl(brickId, brickContentId);
    }

    public async Task<(int NewBrickId, int NewContentId)> UpdateContent(int brickContentId, string content)
    {
        return await _templateManager.UpdateBrickContent(brickContentId, content);
    }
}
