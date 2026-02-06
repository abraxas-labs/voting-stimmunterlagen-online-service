// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Common;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class ManualVotingCardGeneratorJobMockData
{
    public const string BundFutureApprovedGemeindeArneggSwiss1Id = "f2ce7a05-3ad6-4f16-bbd4-941dbea7b827";
    public const string BundFutureApprovedGemeindeArneggSwiss2Id = "8c93d7d7-4e08-43d4-9715-b9e1ba4b53d8";
    public const string BundFutureApprovedStadtUzwilSwiss1Id = "c252b486-9ab7-4ee0-a0ce-6fd51d2333ac";

    public static readonly Guid BundFutureApprovedGemeindeArneggSwiss1Guid = Guid.Parse(BundFutureApprovedGemeindeArneggSwiss1Id);
    public static readonly Guid BundFutureApprovedGemeindeArneggSwiss2Guid = Guid.Parse(BundFutureApprovedGemeindeArneggSwiss2Id);
    public static readonly Guid BundFutureApprovedStadtUzwilSwiss1Guid = Guid.Parse(BundFutureApprovedStadtUzwilSwiss1Id);

    public static ManualVotingCardGeneratorJob BundFutureApprovedGemeindeArneggSwiss1
        => new()
        {
            Id = BundFutureApprovedGemeindeArneggSwiss1Guid,
            Created = MockedClock.GetDate(-3),
            CreatedBy = UserMockData.GemeindepraesidentArnegg,
            Layout = new()
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            },
            Voter = new()
            {
                Salutation = Salutation.Mister,
                Title = "Dr.",
                FirstName = "Stefan",
                LastName = "Jager",
                Street = "via Stazione",
                AddressLine1 = "gegenüber Schulhaus",
                HouseNumber = "12a",
                Town = "Arnegg",
                SwissZipCode = 5600,
                Country = { Iso2 = "CH", Name = "Schweiz" },
                Bfs = "5566",
                LanguageOfCorrespondence = Languages.German,
                VotingCardType = VotingCardType.Swiss,
                DateOfBirth = "1995-09-21",
                PersonId = "234",
                ContestId = ContestMockData.BundFutureApprovedGuid,
            },
        };

    public static ManualVotingCardGeneratorJob BundFutureApprovedGemeindeArneggSwiss2
        => new()
        {
            Id = BundFutureApprovedGemeindeArneggSwiss2Guid,
            Created = MockedClock.GetDate(-1),
            CreatedBy = UserMockData.GemeindepraesidentArnegg,
            Layout = new()
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            },
            Voter = new()
            {
                FirstName = "Laura",
                LastName = "Ebersbach",
                Street = "Üerklisweg",
                HouseNumber = "68",
                Town = "Arnegg",
                SwissZipCode = 5600,
                Country = { Iso2 = "CH", Name = "Schweiz" },
                Bfs = "5566",
                LanguageOfCorrespondence = Languages.German,
                VotingCardType = VotingCardType.Swiss,
                DateOfBirth = "0",
                ContestId = ContestMockData.BundFutureApprovedGuid,
            },
        };

    public static ManualVotingCardGeneratorJob BundFutureApprovedStadtUzwilSwiss1
        => new()
        {
            Id = BundFutureApprovedStadtUzwilSwiss1Guid,
            Created = MockedClock.GetDate(-2),
            CreatedBy = UserMockData.StadtpraesidentUzwil,
            Layout = new()
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtUzwilGuid,
            },
            Voter = new()
            {
                FirstName = "Anke",
                LastName = "Ritter",
                Street = "Kappelergasse",
                HouseNumber = "61",
                Town = "Uzwil",
                SwissZipCode = 9200,
                Country = { Iso2 = "CH", Name = "Schweiz" },
                Bfs = "1155",
                LanguageOfCorrespondence = Languages.German,
                VotingCardType = VotingCardType.Swiss,
                DateOfBirth = "0",
                ContestId = ContestMockData.BundFutureApprovedGuid,
            },
        };

    public static IEnumerable<ManualVotingCardGeneratorJob> All
    {
        get
        {
            yield return BundFutureApprovedGemeindeArneggSwiss1;
            yield return BundFutureApprovedGemeindeArneggSwiss2;
            yield return BundFutureApprovedStadtUzwilSwiss1;
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

            foreach (var job in all)
            {
                job.LayoutId = layoutsByDoiIdAndVcType[(job.Layout.DomainOfInfluenceId, job.Voter!.VotingCardType)].Id;
                job.Layout = null!;
            }

            db.ManualVotingCardGeneratorJobs.AddRange(all);
            await db.SaveChangesAsync();
        });
    }
}
