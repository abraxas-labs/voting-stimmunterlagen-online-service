// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.VoterList;

public class AssignPoliticalBusinessVoterListRequestValidatorTest : ProtoValidatorBaseTest<AssignPoliticalBusinessVoterListRequest>
{
    protected override IEnumerable<AssignPoliticalBusinessVoterListRequest> OkMessages()
    {
        yield return new AssignPoliticalBusinessVoterListRequest
        {
            Id = "b8f6d30d-08e2-4e9b-ad87-c8272a1a3fc5",
            PoliticalBusinessId = "2d1130f2-1240-4137-9e68-ed72dddd26a2",
        };
    }

    protected override IEnumerable<AssignPoliticalBusinessVoterListRequest> NotOkMessages()
    {
        yield return new AssignPoliticalBusinessVoterListRequest
        {
            Id = string.Empty,
            PoliticalBusinessId = "e4259578-370a-43df-8961-284d7c91f3e1",
        };
        yield return new AssignPoliticalBusinessVoterListRequest
        {
            Id = "9eeb6d52-8c7f-4eb5-b683-d9f52a6d6362",
            PoliticalBusinessId = string.Empty,
        };
        yield return new AssignPoliticalBusinessVoterListRequest();
    }
}
