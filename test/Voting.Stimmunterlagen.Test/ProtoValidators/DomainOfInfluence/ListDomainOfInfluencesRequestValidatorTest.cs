// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.DomainOfInfluence;

public class ListDomainOfInfluencesRequestValidatorTest : ProtoValidatorBaseTest<ListDomainOfInfluencesRequest>
{
    protected override IEnumerable<ListDomainOfInfluencesRequest> OkMessages()
    {
        yield return new ListDomainOfInfluencesRequest { ContestId = "2a7cd3de-ab98-47c4-ab0d-17446ef7682a" };
    }

    protected override IEnumerable<ListDomainOfInfluencesRequest> NotOkMessages()
    {
        yield return new ListDomainOfInfluencesRequest { ContestId = string.Empty };
    }
}
