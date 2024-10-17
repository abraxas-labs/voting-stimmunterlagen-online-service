// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.PrintJob;

public class SetPrintJobDoneRequestValidatorTest : ProtoValidatorBaseTest<SetPrintJobDoneRequest>
{
    protected override IEnumerable<SetPrintJobDoneRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.Comment = string.Empty);
        yield return New(x => x.Comment = RandomStringUtil.GenerateComplexMultiLineText(500));
    }

    protected override IEnumerable<SetPrintJobDoneRequest> NotOkMessages()
    {
        yield return New(x => x.DomainOfInfluenceId = string.Empty);
        yield return New(x => x.Comment = RandomStringUtil.GenerateComplexMultiLineText(501));
    }

    private static SetPrintJobDoneRequest New(Action<SetPrintJobDoneRequest>? customizer = null)
    {
        var req = new SetPrintJobDoneRequest
        {
            DomainOfInfluenceId = "cccec354-2c06-4a29-9908-e65d64b7f649",
            Comment = "comment",
        };
        customizer?.Invoke(req);
        return req;
    }
}
