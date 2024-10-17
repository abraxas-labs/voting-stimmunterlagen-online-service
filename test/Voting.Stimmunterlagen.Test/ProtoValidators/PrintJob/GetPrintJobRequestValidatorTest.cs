// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.PrintJob;

public class GetPrintJobRequestValidatorTest : ProtoValidatorBaseTest<GetPrintJobRequest>
{
    protected override IEnumerable<GetPrintJobRequest> OkMessages()
    {
        yield return new() { DomainOfInfluenceId = "e32028fe-e447-423f-ae74-87c2fc78e7fb" };
    }

    protected override IEnumerable<GetPrintJobRequest> NotOkMessages()
    {
        yield return new() { DomainOfInfluenceId = string.Empty };
    }
}
