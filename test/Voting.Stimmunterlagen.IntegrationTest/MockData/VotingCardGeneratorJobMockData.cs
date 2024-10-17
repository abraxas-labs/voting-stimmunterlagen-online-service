// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class VotingCardGeneratorJobMockData
{
    public const string BundFutureApprovedGemeindeArneggJob1Id = "d1586336-05d9-49e7-b208-f6e20def0f80";
    public const string BundFutureApprovedGemeindeArneggJob2Id = "638276f2-2c2d-430c-8f66-eefb1473bea3";
    public const string BundFutureApprovedGemeindeArneggJob3Id = "5af3fcaf-ebf5-4c93-9ff5-26e3193e8f51";

    public static readonly Guid BundFutureApprovedGemeindeArneggJob1Guid = Guid.Parse(BundFutureApprovedGemeindeArneggJob1Id);
    public static readonly Guid BundFutureApprovedGemeindeArneggJob2Guid = Guid.Parse(BundFutureApprovedGemeindeArneggJob2Id);
    public static readonly Guid BundFutureApprovedGemeindeArneggJob3Guid = Guid.Parse(BundFutureApprovedGemeindeArneggJob3Id);

    private static VotingCardGeneratorJob BundFutureApprovedGemeindeArneggJob1 => new()
    {
        Id = BundFutureApprovedGemeindeArneggJob1Guid,
        State = VotingCardGeneratorJobState.Ready,
        FileName = "de.pdf",
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        CountOfVoters = 3,
        Layout = new DomainOfInfluenceVotingCardLayout
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            VotingCardType = VotingCardType.Swiss,
        },
    };

    private static VotingCardGeneratorJob BundFutureApprovedGemeindeArneggJob2 => new()
    {
        Id = BundFutureApprovedGemeindeArneggJob2Guid,
        State = VotingCardGeneratorJobState.ReadyToRunOffline,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        FileName = "it.pdf",
        CountOfVoters = 2,
    };

    private static VotingCardGeneratorJob BundFutureApprovedGemeindeArneggJob3 => new()
    {
        Id = BundFutureApprovedGemeindeArneggJob3Guid,
        State = VotingCardGeneratorJobState.ReadyToRunOffline,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        FileName = "de.pdf",
        CountOfVoters = 2,
    };

    private static IEnumerable<VotingCardGeneratorJob> All
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
            var all = All.ToList();
            var layouts = await db.DomainOfInfluenceVotingCardLayouts.ToListAsync();
            var layoutsByDoiIdAndVcType = layouts.ToDictionary(x => (x.DomainOfInfluenceId, x.VotingCardType));

            foreach (var job in all.Where(vc => vc.State != VotingCardGeneratorJobState.ReadyToRunOffline))
            {
                job.LayoutId = layoutsByDoiIdAndVcType[(job.Layout!.DomainOfInfluenceId, job.Layout.VotingCardType)].Id;
                job.Layout = null!;
            }

            db.VotingCardGeneratorJobs.AddRange(all);
            await db.SaveChangesAsync();
        });
    }
}
