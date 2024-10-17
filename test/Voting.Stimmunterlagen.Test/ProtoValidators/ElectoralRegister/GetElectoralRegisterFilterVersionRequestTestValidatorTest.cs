// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.ElectoralRegister;

public class GetElectoralRegisterFilterVersionRequestTestValidatorTest :
    ProtoValidatorBaseTest<GetElectoralRegisterFilterVersionRequest>
{
    protected override IEnumerable<GetElectoralRegisterFilterVersionRequest> OkMessages()
    {
        yield return new GetElectoralRegisterFilterVersionRequest { FilterVersionId = "48a96a95-fba8-47e3-a780-2de564137642" };
    }

    protected override IEnumerable<GetElectoralRegisterFilterVersionRequest> NotOkMessages()
    {
        yield return new GetElectoralRegisterFilterVersionRequest();
    }
}
