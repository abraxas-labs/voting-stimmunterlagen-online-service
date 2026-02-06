// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Contest;

public class SetCommunalContestDeadlinesRequestValidatorTest : ProtoValidatorBaseTest<SetCommunalContestDeadlinesRequest>
{
    protected override IEnumerable<SetCommunalContestDeadlinesRequest> OkMessages()
    {
        yield return New();
    }

    protected override IEnumerable<SetCommunalContestDeadlinesRequest> NotOkMessages()
    {
        yield return new SetCommunalContestDeadlinesRequest();
        yield return New(x => x.Id = string.Empty);
        yield return New(x => x.DeliveryToPostDeadlineDate = null);
    }

    private static SetCommunalContestDeadlinesRequest New(Action<SetCommunalContestDeadlinesRequest>? customizer = null)
    {
        var req = new SetCommunalContestDeadlinesRequest
        {
            Id = "a49132be-c691-4aa1-a0f8-5d77e75ee283",
            DeliveryToPostDeadlineDate = MockedClock.GetTimestampDate(1),
        };
        customizer?.Invoke(req);
        return req;
    }
}
