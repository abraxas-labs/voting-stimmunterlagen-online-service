// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.ElectoralRegister;

public class UpdateVoterListImportWithNewElectoralRegisterFilterVersionRequestValidatorTest :
    ProtoValidatorBaseTest<UpdateVoterListImportWithNewElectoralRegisterFilterVersionRequest>
{
    protected override IEnumerable<UpdateVoterListImportWithNewElectoralRegisterFilterVersionRequest> OkMessages()
    {
        yield return NewRequest();
        yield return NewRequest(x => x.FilterVersionName = RandomStringUtil.GenerateSimpleSingleLineText(2));
        yield return NewRequest(x => x.FilterVersionName = RandomStringUtil.GenerateSimpleSingleLineText(100));
    }

    protected override IEnumerable<UpdateVoterListImportWithNewElectoralRegisterFilterVersionRequest> NotOkMessages()
    {
        yield return NewRequest(x => x.FilterId = string.Empty);
        yield return NewRequest(x => x.ImportId = string.Empty);
        yield return NewRequest(x => x.FilterVersionName = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return NewRequest(x => x.FilterVersionName = RandomStringUtil.GenerateSimpleSingleLineText(101));
        yield return NewRequest(x => x.FilterVersionDeadline = null);
    }

    private static UpdateVoterListImportWithNewElectoralRegisterFilterVersionRequest NewRequest(Action<UpdateVoterListImportWithNewElectoralRegisterFilterVersionRequest>? customizer = null)
    {
        var request = new UpdateVoterListImportWithNewElectoralRegisterFilterVersionRequest
        {
            ImportId = "b9007bc7-43cc-4048-a703-0bdea64d4574",
            FilterId = "2a1eaed8-46cc-4a8c-81b7-5ea6426d1428",
            FilterVersionName = "fooBar",
            FilterVersionDeadline = MockedClock.GetTimestampDate(),
        };
        customizer?.Invoke(request);
        return request;
    }
}
