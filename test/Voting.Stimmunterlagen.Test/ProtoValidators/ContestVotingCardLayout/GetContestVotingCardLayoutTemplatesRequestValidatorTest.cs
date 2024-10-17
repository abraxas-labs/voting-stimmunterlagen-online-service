// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.ContestVotingCardLayout;

public class GetContestVotingCardLayoutTemplatesRequestValidatorTest :
    ProtoValidatorBaseTest<GetContestVotingCardLayoutTemplatesRequest>
{
    protected override IEnumerable<GetContestVotingCardLayoutTemplatesRequest> OkMessages()
    {
        yield return new() { ContestId = "9aeeabc3-740d-4046-9b73-0ebcceb30050" };
    }

    protected override IEnumerable<GetContestVotingCardLayoutTemplatesRequest> NotOkMessages()
    {
        yield return new();
    }
}
