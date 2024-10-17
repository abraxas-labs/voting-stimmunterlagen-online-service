// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Voting.Stimmunterlagen.Proto.V1.Responses;

namespace Voting.Stimmunterlagen.Services;

[AuthorizeElectionAdmin]
public class DomainOfInfluenceVotingCardBrickService : Proto.V1.DomainOfInfluenceVotingCardBrickService.DomainOfInfluenceVotingCardBrickServiceBase
{
    private readonly DomainOfInfluenceVotingCardBrickManager _manager;
    private readonly IMapper _mapper;

    public DomainOfInfluenceVotingCardBrickService(DomainOfInfluenceVotingCardBrickManager manager, IMapper mapper)
    {
        _manager = manager;
        _mapper = mapper;
    }

    public override async Task<TemplateBricks> List(ListDomainOfInfluenceVotingCardBrickRequest request, ServerCallContext context)
    {
        var bricks = await _manager.List(request.TemplateId);
        return _mapper.Map<TemplateBricks>(bricks);
    }

    public override async Task<GetDomainOfInfluenceVotingCardBrickContentEditorUrlResponse> GetContentEditorUrl(GetDomainOfInfluenceVotingCardBrickContentEditorUrlRequest request, ServerCallContext context)
    {
        var url = await _manager.GetContentEditorUrl(request.BrickId, request.ContentId);
        return new() { Url = url };
    }

    public override async Task<UpdateDomainOfInfluenceVotingCardBrickContentResponse> UpdateContent(UpdateDomainOfInfluenceVotingCardBrickContentRequest request, ServerCallContext context)
    {
        var (newBrickId, newContentId) = await _manager.UpdateContent(request.ContentId, request.Content);
        return new() { NewBrickId = newBrickId, NewContentId = newContentId };
    }
}
