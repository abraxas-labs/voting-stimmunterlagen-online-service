// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.DomainOfInfluenceVotingCardLayout;

public class GetDomainOfInfluenceVotingCardLayoutTemplatesRequestValidatorTest :
    ProtoValidatorBaseTest<GetDomainOfInfluenceVotingCardLayoutTemplatesRequest>
{
    protected override IEnumerable<GetDomainOfInfluenceVotingCardLayoutTemplatesRequest> OkMessages()
    {
        yield return new() { ContestId = "552419c5-01d4-43f3-8c29-8872f2193eb1" };
    }

    protected override IEnumerable<GetDomainOfInfluenceVotingCardLayoutTemplatesRequest> NotOkMessages()
    {
        yield return new();
        yield return new() { ContestId = string.Empty };
    }
}
