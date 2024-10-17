// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.ElectoralRegister;

public class GetElectoralRegisterFilterMetadataRequestValidatorTest
    : ProtoValidatorBaseTest<GetElectoralRegisterFilterMetadataRequest>
{
    protected override IEnumerable<GetElectoralRegisterFilterMetadataRequest> OkMessages()
    {
        yield return New();
    }

    protected override IEnumerable<GetElectoralRegisterFilterMetadataRequest> NotOkMessages()
    {
        yield return New(x => x.FilterId = string.Empty);
        yield return New(x => x.Deadline = null);
    }

    private static GetElectoralRegisterFilterMetadataRequest New(Action<GetElectoralRegisterFilterMetadataRequest>? customizer = null)
    {
        var req = new GetElectoralRegisterFilterMetadataRequest
        {
            FilterId = "d9bcc80a-4364-4dbc-ac30-ab7495f88efc",
            Deadline = Timestamp.FromDateTime(new DateTime(2020, 10, 7, 0, 0, 0, DateTimeKind.Utc)),
        };
        customizer?.Invoke(req);
        return req;
    }
}
