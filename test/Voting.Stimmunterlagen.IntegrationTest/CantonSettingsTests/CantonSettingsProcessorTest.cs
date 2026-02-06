// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1;
using Abraxas.Voting.Basis.Events.V1.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Xunit;
using SharedProto = Abraxas.Voting.Basis.Shared.V1;

namespace Voting.Stimmunterlagen.IntegrationTest.CantonSettingsTests;

public class CantonSettingsProcessorTest : BaseWriteableDbTest
{
    public CantonSettingsProcessorTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CantonSettingsCreated()
    {
        var id = Guid.Parse("f46f1e84-cbcb-40a6-bf40-b4dce9782a81");
        var eventData = new CantonSettingsCreated
        {
            CantonSettings = new CantonSettingsEventData
            {
                Id = id.ToString(),
                Canton = SharedProto.DomainOfInfluenceCanton.Zh,
                VotingDocumentsEVotingEaiMessageType = "EVOT-ZH",
            },
        };

        await TestEventPublisher.PublishTwice(eventData);

        var cantonSettings = await RunOnDb(db => db.CantonSettings.FirstAsync(x => x.Id == id));
        cantonSettings.MatchSnapshot();
    }

    [Fact]
    public async Task CantonSettingsCreatedCantonAr()
    {
        var id = Guid.Parse("c23f1e84-cbcb-40a6-bf40-b4dce9782a82");
        var eventData = new CantonSettingsCreated
        {
            CantonSettings = new CantonSettingsEventData
            {
                Id = id.ToString(),
                Canton = SharedProto.DomainOfInfluenceCanton.Ar,
                VotingDocumentsEVotingEaiMessageType = "EVOT-AR",
            },
        };

        await TestEventPublisher.PublishTwice(eventData);

        var cantonSettings = await RunOnDb(db => db.CantonSettings.FirstAsync(x => x.Id == id));
        cantonSettings.Canton.Should().Be(DomainOfInfluenceCanton.Ar);
    }

    [Fact]
    public async Task CantonSettingsUpdated()
    {
        var id = CantonSettingsMockData.StGallenGuid;
        var eventData = new CantonSettingsCreated
        {
            CantonSettings = new CantonSettingsEventData
            {
                Id = id.ToString(),
                Canton = SharedProto.DomainOfInfluenceCanton.Sg,
                VotingDocumentsEVotingEaiMessageType = "EVOT-SG-Update",
            },
        };

        await TestEventPublisher.PublishTwice(eventData);

        var cantonSettings = await RunOnDb(db => db.CantonSettings.FirstAsync(x => x.Id == id));
        cantonSettings.MatchSnapshot();
    }

    [Fact]
    public async Task CantonSettingsUpdatedShouldUpdateDomainOfInfluenceCantonSettings()
    {
        var createDoiTg = new DomainOfInfluenceCreated
        {
            DomainOfInfluence = new DomainOfInfluenceEventData
            {
                Id = "b032e353-fcfb-4c75-b27e-10d27381c233",
                Canton = SharedProto.DomainOfInfluenceCanton.Tg,
                Bfs = "1234",
                Code = "1",
                Name = "Bund (TG)",
                Type = SharedProto.DomainOfInfluenceType.Ch,
                AuthorityName = "Staatskanzlei TG",
                ShortName = "CH TG",
                SortNumber = 1,
                SecureConnectId = "1234",
            },
        };
        await TestEventPublisher.Publish(createDoiTg);

        var eventDataSg = new CantonSettingsCreated
        {
            CantonSettings = new CantonSettingsEventData
            {
                Id = CantonSettingsMockData.StGallenId,
                Canton = SharedProto.DomainOfInfluenceCanton.Sg,
                VotingDocumentsEVotingEaiMessageType = "EVOT-SG-Update",
            },
        };
        var eventDataTg = new CantonSettingsCreated
        {
            CantonSettings = new CantonSettingsEventData
            {
                Id = CantonSettingsMockData.ThurgauId,
                Canton = SharedProto.DomainOfInfluenceCanton.Tg,
                VotingDocumentsEVotingEaiMessageType = "EVOT-TG-Update",
            },
        };

        await TestEventPublisher.PublishTwice(eventDataSg);
        await TestEventPublisher.PublishTwice(eventDataTg);

        var dois = await RunOnDb(db => db.DomainOfInfluences.ToListAsync());
        var doisByCanton = dois.GroupBy(x => x.Canton).ToDictionary(x => x.Key, x => x.ToList());
        var sgDois = doisByCanton[DomainOfInfluenceCanton.Sg];
        sgDois.Should().NotBeEmpty();
        sgDois.All(x => x.CantonDefaults.VotingDocumentsEVotingEaiMessageType.Equals("EVOT-SG-Update")).Should().BeTrue();

        var tgDois = doisByCanton[DomainOfInfluenceCanton.Tg];
        tgDois.Should().NotBeEmpty();
        tgDois.All(x => x.CantonDefaults.VotingDocumentsEVotingEaiMessageType.Equals("EVOT-TG-Update")).Should().BeTrue();

        var contestDois = await RunOnDb(db => db.ContestDomainOfInfluences.Include(x => x.Contest).ToListAsync());

        var contestDoisByCanton = contestDois.Where(x => !x.Contest!.TestingPhaseEnded).GroupBy(x => x.Canton).ToDictionary(x => x.Key, x => x.ToList());
        var contestDoisSg = contestDoisByCanton[DomainOfInfluenceCanton.Sg];
        contestDoisSg.Should().NotBeEmpty();
        contestDoisSg.All(x => x.CantonDefaults.VotingDocumentsEVotingEaiMessageType.Equals("EVOT-SG-Update")).Should().BeTrue();

        var contestDoisTg = contestDoisByCanton[DomainOfInfluenceCanton.Tg];
        contestDoisTg.Should().NotBeEmpty();
        contestDoisTg.All(x => x.CantonDefaults.VotingDocumentsEVotingEaiMessageType.Equals("EVOT-TG-Update")).Should().BeTrue();

        // should not update if testing phase has ended
        var contestDoisSgTestingPhaseEnded = contestDois.Where(x => x.Contest!.TestingPhaseEnded && x.Canton == DomainOfInfluenceCanton.Sg).ToList();
        contestDoisSgTestingPhaseEnded.Should().NotBeEmpty();
        contestDoisSgTestingPhaseEnded.All(x => x.CantonDefaults.VotingDocumentsEVotingEaiMessageType.Equals(CantonSettingsMockData.StGallen.VotingDocumentsEVotingEaiMessageType)).Should().BeTrue();
    }
}
