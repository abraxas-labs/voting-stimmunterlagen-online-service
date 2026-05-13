// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceVotingCardLayoutTests;

public class VotingCardLayoutBuilderTest : BaseWriteableDbTest
{
    public VotingCardLayoutBuilderTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DomainOfInfluenceVotingCardDataShouldUpdate()
    {
        var id = Guid.Parse("94b5f873-9623-48ed-83f9-72819163b660");
        var contestDomainOfInfluences = await RunOnDb(
            db => db.ContestDomainOfInfluences.Where(x => x.BasisDomainOfInfluenceId == id).Include(x => x.VotingCardLayouts).ToListAsync());
        SetGuidsToNull(contestDomainOfInfluences);
        contestDomainOfInfluences.MatchSnapshot("before-update");

        await TestEventPublisher.Publish(new DomainOfInfluenceVotingCardDataUpdated
        {
            DomainOfInfluenceId = id.ToString(),
            PrintData = new Abraxas.Voting.Basis.Events.V1.Data.DomainOfInfluenceVotingCardPrintDataEventData
            {
                ShippingAway = Abraxas.Voting.Basis.Shared.V1.VotingCardShippingFranking.B1,
                ShippingMethod = Abraxas.Voting.Basis.Shared.V1.VotingCardShippingMethod.PrintingPackagingShippingToMunicipality,
                ShippingReturn = Abraxas.Voting.Basis.Shared.V1.VotingCardShippingFranking.B2,
                ShippingVotingCardsToDeliveryAddress = true,
            },
        });

        var contestDomainOfInfluencesAfterUpdate = await RunOnDb(
            db => db.ContestDomainOfInfluences.Where(x => x.BasisDomainOfInfluenceId == id).Include(x => x.VotingCardLayouts).ToListAsync());
        SetGuidsToNull(contestDomainOfInfluencesAfterUpdate);
        contestDomainOfInfluencesAfterUpdate.MatchSnapshot("after-update");
    }

    [Fact]

    public async Task DomainOfInfluenceVotingCardDataShouldUpdateContestLayout()
    {
        var id = Guid.Parse("d59da8b8-8af3-4082-afe1-db133bc21897");
        var contestVotingCardLayouts =
            await RunOnDb(db =>
                (from cvcl in db.ContestVotingCardLayouts
                 join c in db.Contests
                    on cvcl.ContestId equals c.Id
                 join cdoi in db.ContestDomainOfInfluences
                     on c.DomainOfInfluenceId equals cdoi.Id
                 where cdoi.BasisDomainOfInfluenceId == id
                 orderby cvcl.ContestId
                 select cvcl)
                .ToListAsync());
        SetGuidsToNull(contestVotingCardLayouts);
        contestVotingCardLayouts.MatchSnapshot("before-update");

        await TestEventPublisher.Publish(new DomainOfInfluenceVotingCardDataUpdated
        {
            DomainOfInfluenceId = id.ToString(),
            PrintData = new Abraxas.Voting.Basis.Events.V1.Data.DomainOfInfluenceVotingCardPrintDataEventData
            {
                ShippingAway = Abraxas.Voting.Basis.Shared.V1.VotingCardShippingFranking.B1,
                ShippingMethod = Abraxas.Voting.Basis.Shared.V1.VotingCardShippingMethod.PrintingPackagingShippingToMunicipality,
                ShippingReturn = Abraxas.Voting.Basis.Shared.V1.VotingCardShippingFranking.B2,
                ShippingVotingCardsToDeliveryAddress = true,
            },
        });

        var contestVotingCardLayoutsAfterUpdate =
            await RunOnDb(db =>
                (from cvcl in db.ContestVotingCardLayouts
                 join c in db.Contests
                    on cvcl.ContestId equals c.Id
                 join cdoi in db.ContestDomainOfInfluences
                     on c.DomainOfInfluenceId equals cdoi.Id
                 where cdoi.BasisDomainOfInfluenceId == id
                 orderby cvcl.ContestId
                 select cvcl)
                .ToListAsync());
        SetGuidsToNull(contestVotingCardLayoutsAfterUpdate);
        contestVotingCardLayoutsAfterUpdate.MatchSnapshot("after-update");
    }

    [Fact]
    public async Task DomainOfInfluenceVotingCardDataShouldNotUpdate()
    {
        var id = Guid.Parse("94b5f873-9623-48ed-83f9-72819163b660");
        var steps = await RunOnDb(db =>
            (from s in db.StepStates
             join c in db.ContestDomainOfInfluences
                 on s.DomainOfInfluenceId equals c.Id
             where c.BasisDomainOfInfluenceId == id
                && s.Step == Data.Models.Step.GenerateVotingCards
             select s
            ).ToListAsync());

        await RunOnDb(async db =>
        {
            foreach (var step in steps)
            {
                step.Approved = true;
                db.StepStates.Update(step);
            }

            await db.SaveChangesAsync();
        });
        var contestDomainOfInfluences = await RunOnDb(
            db => db.ContestDomainOfInfluences.Where(x => x.BasisDomainOfInfluenceId == id).Include(x => x.VotingCardLayouts).ToListAsync());
        SetGuidsToNull(contestDomainOfInfluences);
        contestDomainOfInfluences.MatchSnapshot("before-update");
        await TestEventPublisher.Publish(new DomainOfInfluenceVotingCardDataUpdated
        {
            DomainOfInfluenceId = id.ToString(),
            PrintData = new Abraxas.Voting.Basis.Events.V1.Data.DomainOfInfluenceVotingCardPrintDataEventData
            {
                ShippingAway = Abraxas.Voting.Basis.Shared.V1.VotingCardShippingFranking.B1,
                ShippingMethod = Abraxas.Voting.Basis.Shared.V1.VotingCardShippingMethod.PrintingPackagingShippingToMunicipality,
                ShippingReturn = Abraxas.Voting.Basis.Shared.V1.VotingCardShippingFranking.B2,
                ShippingVotingCardsToDeliveryAddress = true,
            },
        });
        var contestDomainOfInfluencesAfterUpdate = await RunOnDb(
            db => db.ContestDomainOfInfluences.Where(x => x.BasisDomainOfInfluenceId == id).Include(x => x.VotingCardLayouts).ToListAsync());
        SetGuidsToNull(contestDomainOfInfluencesAfterUpdate);
        contestDomainOfInfluencesAfterUpdate.MatchSnapshot("after-update");
    }

    private void SetGuidsToNull(List<ContestDomainOfInfluence> contestDomainOfInfluences)
    {
        foreach (var contestDoi in contestDomainOfInfluences)
        {
            contestDoi.Id = Guid.Empty;
            if (contestDoi.VotingCardLayouts is not null)
            {
                foreach (var layout in contestDoi.VotingCardLayouts)
                {
                    layout.Id = Guid.Empty;
                }
            }
        }
    }

    private void SetGuidsToNull(List<ContestVotingCardLayout> contestVotingCardLayouts)
    {
        foreach (var contestLayout in contestVotingCardLayouts)
        {
            contestLayout.Id = Guid.Empty;
        }
    }
}
