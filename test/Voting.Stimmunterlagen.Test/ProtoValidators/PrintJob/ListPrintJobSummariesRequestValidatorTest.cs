// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.PrintJob;

public class ListPrintJobSummariesRequestValidatorTest : ProtoValidatorBaseTest<ListPrintJobSummariesRequest>
{
    protected override IEnumerable<ListPrintJobSummariesRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.Query = string.Empty);
        yield return New(x => x.Query = RandomStringUtil.GenerateSimpleSingleLineText(100));
        yield return New(x => x.State = PrintJobState.Unspecified);
    }

    protected override IEnumerable<ListPrintJobSummariesRequest> NotOkMessages()
    {
        yield return New(x => x.ContestId = string.Empty);
        yield return New(x => x.Query = RandomStringUtil.GenerateSimpleSingleLineText(101));
        yield return New(x => x.State = (PrintJobState)100);
    }

    private static ListPrintJobSummariesRequest New(Action<ListPrintJobSummariesRequest>? customizer = null)
    {
        var req = new ListPrintJobSummariesRequest
        {
            ContestId = "cccec354-2c06-4a29-9908-e65d64b7f649",
            Query = "abc",
            State = PrintJobState.Done,
        };
        customizer?.Invoke(req);
        return req;
    }
}
