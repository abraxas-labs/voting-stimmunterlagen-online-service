// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.DomainOfInfluenceVotingCardLayout;

public class GetDomainOfInfluenceVotingCardLayoutsRequestValidatorTest :
    ProtoValidatorBaseTest<GetDomainOfInfluenceVotingCardLayoutsRequest>
{
    protected override IEnumerable<GetDomainOfInfluenceVotingCardLayoutsRequest> OkMessages()
    {
        yield return new GetDomainOfInfluenceVotingCardLayoutsRequest { DomainOfInfluenceId = "0505acf2-411c-4be5-a65c-2d2be0176a1b" };
    }

    protected override IEnumerable<GetDomainOfInfluenceVotingCardLayoutsRequest> NotOkMessages()
    {
        yield return new GetDomainOfInfluenceVotingCardLayoutsRequest();
        yield return new GetDomainOfInfluenceVotingCardLayoutsRequest { DomainOfInfluenceId = string.Empty };
    }
}
