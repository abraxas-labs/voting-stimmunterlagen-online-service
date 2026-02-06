// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf;
using Grpc.Core;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Voter = Voting.Stimmunterlagen.Data.Models.Voter;

namespace Voting.Stimmunterlagen.Services;

[AuthorizeElectionAdmin]
public class ManualVotingCardGeneratorJobService : Proto.V1.ManualVotingCardGeneratorJobsService.ManualVotingCardGeneratorJobsServiceBase
{
    private readonly ManualVotingCardGeneratorJobManager _manager;
    private readonly IMapper _mapper;

    public ManualVotingCardGeneratorJobService(ManualVotingCardGeneratorJobManager manager, IMapper mapper)
    {
        _manager = manager;
        _mapper = mapper;
    }

    public override async Task<ManualVotingCardGeneratorJobs> List(ListManualVotingCardGeneratorJobsRequest request, ServerCallContext context)
    {
        var jobs = await _manager.List(GuidParser.Parse(request.DomainOfInfluenceId));
        return _mapper.Map<ManualVotingCardGeneratorJobs>(jobs);
    }

    public override async Task<TemplatePreview> Create(CreateManualVotingCardGeneratorJobRequest request, ServerCallContext context)
    {
        var voter = _mapper.Map<Voter>(request.Voter);
        var doiId = GuidParser.Parse(request.DomainOfInfluenceId);
        await using var pdf = await _manager.Create(doiId, voter, context.CancellationToken);
        return new TemplatePreview
        {
            Pdf = await ByteString.FromStreamAsync(pdf, context.CancellationToken),
        };
    }

    public override async Task<TemplatePreview> CreateEmpty(CreateEmptyManualVotingCardGeneratorJobRequest request, ServerCallContext context)
    {
        var doiId = GuidParser.Parse(request.DomainOfInfluenceId);
        await using var pdf = await _manager.CreateEmpty(doiId, context.CancellationToken);
        return new TemplatePreview
        {
            Pdf = await ByteString.FromStreamAsync(pdf, context.CancellationToken),
        };
    }
}
