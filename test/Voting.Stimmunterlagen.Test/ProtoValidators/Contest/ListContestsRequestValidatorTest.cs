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
    protected override IEnumerable<ListContestsRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.States.Clear());
        yield return New(x => x.Pageable.Page = 1_000_000);
        yield return New(x => x.Pageable.PageSize = 1);
        yield return New(x => x.Pageable.PageSize = 100);
        yield return New(x => x.Pageable = null);
    }

    protected override IEnumerable<ListContestsRequest> NotOkMessages()
    {
        yield return New(x => x.States.Add(ContestState.Unspecified));
        yield return New(x => x.States.Add((ContestState)(-1)));
        yield return New(x => x.Pageable.Page = 0);
        yield return New(x => x.Pageable.Page = 1_000_001);
        yield return New(x => x.Pageable.PageSize = 0);
        yield return New(x => x.Pageable.PageSize = 101);
    }

    private static ListContestsRequest New(Action<ListContestsRequest>? customizer = null)
    {
        var req = new ListContestsRequest
        {
            States = { ContestState.Active },
            Pageable = new Pageable { Page = 1, PageSize = 10 },
        };
        customizer?.Invoke(req);
        return req;
    }
}
