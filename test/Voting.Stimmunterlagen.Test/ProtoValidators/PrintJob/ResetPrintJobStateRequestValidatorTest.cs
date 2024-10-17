// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.PrintJob;

public class ResetPrintJobStateRequestValidatorTest : ProtoValidatorBaseTest<ResetPrintJobStateRequest>
{
    protected override IEnumerable<ResetPrintJobStateRequest> OkMessages()
    {
        yield return new() { DomainOfInfluenceId = "e32028fe-e447-423f-ae74-87c2fc78e7fb" };
    }

    protected override IEnumerable<ResetPrintJobStateRequest> NotOkMessages()
    {
        yield return new() { DomainOfInfluenceId = string.Empty };
    }
}
