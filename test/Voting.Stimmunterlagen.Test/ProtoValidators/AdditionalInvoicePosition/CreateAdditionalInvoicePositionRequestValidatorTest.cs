// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.AdditionalInvoicePosition;

public class CreateAdditionalInvoicePositionRequestValidatorTest : ProtoValidatorBaseTest<CreateAdditionalInvoicePositionRequest>
{
    protected override IEnumerable<CreateAdditionalInvoicePositionRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.MaterialNumber = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return New(x => x.MaterialNumber = RandomStringUtil.GenerateSimpleSingleLineText(16));
        yield return New(x => x.AmountCentime = 25);
        yield return New(x => x.AmountCentime = 100000000);
        yield return New(x => x.Comment = string.Empty);
        yield return New(x => x.Comment = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return New(x => x.Comment = RandomStringUtil.GenerateSimpleSingleLineText(500));
    }

    protected override IEnumerable<CreateAdditionalInvoicePositionRequest> NotOkMessages()
    {
        yield return New(x => x.MaterialNumber = string.Empty);
        yield return New(x => x.MaterialNumber = RandomStringUtil.GenerateSimpleSingleLineText(17));
        yield return New(x => x.AmountCentime = 0);
        yield return New(x => x.AmountCentime = 100000001);
        yield return New(x => x.DomainOfInfluenceId = string.Empty);
        yield return New(x => x.Comment = RandomStringUtil.GenerateSimpleSingleLineText(501));
    }

    private static CreateAdditionalInvoicePositionRequest New(Action<CreateAdditionalInvoicePositionRequest>? customizer = null)
    {
        var req = new CreateAdditionalInvoicePositionRequest
        {
            MaterialNumber = "Name",
            Comment = "Comment",
            DomainOfInfluenceId = "cccec354-2c06-4a29-9908-e65d64b7f649",
            AmountCentime = 2750,
        };
        customizer?.Invoke(req);
        return req;
    }
}
