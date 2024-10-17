// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Attachment;

public class CreateAttachmentRequestValidatorTest : ProtoValidatorBaseTest<CreateAttachmentRequest>
{
    protected override IEnumerable<CreateAttachmentRequest> OkMessages()
    {
        yield return New(x => x.Name = RandomStringUtil.GenerateComplexSingleLineText(1));
        yield return New(x => x.Name = RandomStringUtil.GenerateComplexSingleLineText(300));
        yield return New(x => x.Color = string.Empty);
        yield return New(x => x.Color = RandomStringUtil.GenerateComplexSingleLineText(20));
        yield return New(x => x.Supplier = RandomStringUtil.GenerateComplexSingleLineText(1));
        yield return New(x => x.Supplier = RandomStringUtil.GenerateComplexSingleLineText(300));
        yield return New(x => x.PoliticalBusinessIds.Clear());
        yield return New(x => x.OrderedCount = 1);
        yield return New(x => x.OrderedCount = 1000000);
        yield return New(x => x.RequiredCount = 0);
        yield return New(x => x.RequiredCount = 1000000);
    }

    protected override IEnumerable<CreateAttachmentRequest> NotOkMessages()
    {
        yield return New(x => x.Name = string.Empty);
        yield return New(x => x.Name = RandomStringUtil.GenerateComplexSingleLineText(301));
        yield return New(x => x.Color = RandomStringUtil.GenerateComplexSingleLineText(21));
        yield return New(x => x.Supplier = string.Empty);
        yield return New(x => x.Supplier = RandomStringUtil.GenerateComplexSingleLineText(301));
        yield return New(x => x.DeliveryPlannedOn = null);
        yield return New(x => x.DomainOfInfluenceId = string.Empty);
        yield return New(x => x.PoliticalBusinessIds.Add("a"));
        yield return New(x => x.Format = AttachmentFormat.Unspecified);
        yield return New(x => x.Format = (AttachmentFormat)4);
        yield return New(x => x.OrderedCount = 0);
        yield return New(x => x.OrderedCount = 1000001);
        yield return New(x => x.RequiredCount = -1);
        yield return New(x => x.RequiredCount = 1000001);
        yield return New(x => x.Category = AttachmentCategory.Unspecified);
        yield return New(x => x.Category = (AttachmentCategory)100);
    }

    private static CreateAttachmentRequest New(Action<CreateAttachmentRequest>? customizer = null)
    {
        var req = new CreateAttachmentRequest
        {
            Name = "Name",
            DomainOfInfluenceId = "cccec354-2c06-4a29-9908-e65d64b7f649",
            Format = AttachmentFormat.A5,
            Color = "Black",
            Supplier = "Supplier",
            DeliveryPlannedOn = MockedClock.GetTimestampDate(1),
            PoliticalBusinessIds = { "dde6b134-55ab-4750-b574-93302bca134a" },
            OrderedCount = 5,
            RequiredCount = 5,
            Category = AttachmentCategory.BrochureMu,
        };
        customizer?.Invoke(req);
        return req;
    }
}
