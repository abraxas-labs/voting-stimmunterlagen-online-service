// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.ElectoralRegister;

public class ListElectoralRegisterFilterVersionsRequestValidatorTest :
    ProtoValidatorBaseTest<ListElectoralRegisterFilterVersionsRequest>
{
    protected override IEnumerable<ListElectoralRegisterFilterVersionsRequest> OkMessages()
    {
        yield return new ListElectoralRegisterFilterVersionsRequest { FilterId = "48a96a95-fba8-47e3-a780-2de564137642" };
    }

    protected override IEnumerable<ListElectoralRegisterFilterVersionsRequest> NotOkMessages()
    {
        yield return new ListElectoralRegisterFilterVersionsRequest();
    }
}
