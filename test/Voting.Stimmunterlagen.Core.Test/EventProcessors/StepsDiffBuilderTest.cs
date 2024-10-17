// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Data.Models;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.EventProcessors;

public class StepsDiffBuilderTest
{
    private static readonly Guid Id = Guid.Parse("6bec3e5e-c27b-40cf-b5b2-3f2ac4ced070");

    [Fact]
    public void BuildStepsDiffWithoutExistingShouldBuildCorrectDiffForNoContestRole()
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.None,
            Array.Empty<PoliticalBusinessRole>(),
            true,
            false,
            false,
            false,
            true);
        toRemove.Should().BeEmpty();
        toAdd.Should().BeEmpty();
    }

    [Fact]
    public void BuildStepsDiffWithoutExistingShouldBuildCorrectDiffForContestManager()
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.Manager,
            Array.Empty<PoliticalBusinessRole>(),
            false,
            false,
            false,
            false,
            true);
        toRemove.Should().BeEmpty();
        toAdd.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();
        toAdd.Select(x => x.Step).Should().BeEquivalentTo(
            new[]
            {
                    Step.PoliticalBusinessesApproval,
                    Step.LayoutVotingCardsContestManager,
                    Step.LayoutVotingCardsDomainOfInfluences,
                    Step.ContestApproval,
                    Step.ContestOverview,
            });
    }

    [Fact]
    public void BuildStepsDiffWithoutExistingShouldBuildCorrectDiffForContestManagerAndPoliticalBusinessManager()
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.Manager,
            new[] { PoliticalBusinessRole.Manager },
            false,
            false,
            false,
            false,
            true);
        toRemove.Should().BeEmpty();
        toAdd.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();
        toAdd.Select(x => x.Step).Should().BeEquivalentTo(
            new[]
            {
                    Step.PoliticalBusinessesApproval,
                    Step.LayoutVotingCardsContestManager,
                    Step.LayoutVotingCardsDomainOfInfluences,
                    Step.ContestApproval,
                    Step.Attachments,
                    Step.PrintJobOverview,
                    Step.ContestOverview,
            });
    }

    [Fact]
    public void BuildStepsDiffWithoutExistingShouldBuildCorrectDiffForContestManagerAndPoliticalBusinessManagerAndAttendee()
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.Manager,
            new[] { PoliticalBusinessRole.Manager, PoliticalBusinessRole.Attendee },
            false,
            false,
            false,
            false,
            true);
        toRemove.Should().BeEmpty();
        toAdd.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();
        toAdd.Select(x => x.Step).Should().BeEquivalentTo(
            new[]
            {
                    Step.PoliticalBusinessesApproval,
                    Step.LayoutVotingCardsContestManager,
                    Step.LayoutVotingCardsDomainOfInfluences,
                    Step.LayoutVotingCardsPoliticalBusinessAttendee,
                    Step.ContestApproval,
                    Step.Attachments,
                    Step.VoterLists,
                    Step.GenerateVotingCards,
                    Step.PrintJobOverview,
                    Step.GenerateManualVotingCards,
                    Step.ContestOverview,
                    Step.VotingJournal,
            });
    }

    [Fact]
    public void BuildStepsDiffWithExistingShouldBuildCorrectDiffForPoliticalBusinessManagerAndAttendee()
    {
        var existingStepStates = new List<StepState>
            {
                new StepState { Step = Step.PoliticalBusinessesApproval, DomainOfInfluenceId = Id },
                new StepState { Step = Step.ContestApproval, DomainOfInfluenceId = Id },
                new StepState { Step = Step.LayoutVotingCardsContestManager, DomainOfInfluenceId = Id },
            };
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            existingStepStates,
            ContestRole.Attendee,
            new[] { PoliticalBusinessRole.Manager, PoliticalBusinessRole.Attendee },
            false,
            false,
            false,
            false,
            true);
        toRemove.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();
        toAdd.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();

        toRemove.Select(x => x.Step).Should().BeEquivalentTo(
            new[]
            {
                    Step.LayoutVotingCardsContestManager,
                    Step.ContestApproval,
            });

        toAdd.Select(x => x.Step).Should().BeEquivalentTo(
            new[]
            {
                    Step.LayoutVotingCardsPoliticalBusinessAttendee,
                    Step.Attachments,
                    Step.VoterLists,
                    Step.GenerateVotingCards,
                    Step.PrintJobOverview,
                    Step.GenerateManualVotingCards,
                    Step.VotingJournal,
            });
    }

    [Fact]
    public void BuildStepsDiffWithoutExistingShouldBuildCorrectDiffForContestManagerAndPoliticalBusinessManagerAndAttendeeAsSingle()
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.Manager,
            new[] { PoliticalBusinessRole.Manager, PoliticalBusinessRole.Attendee },
            true,
            false,
            false,
            false,
            true);
        toRemove.Should().BeEmpty();
        toAdd.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();
        toAdd.Select(x => x.Step).Should().BeEquivalentTo(
            new[]
            {
                    Step.PoliticalBusinessesApproval,
                    Step.LayoutVotingCardsPoliticalBusinessAttendee,
                    Step.ContestApproval,
                    Step.Attachments,
                    Step.VoterLists,
                    Step.GenerateVotingCards,
                    Step.PrintJobOverview,
                    Step.GenerateManualVotingCards,
                    Step.ContestOverview,
                    Step.VotingJournal,
            });
    }

    [Fact]
    public void BuildStepsDiffWithoutExistingShouldBuildCorrectDiffForPoliticalBusinessManager()
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.Attendee,
            new[] { PoliticalBusinessRole.Manager },
            false,
            false,
            false,
            false,
            true);
        toRemove.Should().BeEmpty();
        toAdd.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();
        toAdd.Select(x => x.Step).Should().BeEquivalentTo(
            new[]
            {
                    Step.PoliticalBusinessesApproval,
                    Step.Attachments,
                    Step.PrintJobOverview,
            });
    }

    [Fact]
    public void BuildStepsDiffWithoutExistingShouldBuildCorrectDiffForPoliticalBusinessManagerAndAttendee()
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.Attendee,
            new[] { PoliticalBusinessRole.Manager, PoliticalBusinessRole.Attendee },
            false,
            false,
            false,
            false,
            true);
        toRemove.Should().BeEmpty();
        toAdd.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();
        toAdd.Select(x => x.Step).Should().BeEquivalentTo(
            new[]
            {
                    Step.PoliticalBusinessesApproval,
                    Step.LayoutVotingCardsPoliticalBusinessAttendee,
                    Step.Attachments,
                    Step.VoterLists,
                    Step.GenerateVotingCards,
                    Step.PrintJobOverview,
                    Step.GenerateManualVotingCards,
                    Step.VotingJournal,
            });
    }

    [Fact]
    public void BuildStepsDiffWithExistingShouldBuildCorrectDiffForPoliticalBusinessAttendee()
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.Attendee,
            new[] { PoliticalBusinessRole.Attendee },
            false,
            false,
            false,
            false,
            true);
        toRemove.Should().BeEmpty();
        toAdd.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();
        toAdd
            .Select(x => x.Step)
            .Should()
            .BeEquivalentTo(
                new[]
                {
                        Step.PoliticalBusinessesApproval,
                        Step.LayoutVotingCardsPoliticalBusinessAttendee,
                        Step.Attachments,
                        Step.VoterLists,
                        Step.GenerateVotingCards,
                        Step.PrintJobOverview,
                        Step.GenerateManualVotingCards,
                        Step.VotingJournal,
                });
    }

    [Fact]
    public void BuildStepsDiffWithoutExistingShouldBuildCorrectDiffForPoliticalBusinessAttendeeAndEVotingContest()
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.Attendee,
            new[] { PoliticalBusinessRole.Attendee },
            false,
            true,
            false,
            false,
            true);
        toRemove.Should().BeEmpty();
        toAdd.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();
        toAdd
            .Select(x => x.Step)
            .Should()
            .BeEquivalentTo(
                new[]
                {
                        Step.PoliticalBusinessesApproval,
                        Step.LayoutVotingCardsPoliticalBusinessAttendee,
                        Step.Attachments,
                        Step.VoterLists,
                        Step.GenerateVotingCards,
                        Step.PrintJobOverview,
                        Step.GenerateManualVotingCards,
                        Step.VotingJournal,
                });
    }

    [Fact]
    public void BuildStepsDiffWithoutExistingShouldBuildCorrectDiffForContestManagerAndEVotingContest()
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.Manager,
            Array.Empty<PoliticalBusinessRole>(),
            false,
            true,
            false,
            false,
            true);
        toRemove.Should().BeEmpty();
        toAdd.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();
        toAdd.Select(x => x.Step).Should().BeEquivalentTo(
            new[]
            {
                    Step.PoliticalBusinessesApproval,
                    Step.LayoutVotingCardsContestManager,
                    Step.LayoutVotingCardsDomainOfInfluences,
                    Step.ContestApproval,
                    Step.EVoting,
                    Step.ContestOverview,
            });
    }

    [Fact]
    public void BuildStepsDiffWithoutExistingShouldBuildCorrectDiffWithExternalPrintingCenterAsPbAttendee()
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.Attendee,
            new[] { PoliticalBusinessRole.Attendee },
            false,
            true,
            true,
            false,
            true);

        toRemove.Should().BeEmpty();
        toAdd.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();
        toAdd.Select(x => x.Step).Should().BeEquivalentTo(
            new[]
            {
                Step.PoliticalBusinessesApproval,
                Step.LayoutVotingCardsPoliticalBusinessAttendee,
                Step.Attachments,
                Step.VoterLists,
                Step.GenerateVotingCards,
                Step.PrintJobOverview,
                Step.GenerateManualVotingCards,
                Step.VotingJournal,
            });
    }

    [Theory]
    [InlineData(PoliticalBusinessRole.Manager, false, true)]
    [InlineData(PoliticalBusinessRole.Manager, true, false)]
    [InlineData(PoliticalBusinessRole.Attendee, false, true)]
    [InlineData(PoliticalBusinessRole.Attendee, true, true)]
    public void BuildStepsDiffWithExternalPrintingCenter(PoliticalBusinessRole pbRole, bool externalPrintingCenter, bool shouldAddAttachmentStep)
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.Attendee,
            new[] { pbRole },
            false,
            false,
            externalPrintingCenter,
            false,
            true);

        var isAttachmentStepAdded = toAdd.Any(x => x.Step == Step.Attachments);
        isAttachmentStepAdded.Should().Be(shouldAddAttachmentStep);
    }

    [Fact]
    public void BuildStepsDiffWithoutExistingShouldBuildCorrectDiffForContestManagerInPoliticalAssembly()
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.Manager,
            Array.Empty<PoliticalBusinessRole>(),
            false,
            false,
            false,
            true,
            true);
        toRemove.Should().BeEmpty();
        toAdd.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();
        toAdd.Select(x => x.Step).Should().BeEquivalentTo(
            new[]
            {
                Step.LayoutVotingCardsContestManager,
                Step.LayoutVotingCardsDomainOfInfluences,
                Step.LayoutVotingCardsPoliticalBusinessAttendee,
                Step.ContestApproval,
                Step.Attachments,
                Step.VoterLists,
                Step.GenerateVotingCards,
                Step.PrintJobOverview,
                Step.GenerateManualVotingCards,
                Step.ContestOverview,
                Step.VotingJournal,
            });
    }

    [Fact]
    public void BuildStepsDiffWithoutExistingShouldBuildCorrectDiffForContestManagerAsSingleAttendeeInPoliticalAssembly()
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.Manager,
            Array.Empty<PoliticalBusinessRole>(),
            true,
            false,
            false,
            true,
            true);
        toRemove.Should().BeEmpty();
        toAdd.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();
        toAdd.Select(x => x.Step).Should().BeEquivalentTo(
            new[]
            {
                Step.LayoutVotingCardsPoliticalBusinessAttendee,
                Step.ContestApproval,
                Step.Attachments,
                Step.VoterLists,
                Step.GenerateVotingCards,
                Step.PrintJobOverview,
                Step.GenerateManualVotingCards,
                Step.ContestOverview,
                Step.VotingJournal,
            });
    }

    [Fact]
    public void BuildStepsDiffWithoutExistingShouldBuildCorrectDiffForContestAttendeeInPoliticalAssembly()
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.Attendee,
            Array.Empty<PoliticalBusinessRole>(),
            false,
            false,
            false,
            true,
            true);
        toRemove.Should().BeEmpty();
        toAdd.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();
        toAdd.Select(x => x.Step).Should().BeEquivalentTo(
            new[]
            {
                Step.LayoutVotingCardsPoliticalBusinessAttendee,
                Step.Attachments,
                Step.VoterLists,
                Step.GenerateVotingCards,
                Step.PrintJobOverview,
                Step.GenerateManualVotingCards,
                Step.VotingJournal,
            });
    }

    [Fact]
    public void BuildStepsDiffWithoutExistingShouldBuildCorrectDiffForContestAttendeeInPoliticalAssemblyWithoutResponsibleForVotingCards()
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.Attendee,
            Array.Empty<PoliticalBusinessRole>(),
            false,
            false,
            false,
            true,
            false);
        toRemove.Should().BeEmpty();
        toAdd.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();
        toAdd.Select(x => x.Step).Should().BeEquivalentTo(Array.Empty<Step>());
    }

    [Fact]
    public void BuildStepsDiffWithoutExistingShouldBuildCorrectDiffWithExternalPrintingCenterInPoliticalAssembly()
    {
        var (toRemove, toAdd) = StepsDiffBuilder.BuildStepsDiff(
            Id,
            null,
            ContestRole.Attendee,
            Array.Empty<PoliticalBusinessRole>(),
            false,
            false,
            true,
            true,
            true);

        toRemove.Should().BeEmpty();
        toAdd.All(x => x.DomainOfInfluenceId == Id).Should().BeTrue();
        toAdd.Select(x => x.Step).Should().BeEquivalentTo(
            new[]
            {
                Step.LayoutVotingCardsPoliticalBusinessAttendee,
                Step.VoterLists,
                Step.GenerateVotingCards,
                Step.PrintJobOverview,
                Step.GenerateManualVotingCards,
                Step.VotingJournal,
            });
    }
}
