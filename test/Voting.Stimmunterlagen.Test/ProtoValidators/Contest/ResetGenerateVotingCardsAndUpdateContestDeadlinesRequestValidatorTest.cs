// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Contest;

public class ResetGenerateVotingCardsAndUpdateContestDeadlinesRequestValidatorTest : ProtoValidatorBaseTest<ResetGenerateVotingCardsAndUpdateContestDeadlinesRequest>
{
    protected override IEnumerable<ResetGenerateVotingCardsAndUpdateContestDeadlinesRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.ResetGenerateVotingCardsTriggeredDomainOfInfluenceIds.Clear());
    }

    protected override IEnumerable<ResetGenerateVotingCardsAndUpdateContestDeadlinesRequest> NotOkMessages()
    {
        yield return New(x => x.Id = string.Empty);
        yield return New(x => x.PrintingCenterSignUpDeadlineDate = null);
        yield return New(x => x.GenerateVotingCardsDeadlineDate = null);
        yield return New(x => x.ResetGenerateVotingCardsTriggeredDomainOfInfluenceIds.Add("a"));
    }

    private static ResetGenerateVotingCardsAndUpdateContestDeadlinesRequest New(Action<ResetGenerateVotingCardsAndUpdateContestDeadlinesRequest>? customizer = null)
    {
        var req = new ResetGenerateVotingCardsAndUpdateContestDeadlinesRequest
        {
            Id = "a49132be-c691-4aa1-a0f8-5d77e75ee283",
            PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(1),
            GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(1),
            ResetGenerateVotingCardsTriggeredDomainOfInfluenceIds = { "a49132be-c691-4aa1-a0f8-5d77e75ee283" },
        };
        customizer?.Invoke(req);
        return req;
    }
}
