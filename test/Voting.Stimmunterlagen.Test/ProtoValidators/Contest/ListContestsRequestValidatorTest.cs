// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Contest;

public class ListContestsRequestValidatorTest : ProtoValidatorBaseTest<ListContestsRequest>
{
    private static readonly DateTime BaseDate = new(2020, 5, 12, 1, 2, 3, DateTimeKind.Utc);

    protected override IEnumerable<ListContestsRequest> OkMessages()
    {
        yield return new ListContestsRequest();
        yield return new ListContestsRequest
        {
            States =
                {
                    ContestState.Active,
                },
        };
    }

    protected override IEnumerable<ListContestsRequest> NotOkMessages()
    {
        // unspecified
        yield return new ListContestsRequest
        {
            States =
                {
                   ContestState.Unspecified,
                },
        };

        // out of range
        yield return new ListContestsRequest
        {
            States =
                {
                    (ContestState)(-1),
                },
        };
    }
}
