// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.PoliticalBusiness;

public class ListPoliticalBusinessesRequestValidatorTest : ProtoValidatorBaseTest<ListPoliticalBusinessesRequest>
{
    protected override IEnumerable<ListPoliticalBusinessesRequest> OkMessages()
    {
        yield return new ListPoliticalBusinessesRequest { ContestId = "91bfb821-adeb-4d9f-b638-0be33af33ca3" };
        yield return new ListPoliticalBusinessesRequest { DomainOfInfluenceId = "ec5f6204-0cc4-42bd-9548-6026ed6a2a43" };
    }

    protected override IEnumerable<ListPoliticalBusinessesRequest> NotOkMessages()
    {
        yield return new ListPoliticalBusinessesRequest { ContestId = "x" };
    }
}
