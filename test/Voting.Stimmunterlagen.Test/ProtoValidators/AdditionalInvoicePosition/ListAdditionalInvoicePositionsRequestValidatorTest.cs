// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.AdditionalInvoicePosition;

public class ListAdditionalInvoicePositionsRequestValidatorTest : ProtoValidatorBaseTest<ListAdditionalInvoicePositionsRequest>
{
    protected override IEnumerable<ListAdditionalInvoicePositionsRequest> OkMessages()
    {
        yield return new() { ContestId = "cccec354-2c06-4a29-9908-e65d64b7f649" };
    }

    protected override IEnumerable<ListAdditionalInvoicePositionsRequest> NotOkMessages()
    {
        yield return new() { ContestId = string.Empty };
    }
}
