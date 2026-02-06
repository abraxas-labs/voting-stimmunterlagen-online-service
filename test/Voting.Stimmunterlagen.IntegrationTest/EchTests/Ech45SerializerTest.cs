// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Ech;
using Voting.Lib.Ech.Ech0045_4_0.Schemas;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Ech.Converter;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.EchTests;

public class Ech45SerializerTest : BaseReadOnlyDbTest
{
    public Ech45SerializerTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Theory]
    [InlineData(Ech0045Version.V4, "TestEch0045v4")]
    [InlineData(Ech0045Version.V6, "TestEch0045v6")]
    public async Task TestEch0045(Ech0045Version version, string testName)
    {
        var contestId = ContestMockData.BundFutureApprovedGuid;

        var voterList = await RunOnDb(async db =>
            await db.VoterLists
                .Include(vl => vl.Voters)
                .Include(vl => vl.DomainOfInfluence!.CountingCircles!).ThenInclude(doiCc => doiCc.CountingCircle)
                .FirstAsync(vl => vl.Id == VoterListMockData.BundFutureApprovedGemeindeArneggEVoterGuid));

        var contest = await RunOnDb(async db =>
            await db.Contests
                .Include(c => c.Translations)
                .Include(c => c.DomainOfInfluence)
                .FirstAsync(c => c.Id == contestId));

        var parentsAndSelf = await RunScoped<DomainOfInfluenceManager, List<ContestDomainOfInfluence>>(x => x.GetParentsAndSelf(voterList.DomainOfInfluenceId));
        var doiHierarchyById = new Dictionary<Guid, List<ContestDomainOfInfluence>>
        {
            [voterList.DomainOfInfluenceId] = parentsAndSelf,
        };

        RunScoped<Ech0045Service>(serializer =>
        {
            var serializedBytes = serializer.WriteEch0045Xml(version, contest, voterList, DomainOfInfluenceCanton.Sg, doiHierarchyById);
            var serialized = Encoding.UTF8.GetString(serializedBytes);

            XmlUtil.ValidateSchema(serialized, Ech0045Schemas.LoadEch0045Schemas());
            MatchXmlSnapshot(serialized, testName);
        });
    }

    [Fact]
    public async Task TestEch0045_TestDeliveryFlag()
    {
        var contestId = ContestMockData.BundFutureApprovedGuid;

        var voterList = await RunOnDb(async db =>
            await db.VoterLists
                .Include(vl => vl.Voters)
                .Include(vl => vl.DomainOfInfluence!.CountingCircles!).ThenInclude(doiCc => doiCc.CountingCircle)
                .FirstAsync(vl => vl.Id == VoterListMockData.BundFutureApprovedGemeindeArneggEVoterGuid));

        var contest = await RunOnDb(async db =>
            await db.Contests
                .Include(c => c.Translations)
                .Include(c => c.DomainOfInfluence)
                .FirstAsync(c => c.Id == contestId));

        contest.State = ContestState.Active;

        var parentsAndSelf = await RunScoped<DomainOfInfluenceManager, List<ContestDomainOfInfluence>>(x => x.GetParentsAndSelf(voterList.DomainOfInfluenceId));
        var doiHierarchyById = new Dictionary<Guid, List<ContestDomainOfInfluence>>
        {
            [voterList.DomainOfInfluenceId] = parentsAndSelf,
        };

        RunScoped<Ech0045Service>(serializer =>
        {
            var serializedBytes = serializer.WriteEch0045Xml(Ech0045Version.V4, contest, voterList, DomainOfInfluenceCanton.Sg, doiHierarchyById);
            var serialized = Encoding.UTF8.GetString(serializedBytes);

            var schemaSet = Ech0045Schemas.LoadEch0045Schemas();
            var delivery = new EchDeserializer().DeserializeXml<Ech0045_4_0.VoterDelivery>(serialized, schemaSet);

            delivery.DeliveryHeader.TestDeliveryFlag.Should().BeFalse();
        });
    }

    private void MatchXmlSnapshot(string xml, string fileName)
    {
        xml = XmlUtil.FormatTestXml(xml);
        var path = Path.Join(TestSourcePaths.TestProjectSourceDirectory, "EchTests", "_snapshots", fileName + ".xml");

#if UPDATE_SNAPSHOTS
        var updateSnapshot = true;
#else
        var updateSnapshot = false;
#endif
        xml.MatchRawSnapshot(path, updateSnapshot);
    }
}
