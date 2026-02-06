// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Contest;

public class ResetGenerateVotingCardsAndUpdateCommunalContestDeadlinesRequestValidatorTest : ProtoValidatorBaseTest<ResetGenerateVotingCardsAndUpdateCommunalContestDeadlinesRequest>
{
    protected override IEnumerable<ResetGenerateVotingCardsAndUpdateCommunalContestDeadlinesRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.ResetGenerateVotingCardsTriggeredDomainOfInfluenceIds.Clear());
    }

    protected override IEnumerable<ResetGenerateVotingCardsAndUpdateCommunalContestDeadlinesRequest> NotOkMessages()
    {
        yield return New(x => x.Id = string.Empty);
        yield return New(x => x.PrintingCenterSignUpDeadlineDate = null);
        yield return New(x => x.GenerateVotingCardsDeadlineDate = null);
        yield return New(x => x.DeliveryToPostDeadlineDate = null);
        yield return New(x => x.AttachmentDeliveryDeadlineDate = null);
        yield return New(x => x.ResetGenerateVotingCardsTriggeredDomainOfInfluenceIds.Add("a"));
    }

    private static ResetGenerateVotingCardsAndUpdateCommunalContestDeadlinesRequest New(Action<ResetGenerateVotingCardsAndUpdateCommunalContestDeadlinesRequest>? customizer = null)
    {
        var req = new ResetGenerateVotingCardsAndUpdateCommunalContestDeadlinesRequest
        {
            Id = "a49132be-c691-4aa1-a0f8-5d77e75ee283",
            PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(1),
            GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(1),
            AttachmentDeliveryDeadlineDate = MockedClock.GetTimestampDate(1),
            DeliveryToPostDeadlineDate = MockedClock.GetTimestampDate(1),
            ResetGenerateVotingCardsTriggeredDomainOfInfluenceIds = { "a49132be-c691-4aa1-a0f8-5d77e75ee283" },
        };
        customizer?.Invoke(req);
        return req;
    }
}
