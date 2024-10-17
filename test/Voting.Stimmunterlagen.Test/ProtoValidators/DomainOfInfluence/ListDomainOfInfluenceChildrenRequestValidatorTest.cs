// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.DomainOfInfluence;

public class ListDomainOfInfluenceChildrenRequestValidatorTest : ProtoValidatorBaseTest<ListDomainOfInfluenceChildrenRequest>
{
    protected override IEnumerable<ListDomainOfInfluenceChildrenRequest> OkMessages()
    {
        yield return new() { DomainOfInfluenceId = "1d50e7c9-9c21-4ad6-991f-ba56b7492c23" };
    }

    protected override IEnumerable<ListDomainOfInfluenceChildrenRequest> NotOkMessages()
    {
        yield return new();
        yield return new() { DomainOfInfluenceId = string.Empty };
    }
}
