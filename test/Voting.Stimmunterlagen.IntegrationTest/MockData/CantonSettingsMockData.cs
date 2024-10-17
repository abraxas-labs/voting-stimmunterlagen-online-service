// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class CantonSettingsMockData
{
    public const string StGallenId = "90b0af34-7b94-4bc3-a27b-42715c2fe672";
    public const string ThurgauId = "f0d39e93-6e5d-41d1-9a18-c2db3fb3f0c1";

    public static readonly Guid StGallenGuid = Guid.Parse(StGallenId);
    public static readonly Guid ThurgauGuid = Guid.Parse(ThurgauId);

    public static CantonSettings StGallen => new()
    {
        Id = StGallenGuid,
        Canton = DomainOfInfluenceCanton.Sg,
        VotingDocumentsEVotingEaiMessageType = "EVOT-SG",
    };

    public static CantonSettings Thurgau => new()
    {
        Id = ThurgauGuid,
        Canton = DomainOfInfluenceCanton.Tg,
        VotingDocumentsEVotingEaiMessageType = "EVOT-TG",
    };

    public static IEnumerable<CantonSettings> All
    {
        get
        {
            yield return StGallen;
            yield return Thurgau;
        }
    }

    public static Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        return runScoped(async sp =>
        {
            var cantonSettingsRepo = sp.GetRequiredService<IDbRepository<CantonSettings>>();

            foreach (var cantonSettings in All)
            {
                await cantonSettingsRepo.Create(cantonSettings);
            }
        });
    }
}
