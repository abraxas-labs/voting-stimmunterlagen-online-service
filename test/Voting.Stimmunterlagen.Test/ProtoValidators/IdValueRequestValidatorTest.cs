// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators;

public class IdValueRequestValidatorTest : ProtoValidatorBaseTest<IdValueRequest>
{
    protected override IEnumerable<IdValueRequest> NotOkMessages()
    {
        yield return new() { Id = string.Empty };
    }

    protected override IEnumerable<IdValueRequest> OkMessages()
    {
        yield return new() { Id = "885fab36-7ce8-4dd2-9ad2-2f8b41991f6f" };
    }
}
