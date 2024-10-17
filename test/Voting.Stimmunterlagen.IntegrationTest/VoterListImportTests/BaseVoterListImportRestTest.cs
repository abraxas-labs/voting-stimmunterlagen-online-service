// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.Models.Request;
using Voting.Stimmunterlagen.Models.Response;

namespace Voting.Stimmunterlagen.IntegrationTest.VoterListImportTests;

public abstract class BaseVoterListImportRestTest : BaseWriteableDbRestTest
{
    protected const string Url = "v1/voter-list-import";

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    protected BaseVoterListImportRestTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    protected async Task WithRequest(string? testFile, UpdateVoterListImportRequest? request, Func<MultipartFormDataContent, Task> processor)
    {
        StreamContent? fileContent = null;
        StringContent? dataContent = null;

        if (testFile != null)
        {
            fileContent = new StreamContent(File.OpenRead(testFile));
            fileContent.Headers.Add("Content-Type", MediaTypeNames.Text.Xml);
        }

        if (request != null)
        {
            dataContent = new StringContent(JsonSerializer.Serialize(request, request.GetType()));
        }

        try
        {
            using var content = new MultipartFormDataContent("boundary-f7611587-0a7b-4eef-994b-31f9c2231cd8");

            if (dataContent != null)
            {
                content.Add(dataContent, "data");
            }

            if (testFile != null)
            {
                content.Add(fileContent!, "file", Path.GetFileName(testFile));
            }

            await processor(content);
        }
        finally
        {
            dataContent?.Dispose();
            fileContent?.Dispose();
        }
    }

    protected async Task<VoterListImport> GetByName(string name)
    {
        var voterListImport = await RunOnDb(db => db.VoterListImports
            .Include(x => x.VoterLists!.OrderBy(vl => vl.VotingCardType))
            .ThenInclude(x => x.Voters!.OrderBy(y => y.LastName))
            .Include(x => x.VoterLists!)
            .ThenInclude(x => x.VoterDuplicates)
            .Include(x => x.VoterLists!)
            .ThenInclude(x => x.PoliticalBusinessEntries!.OrderBy(y => y.PoliticalBusinessId))
            .SingleOrDefaultAsync(x => x.Name == name))!;

        foreach (var voterList in voterListImport!.VoterLists!)
        {
            voterList.Import = null;
            voterList.ImportId = Guid.Empty;
            voterList.Id = Guid.Empty;

            foreach (var voter in voterList.Voters!)
            {
                voter.List = null;
                voter.ListId = Guid.Empty;
                voter.Id = Guid.Empty;

                voter.ContestIndex.Should().NotBe(0);
                voter.ContestIndex = 0;

                foreach (var placeOfOrigin in voter.PlacesOfOrigin ?? Enumerable.Empty<VoterPlaceOfOrigin>())
                {
                    placeOfOrigin.VoterId = Guid.Empty;
                    placeOfOrigin.Voter = null;
                }
            }

            foreach (var voterDuplicate in voterList.VoterDuplicates!)
            {
                voterDuplicate.Id = Guid.Empty;
                voterDuplicate.ListId = Guid.Empty;
                voterDuplicate.List = null;
            }

            foreach (var pbEntry in voterList.PoliticalBusinessEntries!)
            {
                pbEntry.Id = Guid.Empty;
                pbEntry.VoterListId = Guid.Empty;
                pbEntry.VoterList = null;
            }
        }

        return voterListImport;
    }

    protected async Task<CreateUpdateVoterListImportResponse> DeserializeHttpResponse(HttpResponseMessage response)
    {
        return JsonSerializer.Deserialize<CreateUpdateVoterListImportResponse>(
            await response.Content.ReadAsStringAsync(),
            _jsonSerializerOptions)!;
    }
}
