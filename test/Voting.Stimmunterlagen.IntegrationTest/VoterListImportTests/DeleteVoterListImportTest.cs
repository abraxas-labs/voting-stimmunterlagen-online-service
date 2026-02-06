// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.VoterListImportTests;

public class DeleteVoterListImportTest : BaseWriteableDbGrpcTest<VoterListImportService.VoterListImportServiceClient>
{
    public DeleteVoterListImportTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldDelete()
    {
        await GemeindeArneggElectionAdminClient.DeleteAsync(new IdValueRequest { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggId });

        (await RunOnDb(db => db.VoterLists
            .Where(vl => vl.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .SumAsync(vl => vl.CountOfVotingCards))).Should().Be(2);

        await GemeindeArneggElectionAdminClient.DeleteAsync(new IdValueRequest { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggElectoralRegisterId });

        (await RunOnDb(db => db.VoterLists
            .Where(vl => vl.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .SumAsync(vl => vl.CountOfVotingCards))).Should().Be(0);

        RunOnDb(db => db.VoterListImports.Any(x => x.Id == VoterListImportMockData.BundFutureApprovedGemeindeArneggGuid))
            .Should()
            .BeFalse();

        var attachment = await RunOnDb(db => db.Attachments
            .Include(x => x.DomainOfInfluenceAttachmentCounts)
            .FirstAsync(x => x.Id == AttachmentMockData.BundFutureApprovedGemeindeArneggGuid));
        attachment.TotalRequiredForVoterListsCount.Should().Be(0);
        attachment.DomainOfInfluenceAttachmentCounts!
            .Single(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .RequiredForVoterListsCount
            .Should()
            .Be(0);
    }

    [Fact]
    public async Task ShouldUpdateDomainOfInfluenceLastVoterUpdate()
    {
        var doi = await RunOnDb(db => db.ContestDomainOfInfluences.FirstAsync(doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid));
        doi.LastVoterUpdate.Should().BeNull();

        await GemeindeArneggElectionAdminClient.DeleteAsync(new IdValueRequest { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggId });

        doi = await RunOnDb(db => db.ContestDomainOfInfluences.FirstAsync(doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid));
        doi.LastVoterUpdate.Should().Be(MockedClock.GetDate());
    }

    [Fact]
    public async Task DeleteLatestImportShouldEnableVotingCardPrintOnPreviousImport()
    {
        var doiId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid;
        var vlId1 = VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid;
        var vlId2 = VoterListMockData.BundFutureApprovedGemeindeArneggSwissElectoralRegisterGuid;

        var voterLists = await RunOnDb(db => db.VoterLists
            .Where(vl => vl.DomainOfInfluenceId == doiId)
            .OrderBy(vl => vl.Id)
            .Include(vl => vl.Voters!.OrderBy(v => v.PersonId))
            .ToListAsync());

        var voter1 = voterLists.Single(vl => vl.Id == vlId1).Voters!.First();
        var voter2 = voterLists.Single(vl => vl.Id == vlId2).Voters!.First();

        var voter1Id = voter1.Id;

        var duplicate = new DomainOfInfluenceVoterDuplicate
        {
            DomainOfInfluenceId = doiId,
        };

        await RunOnDb(async db =>
        {
            db.DomainOfInfluenceVoterDuplicates.Add(duplicate);
            voter1.VoterDuplicateId = duplicate.Id;
            voter1.VotingCardPrintDisabled = true;
            voter2.VoterDuplicateId = duplicate.Id;
            db.Voters.Update(voter1);
            db.Voters.Update(voter2);
            await db.SaveChangesAsync();
        });

        await RunScoped<VoterListRepo>(r => r.UpdateVotingCardCounts(doiId));

        voterLists = await RunOnDb(db => db.VoterLists
                    .Where(vl => vl.DomainOfInfluenceId == doiId)
                    .Include(vl => vl.Voters!.OrderBy(v => v.PersonId))
                    .ToListAsync());

        voterLists.Sum(vl => vl.CountOfVotingCards).Should().Be(8);

        var voterList1 = voterLists.Single(vl => vl.Id == vlId1);
        voterList1.CountOfVotingCards.Should().Be(2);
        voterList1.Voters.Should().HaveCount(3);
        voterList1.Voters!.Any(v => v.Id == voter1Id && v.VotingCardPrintDisabled).Should().BeTrue();

        voterLists.Single(vl => vl.Id == vlId1).CountOfVotingCards.Should().Be(2);
        voterLists.Single(vl => vl.Id == vlId2).CountOfVotingCards.Should().Be(2);
        voterLists[1].CountOfVotingCards.Should().Be(2);

        await GemeindeArneggElectionAdminClient.DeleteAsync(new IdValueRequest { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggElectoralRegisterId });

        voterLists = await RunOnDb(db => db.VoterLists
            .Where(vl => vl.DomainOfInfluenceId == doiId)
            .OrderBy(vl => vl.Id)
            .Include(vl => vl.Voters!.OrderBy(v => v.PersonId))
            .ToListAsync());

        // 2 Voters got deleted, but 1 Voter got his print enabled (because the matching one which got deleted was the "active" one / print was enabled).
        // Now to ensure that the voter still gets his voting card, the latest matching voter will get print enabled.
        voterLists.Sum(vl => vl.CountOfVotingCards).Should().Be(7);

        voterList1 = voterLists.Single(vl => vl.Id == vlId1);
        voterList1.CountOfVotingCards.Should().Be(3);
        voterList1.Voters!.Any(v => v.Id == voter1Id && !v.VotingCardPrintDisabled).Should().BeTrue();

        (await RunOnDb(db => db.DomainOfInfluenceVoterDuplicates.AnyAsync(d => d.DomainOfInfluenceId == doiId)))
            .Should().BeFalse();
    }

    [Fact]
    public async Task DeleteOldImportShouldCleanUpDuplicates()
    {
        var doiId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid;
        var vlId1 = VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid;
        var vlId2 = VoterListMockData.BundFutureApprovedGemeindeArneggSwissElectoralRegisterGuid;

        var voterLists = await RunOnDb(db => db.VoterLists
            .Where(vl => vl.DomainOfInfluenceId == doiId)
            .OrderBy(vl => vl.Id)
            .Include(vl => vl.Voters!.OrderBy(v => v.PersonId))
            .ToListAsync());

        var voter1 = voterLists.Single(vl => vl.Id == vlId1).Voters!.First();
        var voter2 = voterLists.Single(vl => vl.Id == vlId2).Voters!.First();
        var voter2Id = voter2.Id;
        var duplicate = new DomainOfInfluenceVoterDuplicate
        {
            DomainOfInfluenceId = doiId,
        };

        await RunOnDb(async db =>
        {
            db.DomainOfInfluenceVoterDuplicates.Add(duplicate);
            voter1.VoterDuplicateId = duplicate.Id;
            voter1.VotingCardPrintDisabled = true;
            voter2.VoterDuplicateId = duplicate.Id;
            db.Voters.Update(voter1);
            db.Voters.Update(voter2);
            await db.SaveChangesAsync();
        });

        await RunScoped<VoterListRepo>(r => r.UpdateVotingCardCounts(doiId));

        voterLists = await RunOnDb(db => db.VoterLists
            .Where(vl => vl.DomainOfInfluenceId == doiId)
            .OrderBy(vl => vl.Id)
            .Include(vl => vl.Voters!.OrderBy(v => v.PersonId))
            .ToListAsync());

        var voterList2 = voterLists.Single(vl => vl.Id == vlId2);
        voterList2.CountOfVotingCards.Should().Be(2);
        voterList2.Voters.Should().HaveCount(2);
        voterList2.Voters!.Any(v => v.VoterDuplicateId.HasValue && v.Id == voter2Id && !v.VotingCardPrintDisabled).Should().BeTrue();

        await GemeindeArneggElectionAdminClient.DeleteAsync(new IdValueRequest { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggId });

        voterLists = await RunOnDb(db => db.VoterLists
            .Where(vl => vl.DomainOfInfluenceId == doiId)
            .OrderBy(vl => vl.Id)
            .Include(vl => vl.Voters!.OrderBy(v => v.PersonId))
            .ToListAsync());

        // The duplicate entry should be deleted, because after the delete only 1 voter with the same voter key remains.
        voterLists.Sum(vl => vl.CountOfVotingCards).Should().Be(2);

        voterList2 = voterLists.Single(vl => vl.Id == vlId2);
        voterList2.CountOfVotingCards.Should().Be(2);
        voterList2.Voters.Should().HaveCount(2);
        voterList2.Voters!.Any(v => !v.VoterDuplicateId.HasValue && v.Id == voter2Id && !v.VotingCardPrintDisabled).Should().BeTrue();

        (await RunOnDb(db => db.DomainOfInfluenceVoterDuplicates.AnyAsync(d => d.DomainOfInfluenceId == doiId)))
            .Should().BeFalse();
    }

    [Fact]
    public async Task ShouldThrowIfNotFound()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.DeleteAsync(new IdValueRequest { Id = "66875c4c-4bc4-4eba-9f18-a5ecefaa6c99" }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.DeleteAsync(new IdValueRequest { Id = VoterListImportMockData.BundArchivedGemeindeArneggId }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfPastGenerateVotingCardsDeadline()
    {
        await SetContestBundFutureApprovedToPastGenerateVotingCardsDeadline();
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.DeleteAsync(new() { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggId }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfNotOwner()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.DeleteAsync(new IdValueRequest { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggId }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(VoterListImportService.VoterListImportServiceClient service)
    {
        await service.DeleteAsync(new IdValueRequest { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
