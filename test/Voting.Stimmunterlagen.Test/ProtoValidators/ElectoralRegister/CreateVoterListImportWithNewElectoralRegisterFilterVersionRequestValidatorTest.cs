// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.ElectoralRegister;

public class CreateVoterListImportWithNewElectoralRegisterFilterVersionRequestValidatorTest :
    ProtoValidatorBaseTest<CreateVoterListImportWithNewElectoralRegisterFilterVersionRequest>
{
    protected override IEnumerable<CreateVoterListImportWithNewElectoralRegisterFilterVersionRequest> OkMessages()
    {
        yield return NewRequest();
        yield return NewRequest(x => x.FilterVersionName = RandomStringUtil.GenerateSimpleSingleLineText(2));
        yield return NewRequest(x => x.FilterVersionName = RandomStringUtil.GenerateSimpleSingleLineText(100));
    }

    protected override IEnumerable<CreateVoterListImportWithNewElectoralRegisterFilterVersionRequest> NotOkMessages()
    {
        yield return NewRequest(x => x.FilterId = string.Empty);
        yield return NewRequest(x => x.DomainOfInfluenceId = string.Empty);
        yield return NewRequest(x => x.FilterVersionName = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return NewRequest(x => x.FilterVersionName = RandomStringUtil.GenerateSimpleSingleLineText(101));
        yield return NewRequest(x => x.FilterVersionDeadline = null);
    }

    private static CreateVoterListImportWithNewElectoralRegisterFilterVersionRequest NewRequest(Action<CreateVoterListImportWithNewElectoralRegisterFilterVersionRequest>? customizer = null)
    {
        var request = new CreateVoterListImportWithNewElectoralRegisterFilterVersionRequest
        {
            DomainOfInfluenceId = "8b32251c-cf4e-4387-9a40-1de48f473e97",
            FilterId = "e934b223-5c45-4857-aacf-55cb27d33e25",
            FilterVersionName = "FooBar",
            FilterVersionDeadline = MockedClock.GetTimestampDate(),
        };
        customizer?.Invoke(request);
        return request;
    }
}
