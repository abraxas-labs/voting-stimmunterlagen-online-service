// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Step;

public class ListStepsRequestValidatorTest : ProtoValidatorBaseTest<ListStepsRequest>
{
    protected override IEnumerable<ListStepsRequest> OkMessages()
    {
        yield return new ListStepsRequest { DomainOfInfluenceId = "2a7cd3de-ab98-47c4-ab0d-17446ef7682a" };
    }

    protected override IEnumerable<ListStepsRequest> NotOkMessages()
    {
        yield return new ListStepsRequest { DomainOfInfluenceId = string.Empty };
    }
}
