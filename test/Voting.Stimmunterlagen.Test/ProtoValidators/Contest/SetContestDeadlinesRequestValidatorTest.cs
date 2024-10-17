// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Contest;

public class SetContestDeadlinesRequestValidatorTest : ProtoValidatorBaseTest<SetContestDeadlinesRequest>
{
    protected override IEnumerable<SetContestDeadlinesRequest> OkMessages()
    {
        yield return New();
    }

    protected override IEnumerable<SetContestDeadlinesRequest> NotOkMessages()
    {
        yield return new SetContestDeadlinesRequest();
        yield return New(x => x.Id = string.Empty);
        yield return New(x => x.PrintingCenterSignUpDeadlineDate = null);
        yield return New(x => x.AttachmentDeliveryDeadlineDate = null);
        yield return New(x => x.GenerateVotingCardsDeadlineDate = null);
    }

    private static SetContestDeadlinesRequest New(Action<SetContestDeadlinesRequest>? customizer = null)
    {
        var req = new SetContestDeadlinesRequest
        {
            Id = "a49132be-c691-4aa1-a0f8-5d77e75ee283",
            PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(1),
            AttachmentDeliveryDeadlineDate = MockedClock.GetTimestampDate(1),
            GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(1),
        };
        customizer?.Invoke(req);
        return req;
    }
}
