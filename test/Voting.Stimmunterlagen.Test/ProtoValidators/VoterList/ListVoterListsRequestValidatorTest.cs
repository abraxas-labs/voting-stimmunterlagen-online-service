// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.VoterList;

public class ListVoterListsRequestValidatorTest : ProtoValidatorBaseTest<ListVoterListsRequest>
{
    protected override IEnumerable<ListVoterListsRequest> OkMessages()
    {
        yield return new ListVoterListsRequest { DomainOfInfluenceId = "40bf6d33-9e7e-4e86-80ae-06110b3921a8" };
    }

    protected override IEnumerable<ListVoterListsRequest> NotOkMessages()
    {
        yield return new ListVoterListsRequest { DomainOfInfluenceId = string.Empty };
        yield return new ListVoterListsRequest();
    }
}
