// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.ContestVotingCardLayout;

public class GetContestVotingCardLayoutsRequestValidatorTest :
    ProtoValidatorBaseTest<GetContestVotingCardLayoutsRequest>
{
    protected override IEnumerable<GetContestVotingCardLayoutsRequest> OkMessages()
    {
        yield return new GetContestVotingCardLayoutsRequest { ContestId = "764a9f43-a76f-437b-9173-8e8864dc2787" };
    }

    protected override IEnumerable<GetContestVotingCardLayoutsRequest> NotOkMessages()
    {
        yield return new GetContestVotingCardLayoutsRequest();
    }
}
