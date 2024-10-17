// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Voting.Lib.Rest.Utils;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Ech.Converter;
using Voting.Stimmunterlagen.Models.Request;
using Voting.Stimmunterlagen.Models.Response;
using Voting.Stimmunterlagen.Util;

namespace Voting.Stimmunterlagen.Controller;

// see https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-5.0#upload-large-files-with-streaming
[Route("v1/voter-list-import")]
[ApiController]
[AuthorizeElectionAdmin]
public class VoterListImportController : ControllerBase
{
    private const long MaxDocumentSize = 1024L * 1024L * 1024L; // 1 GB

    private readonly VoterListImportManager _voterListImportManager;
    private readonly IMapper _mapper;
    private readonly MultipartRequestHelper _multipartHelper;
    private readonly EchService _echService;

    public VoterListImportController(
        VoterListImportManager voterListImportManager,
        IMapper mapper,
        MultipartRequestHelper multipartHelper,
        EchService echService)
    {
        _voterListImportManager = voterListImportManager;
        _mapper = mapper;
        _multipartHelper = multipartHelper;
        _echService = echService;
    }

    [HttpPost]
    [DisableFormValueModelBinding]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxDocumentSize)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxDocumentSize)]
    public async Task<CreateUpdateVoterListImportResponse> Create()
    {
        return await ReadVoterListImport<CreateVoterListImportRequest>(
            null,
            _voterListImportManager.Create,
            _ => throw new ValidationException("File content is missing"));
    }

    [HttpPut("{id:Guid}")]
    [DisableFormValueModelBinding]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxDocumentSize)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxDocumentSize)]
    public async Task<CreateUpdateVoterListImportResponse> Update(Guid id)
    {
        return await ReadVoterListImport<UpdateVoterListImportRequest>(id, _voterListImportManager.Update, _voterListImportManager.Update);
    }

    private async Task<CreateUpdateVoterListImportResponse> ReadVoterListImport<T>(
        Guid? importId,
        Func<VoterListImport, XmlReader, CancellationToken, Task> processWithFile,
        Func<VoterListImport, Task> processWithoutFile)
        where T : UpdateVoterListImportRequest
    {
        // Because the XML deserialization in combination with XML schema validation isn't fully asynchronous, we need to allow sync IO access.
        // see https://github.com/dotnet/runtime/issues/78802
        // Previous attempts at buffering parts (ex. 500 KB) of the request were not successful, sync IO access was still happening
        // Buffering the entire request body to memory or a file is not a good idea, as the request can get very big
        var bodyControl = HttpContext.Features.Get<IHttpBodyControlFeature>();
        if (bodyControl != null)
        {
            bodyControl.AllowSynchronousIO = true;
        }

        return await _multipartHelper.ReadFileAndData<T, CreateUpdateVoterListImportResponse>(
            Request,
            async data =>
            {
                var voterListImport = _mapper.Map<VoterListImport>(data.RequestData);
                voterListImport.Id = importId ?? Guid.Empty;
                voterListImport.Source = VoterListSource.ManualEch45Upload;
                voterListImport.SourceId = data.FileName ?? string.Empty;

                using var xmlReader = _echService.GetEch0045Reader(data.FileContent);
                voterListImport.AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit = await _echService.IsFromElectoralRegister(xmlReader, HttpContext.RequestAborted);
                await processWithFile(voterListImport, xmlReader, HttpContext.RequestAborted);

                return new CreateUpdateVoterListImportResponse
                {
                    ImportId = voterListImport.Id,
                    VoterLists = _mapper.Map<List<CreateUpdateVoterListResponse>>(voterListImport.VoterLists),
                    AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit = voterListImport.AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit,
                };
            },
            async data =>
            {
                var voterListImport = _mapper.Map<VoterListImport>(data);
                voterListImport.Id = importId ?? Guid.Empty;
                voterListImport.Source = VoterListSource.ManualEch45Upload;
                await processWithoutFile(voterListImport);

                return new CreateUpdateVoterListImportResponse
                {
                    ImportId = voterListImport.Id,
                    AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit = voterListImport.AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit,
                };
            },
            [MediaTypeNames.Text.Xml]);
    }
}
