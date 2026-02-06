// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Voting.Lib.ImageProcessing;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.ObjectStorage;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using EVotingModels = Voting.Stimmunterlagen.EVoting.Models;

namespace Voting.Stimmunterlagen.Core.Managers.EVoting;

public class EVotingContestBuilder
{
    private readonly IMapper _mapper;
    private readonly DomainOfInfluenceLogoStorage _logoStorage;
    private readonly IImageProcessor _imageProcessor;
    private readonly uint _maxLogoHeight;

    public EVotingContestBuilder(
        IMapper mapper,
        DomainOfInfluenceLogoStorage logoStorage,
        IImageProcessor imageProcessor,
        ApiConfig config)
    {
        _mapper = mapper;
        _logoStorage = logoStorage;
        _imageProcessor = imageProcessor;
        _maxLogoHeight = config.ContestEVotingExport.MaxLogoHeight;
    }

    public async Task<EVotingModels.Contest> BuildContest(
        Contest contest,
        IReadOnlyCollection<Attachment> attachments,
        IReadOnlyCollection<VoterList> voterLists)
    {
        var eVotingContest = _mapper.Map<Contest, EVotingModels.Contest>(contest);
        var eVotingDois = new List<EVotingModels.DomainOfInfluence>();

        var attachmentStationsByDoiId = AttachmentStationsBuilder.BuildAttachmentStationsByDomainOfInfluenceId(voterLists, attachments, contest.IsPoliticalAssembly);

        foreach (var doi in contest.ContestDomainOfInfluences!)
        {
            eVotingDois.Add(await BuildDomainOfInfluence(doi, attachmentStationsByDoiId.GetValueOrDefault(doi.Id) ?? string.Empty));
        }

        eVotingContest.ContestDomainOfInfluences = eVotingDois;
        return eVotingContest;
    }

    private async Task<EVotingModels.DomainOfInfluence> BuildDomainOfInfluence(ContestDomainOfInfluence doi, string attachmentStations)
    {
        var eVotingDoi = _mapper.Map<EVotingModels.DomainOfInfluence>(doi);
        eVotingDoi.Logo = CompressLogo(await _logoStorage.TryFetchAsBase64(doi));
        eVotingDoi.AttachmentStations = attachmentStations;
        return eVotingDoi;
    }

    private string? CompressLogo(string? base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
        {
            return null;
        }

        var logoBytes = Convert.FromBase64String(base64String);

        var processedLogoBytes = _imageProcessor.ConvertFormat(logoBytes, ImageFormat.Png);

        // using int.MaxValue as width, with a maintained aspect ratio will ensure that the image gets the specified height.
        processedLogoBytes = _imageProcessor.Resize(processedLogoBytes, int.MaxValue, _maxLogoHeight, true);
        processedLogoBytes = _imageProcessor.LosslessCompress(processedLogoBytes);

        return Convert.ToBase64String(processedLogoBytes);
    }
}
