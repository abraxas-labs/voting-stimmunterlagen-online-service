// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
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
using VotingCardGroup = Voting.Stimmunterlagen.Data.Models.VotingCardGroup;
using VotingCardSort = Voting.Stimmunterlagen.Data.Models.VotingCardSort;
using VotingCardType = Voting.Stimmunterlagen.Data.Models.VotingCardType;

namespace Voting.Stimmunterlagen.Services;

[AuthorizeElectionAdmin]
public class DomainOfInfluenceVotingCardService : Proto.V1.DomainOfInfluenceVotingCardService.DomainOfInfluenceVotingCardServiceBase
{
    private readonly IMapper _mapper;
    private readonly DomainOfInfluenceVotingCardManager _manager;

    public DomainOfInfluenceVotingCardService(IMapper mapper, DomainOfInfluenceVotingCardManager manager)
    {
        _mapper = mapper;
        _manager = manager;
    }

    public override async Task<Empty> SetConfiguration(SetDomainOfInfluenceVotingCardConfigurationRequest request, ServerCallContext context)
    {
        await _manager.SetConfiguration(
            GuidParser.Parse(request.DomainOfInfluenceId),
            request.SampleCount,
            request.VotingCardGroups.Cast<VotingCardGroup>(),
            request.VotingCardSorts.Cast<VotingCardSort>());
        return ProtobufEmpty.Instance;
    }

    public override async Task<DomainOfInfluenceVotingCardConfiguration> GetConfiguration(
        GetDomainOfInfluenceVotingCardConfigurationRequest request,
        ServerCallContext context)
    {
        var configuration = await _manager.GetConfiguration(GuidParser.Parse(request.DomainOfInfluenceId));
        return _mapper.Map<DomainOfInfluenceVotingCardConfiguration>(configuration);
    }

    public override async Task<TemplatePreview> GetPdfPreview(GetDomainOfInfluenceVotingCardPdfPreviewRequest request, ServerCallContext context)
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
}
