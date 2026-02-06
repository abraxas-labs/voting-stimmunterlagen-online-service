// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Voting.Lib.Common;
using Voting.Lib.Grpc;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using VotingCardLayoutDataConfiguration = Voting.Stimmunterlagen.Data.Models.VotingCardLayoutDataConfiguration;
using VotingCardType = Voting.Stimmunterlagen.Data.Models.VotingCardType;

namespace Voting.Stimmunterlagen.Services;

[AuthorizeElectionAdmin]
public class ContestVotingCardLayoutService : Proto.V1.ContestVotingCardLayoutService.ContestVotingCardLayoutServiceBase
{
    private readonly IMapper _mapper;
    private readonly ContestVotingCardLayoutManager _manager;

    public ContestVotingCardLayoutService(IMapper mapper, ContestVotingCardLayoutManager manager)
    {
        _mapper = mapper;
        _manager = manager;
    }

    public override async Task<Empty> SetLayout(SetContestVotingCardLayoutRequest request, ServerCallContext context)
    {
        await _manager.SetLayout(
            GuidParser.Parse(request.ContestId),
            _mapper.Map<VotingCardType>(request.VotingCardType),
            request.AllowCustom,
            request.TemplateId,
            _mapper.Map<VotingCardLayoutDataConfiguration>(request.DataConfiguration));
        return ProtobufEmpty.Instance;
    }

    public override async Task<ContestVotingCardLayouts> GetLayouts(GetContestVotingCardLayoutsRequest request, ServerCallContext context)
    {
        var layouts = await _manager.GetLayouts(GuidParser.Parse(request.ContestId));
        return _mapper.Map<ContestVotingCardLayouts>(layouts);
    }

    public override async Task<Templates> GetTemplates(GetContestVotingCardLayoutTemplatesRequest request, ServerCallContext context)
    {
        var templates = await _manager.GetTemplates(GuidParser.Parse(request.ContestId));
        return _mapper.Map<Templates>(templates);
    }

    public override async Task<TemplatePreview> GetPdfPreview(GetContestVotingCardLayoutPdfPreviewRequest request, ServerCallContext context)
    {
        await using var pdf = await _manager.GetPdfPreview(
            GuidParser.Parse(request.ContestId),
            _mapper.Map<VotingCardType>(request.VotingCardType),
            context.CancellationToken);
        return new TemplatePreview
        {
            Pdf = await ByteString.FromStreamAsync(pdf, context.CancellationToken),
        };
    }
}
