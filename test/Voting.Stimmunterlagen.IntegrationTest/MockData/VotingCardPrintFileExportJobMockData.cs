// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class VotingCardPrintFileExportJobMockData
{
    public const string BundFutureApprovedGemeindeArneggJob1Id = "a0fd9c4f-f73d-4ad6-a898-2df4c901167e";
    public const string BundFutureApprovedGemeindeArneggJob2Id = "036ec07d-f7a4-457e-9144-13ac8f91a4a5";
    public const string BundFutureApprovedGemeindeArneggJob3Id = "9e6b1dcf-cf14-4923-b000-2ffc4de7eb60";

    public static readonly Guid BundFutureApprovedGemeindeArneggJob1Guid = Guid.Parse(BundFutureApprovedGemeindeArneggJob1Id);
    public static readonly Guid BundFutureApprovedGemeindeArneggJob2Guid = Guid.Parse(BundFutureApprovedGemeindeArneggJob2Id);
    public static readonly Guid BundFutureApprovedGemeindeArneggJob3Guid = Guid.Parse(BundFutureApprovedGemeindeArneggJob3Id);

    private static VotingCardPrintFileExportJob BundFutureApprovedGemeindeArneggJob1 => new()
    {
        Id = BundFutureApprovedGemeindeArneggJob1Guid,
        State = ExportJobState.ReadyToRun,
        FileName = "de.csv",
        VotingCardGeneratorJobId = VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid,
    };

    private static VotingCardPrintFileExportJob BundFutureApprovedGemeindeArneggJob2 => new()
    {
        Id = BundFutureApprovedGemeindeArneggJob2Guid,
        State = ExportJobState.ReadyToRun,
        FileName = "it.csv",
        VotingCardGeneratorJobId = VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob2Guid,
    };

    private static VotingCardPrintFileExportJob BundFutureApprovedGemeindeArneggJob3 => new()
    {
        Id = BundFutureApprovedGemeindeArneggJob3Guid,
        State = ExportJobState.ReadyToRun,
        FileName = "fr.csv",
        VotingCardGeneratorJobId = VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob3Guid,
    };

    private static IEnumerable<VotingCardPrintFileExportJob> All
    {
        get
        {
            yield return BundFutureApprovedGemeindeArneggJob1;
            yield return BundFutureApprovedGemeindeArneggJob2;
            yield return BundFutureApprovedGemeindeArneggJob3;
        }
    }

    public static Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        return runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();
            db.VotingCardPrintFileExportJobs.AddRange(All);
            await db.SaveChangesAsync();
        });
    }
}
