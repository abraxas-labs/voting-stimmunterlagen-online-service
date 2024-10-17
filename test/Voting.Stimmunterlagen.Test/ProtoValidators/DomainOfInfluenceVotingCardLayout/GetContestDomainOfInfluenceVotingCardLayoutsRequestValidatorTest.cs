// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.DomainOfInfluenceVotingCardLayout;

public class GetContestDomainOfInfluenceVotingCardLayoutsRequestValidatorTest :
    ProtoValidatorBaseTest<GetContestDomainOfInfluenceVotingCardLayoutsRequest>
{
    protected override IEnumerable<GetContestDomainOfInfluenceVotingCardLayoutsRequest> OkMessages()
    {
        yield return new GetContestDomainOfInfluenceVotingCardLayoutsRequest { ContestId = "e7635456-9541-4615-bf31-be0a7e037e86" };
    }

    protected override IEnumerable<GetContestDomainOfInfluenceVotingCardLayoutsRequest> NotOkMessages()
    {
        yield return new GetContestDomainOfInfluenceVotingCardLayoutsRequest();
        yield return new GetContestDomainOfInfluenceVotingCardLayoutsRequest { ContestId = string.Empty };
    }
}
