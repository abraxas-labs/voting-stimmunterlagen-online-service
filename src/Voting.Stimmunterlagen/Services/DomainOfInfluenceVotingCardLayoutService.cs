// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Voting.Lib.Common;
using Voting.Lib.Grpc;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using GroupedDomainOfInfluenceVotingCardLayouts = Voting.Stimmunterlagen.Proto.V1.Models.GroupedDomainOfInfluenceVotingCardLayouts;
using VotingCardLayoutDataConfiguration = Voting.Stimmunterlagen.Data.Models.VotingCardLayoutDataConfiguration;
using VotingCardType = Voting.Stimmunterlagen.Data.Models.VotingCardType;

namespace Voting.Stimmunterlagen.Services;

[AuthorizeElectionAdmin]
public class DomainOfInfluenceVotingCardLayoutService : Proto.V1.DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceBase
{
    private readonly IMapper _mapper;
    private readonly DomainOfInfluenceVotingCardLayoutManager _manager;

    public DomainOfInfluenceVotingCardLayoutService(IMapper mapper, DomainOfInfluenceVotingCardLayoutManager manager)
    {
        _mapper = mapper;
        _manager = manager;
    }

    public override async Task<Empty> SetLayout(SetDomainOfInfluenceVotingCardLayoutRequest request, ServerCallContext context)
    {
        await _manager.SetLayout(
            GuidParser.Parse(request.DomainOfInfluenceId),
            _mapper.Map<VotingCardType>(request.VotingCardType),
            request.AllowCustom,
            request.TemplateId,
            _mapper.Map<VotingCardLayoutDataConfiguration>(request.DataConfiguration));
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> SetOverriddenLayout(
        SetOverriddenDomainOfInfluenceVotingCardLayoutRequest request,
        ServerCallContext context)
    {
        await _manager.SetOverriddenLayout(
            GuidParser.Parse(request.DomainOfInfluenceId),
            _mapper.Map<VotingCardType>(request.VotingCardType),
            request.TemplateId,
            _mapper.Map<VotingCardLayoutDataConfiguration>(request.DataConfiguration));
        return ProtobufEmpty.Instance;
    }

    public override async Task<DomainOfInfluenceVotingCardLayouts> GetLayouts(
        GetDomainOfInfluenceVotingCardLayoutsRequest request,
        ServerCallContext context)
    {
        var layouts = await _manager.GetLayoutsByDomainOfInfluence(GuidParser.Parse(request.DomainOfInfluenceId));
        return _mapper.Map<DomainOfInfluenceVotingCardLayouts>(layouts);
    }

    public override async Task<GroupedDomainOfInfluenceVotingCardLayouts> GetContestLayouts(GetContestDomainOfInfluenceVotingCardLayoutsRequest request, ServerCallContext context)
    {
        var layouts = await _manager.GetLayoutsByContest(GuidParser.Parse(request.ContestId));
        return _mapper.Map<GroupedDomainOfInfluenceVotingCardLayouts>(layouts);
    }

    public override async Task<Templates> GetTemplates(GetDomainOfInfluenceVotingCardLayoutTemplatesRequest request, ServerCallContext context)
    {
        var templates = await _manager.GetTemplates();
        return _mapper.Map<Templates>(templates);
    }

    public override async Task<TemplatePreview> GetPdfPreview(GetDomainOfInfluenceVotingCardLayoutPdfPreviewRequest request, ServerCallContext context)
    {
        await using var pdf = await _manager.GetPdfPreview(
            GuidParser.Parse(request.DomainOfInfluenceId),
            _mapper.Map<VotingCardType>(request.VotingCardType),
            context.CancellationToken);
        return new TemplatePreview
        {
            Pdf = await ByteString.FromStreamAsync(pdf, context.CancellationToken),
        };
    }

    public override async Task<TemplateDataValues> GetTemplateData(GetDomainOfInfluenceVotingCardLayoutTemplateDataRequest request, ServerCallContext context)
    {
        var data = await _manager.GetTemplateData(GuidParser.Parse(request.DomainOfInfluenceId));
        return _mapper.Map<TemplateDataValues>(data);
    }

    public override async Task<Empty> SetTemplateData(SetDomainOfInfluenceVotingCardLayoutTemplateDataRequest request, ServerCallContext context)
    {
        await _manager.SetTemplateData(
            GuidParser.Parse(request.DomainOfInfluenceId),
            _mapper.Map<List<SimpleTemplateFieldValue>>(request.Fields));
        return ProtobufEmpty.Instance;
    }
}
