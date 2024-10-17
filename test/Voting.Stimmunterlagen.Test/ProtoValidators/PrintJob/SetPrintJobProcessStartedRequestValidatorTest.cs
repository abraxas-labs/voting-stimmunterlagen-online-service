// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.PrintJob;

public class SetPrintJobProcessStartedRequestValidatorTest : ProtoValidatorBaseTest<SetPrintJobProcessStartedRequest>
{
    protected override IEnumerable<SetPrintJobProcessStartedRequest> OkMessages()
    {
        yield return new() { DomainOfInfluenceId = "a26a0ff8-260d-46f1-a8c5-551479da9cb4" };
    }

    protected override IEnumerable<SetPrintJobProcessStartedRequest> NotOkMessages()
    {
        yield return new() { DomainOfInfluenceId = string.Empty };
    }
}
