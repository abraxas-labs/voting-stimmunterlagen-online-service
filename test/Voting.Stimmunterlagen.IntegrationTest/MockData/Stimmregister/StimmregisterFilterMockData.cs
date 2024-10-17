// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using Voting.Stimmregister.Proto.V1.Services.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData.Stimmregister;

public static class StimmregisterFilterMockData
{
    public const string SwissWithVotingRightId = "3fc82cb1-b638-4a61-a2e9-ca47a9e6a9e0";
    public const string SwissWithoutVotingRightId = "bb3b45a2-2028-4477-8c2a-64e3580156fa";
    public const string SwissAged16AndOlder = "9586b50f-d375-4d68-99a3-e1d11402115d";

    public const string SwissWithVotingRightVersion1Id = "65d05d9c-8543-40c1-9cdb-f81fd5c2f26c";
    public const string SwissWithVotingRightVersion2Id = "4dae1953-eded-4613-acde-abd88936313e";
    public const string SwissWithVotingRightVersion3Id = "82bad2e5-c214-413f-94c2-7b82c95abccb";
    public const string SwissWithVotingRightVersion4NewCreatedId = "49bab8b7-56b1-4aeb-9ac6-d3c2c21a094e";

    public static readonly IReadOnlyCollection<FilterDefinitionModel> All = new[]
    {
        new FilterDefinitionModel
        {
            Id = SwissWithVotingRightId,
            Description = "Schweizer mit Stimmrecht",
            Name = "CH OK",
            Versions =
            {
                new FilterVersionModel
                {
                    Id = SwissWithVotingRightVersion1Id,
                    Name = "V1 01.08.2023",
                    Deadline = GrpcTestUtils.CreateTimestamp(2023, 8, 1),
                    Count = 5,
                    CountOfInvalidPersons = 1,
                    AuditInfo = new AuditInfoModel
                    {
                        CreatedAt = GrpcTestUtils.CreateTimestamp(2023, 3, 1, 10, 15, 12),
                        CreatedByName = "Rudolf Meier",
                    },
                },
                new FilterVersionModel
                {
                    Id = SwissWithVotingRightVersion2Id,
                    Name = "V2 01.09.2023",
                    Deadline = GrpcTestUtils.CreateTimestamp(2023, 9, 1),
                    Count = 4,
                    CountOfInvalidPersons = 0,
                    AuditInfo = new AuditInfoModel
                    {
                        CreatedAt = GrpcTestUtils.CreateTimestamp(2023, 4, 3, 11, 16, 15),
                        CreatedByName = "Hans-Peter Ryss",
                    },
                },
                new FilterVersionModel
                {
                    Id = SwissWithVotingRightVersion3Id,
                    Name = "V3 01.12.2023",
                    Deadline = GrpcTestUtils.CreateTimestamp(2023, 12, 1),
                    Count = 30_000,
                    CountOfInvalidPersons = 0,
                    AuditInfo = new AuditInfoModel
                    {
                        CreatedAt = GrpcTestUtils.CreateTimestamp(2023, 6, 7, 9, 12, 4),
                        CreatedByName = "Hans-Peter Ryss",
                    },
                },
                new FilterVersionModel
                {
                    Id = SwissWithVotingRightVersion4NewCreatedId,
                    Name = "V4 01.01.2024",
                    Deadline = GrpcTestUtils.CreateTimestamp(2023, 12, 12),
                    Count = 4,
                    CountOfInvalidPersons = 0,
                    AuditInfo = new AuditInfoModel
                    {
                        CreatedAt = GrpcTestUtils.CreateTimestamp(2023, 6, 7, 9, 15, 10),
                        CreatedByName = "Hans-Peter Ryss",
                    },
                },
            },
        },
        new FilterDefinitionModel
        {
            Id = SwissWithoutVotingRightId,
            Description = "Schweizer ohne Stimmrecht",
            Name = "CH NOK",
        },
        new FilterDefinitionModel
        {
            Id = SwissAged16AndOlder,
            Description = "Schweizer ab 16 Jahren",
            Name = "CH 16+",
        },
    };

    private static readonly IReadOnlyDictionary<string, FilterDefinitionModel> ById = All.ToDictionary(x => x.Id);

    private static readonly IReadOnlyDictionary<string, (FilterDefinitionModel Filter, FilterVersionModel Version)> ByVersionId = All
        .SelectMany(x => x.Versions.Select(v => (Filter: x, Version: v)))
        .ToDictionary(x => x.Version.Id);

    public static FilterDefinitionModel? Get(string filterId)
        => ById.GetValueOrDefault(filterId);

    public static (FilterDefinitionModel Filter, FilterVersionModel Version)? GetByVersionId(string filterVersionId)
    {
        return ByVersionId.TryGetValue(filterVersionId, out var filterAndVersion)
            ? filterAndVersion
            : null;
    }
}
