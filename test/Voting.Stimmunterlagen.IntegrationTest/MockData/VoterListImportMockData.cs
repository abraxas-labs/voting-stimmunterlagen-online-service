// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class VoterListImportMockData
{
    public const string BundArchivedGemeindeArneggId = "00d0c4c2-248f-4955-9a49-7f1347018b62";
    public const string BundFutureApprovedGemeindeArneggId = "0b3e449f-8675-42c9-b352-0867c62be4af";
    public const string BundFutureApprovedGemeindeArneggElectoralRegisterId = "52fc9ddb-87cb-4e73-b5ff-4017009f0e9b";
    public const string BundFutureApprovedStadtGossauId = "edfef886-8659-4f39-b428-7315d903ac02";
    public const string PoliticalAssemblyBundFutureApprovedGemeindeArneggId = "9b24b2a8-6395-4df9-8448-d3fac97bb53a";

    public static readonly Guid BundArchivedGemeindeArneggGuid = Guid.Parse(BundArchivedGemeindeArneggId);
    public static readonly Guid BundFutureApprovedGemeindeArneggGuid = Guid.Parse(BundFutureApprovedGemeindeArneggId);
    public static readonly Guid BundFutureApprovedGemeindeArneggElectoralRegisterGuid = Guid.Parse(BundFutureApprovedGemeindeArneggElectoralRegisterId);
    public static readonly Guid BundFutureApprovedStadtGossauGuid = Guid.Parse(BundFutureApprovedStadtGossauId);
    public static readonly Guid PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid = Guid.Parse(PoliticalAssemblyBundFutureApprovedGemeindeArneggId);

    public static VoterListImport BundArchivedGemeindeArnegg => new()
    {
        Id = BundArchivedGemeindeArneggGuid,
        Name = "Stimmregister Schweizer Arnegg",
        Source = VoterListSource.ManualEch45Upload,
        SourceId = "arnegg-ech-0045-swiss.xml",
        LastUpdate = MockedClock.GetDate(-10),
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggGuid,
    };

    public static VoterListImport BundFutureApprovedGemeindeArnegg => new()
    {
        Id = BundFutureApprovedGemeindeArneggGuid,
        Name = "Stimmregister Schweizer Arnegg",
        Source = VoterListSource.ManualEch45Upload,
        SourceId = "arnegg-ech-0045-swiss.xml",
        LastUpdate = MockedClock.GetDate(-10),
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
    };

    public static VoterListImport BundFutureApprovedGemeindeArneggElectoralRegister => new()
    {
        Id = BundFutureApprovedGemeindeArneggElectoralRegisterGuid,
        Name = "Stimmregister Arnegg via Stimmregister",
        Source = VoterListSource.VotingStimmregisterFilterVersion,
        SourceId = "ce2387c1-625d-4026-b29f-6f90b465dae8",
        LastUpdate = MockedClock.GetDate(-2),
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit = true,
    };

    public static VoterListImport BundFutureApprovedStadtGossau => new()
    {
        Id = BundFutureApprovedStadtGossauGuid,
        Name = "Stimmregister Schweizer Gossau",
        Source = VoterListSource.ManualEch45Upload,
        SourceId = "gossau-ech-0045-swiss.xml",
        LastUpdate = MockedClock.GetDate(-2),
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid,
    };

    public static VoterListImport PoliticalAssemblyBundFutureApprovedGemeindeArnegg => new()
    {
        Id = PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid,
        Name = "Stimmregister Schweizer Arnegg",
        Source = VoterListSource.ManualEch45Upload,
        SourceId = "arnegg-ech-0045-swiss.xml",
        LastUpdate = MockedClock.GetDate(-10),
        DomainOfInfluenceId = DomainOfInfluenceMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid,
    };

    public static IEnumerable<VoterListImport> All
    {
        get
        {
            yield return BundArchivedGemeindeArnegg;
            yield return BundFutureApprovedGemeindeArnegg;
            yield return BundFutureApprovedGemeindeArneggElectoralRegister;
            yield return BundFutureApprovedStadtGossau;
            yield return PoliticalAssemblyBundFutureApprovedGemeindeArnegg;
        }
    }

    public static Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        var all = All.ToList();
        return runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();
            db.VoterListImports.AddRange(all);
            await db.SaveChangesAsync();
        });
    }
}
