// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Attachment;

public class ListAttachmentCategorySummariesFilterRequestTest : ProtoValidatorBaseTest<ListAttachmentCategorySummariesFilterRequest>
{
    public static ListAttachmentCategorySummariesFilterRequest New(Action<ListAttachmentCategorySummariesFilterRequest>? customizer = null)
    {
        var req = new ListAttachmentCategorySummariesFilterRequest
        {
            ContestId = "cccec354-2c06-4a29-9908-e65d64b7f649",
            Query = "abc",
            State = AttachmentState.Defined,
        };
        customizer?.Invoke(req);
        return req;
    }

    protected override IEnumerable<ListAttachmentCategorySummariesFilterRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.Query = string.Empty);
        yield return New(x => x.Query = RandomStringUtil.GenerateSimpleSingleLineText(100));
        yield return New(x => x.State = AttachmentState.Unspecified);
    }

    protected override IEnumerable<ListAttachmentCategorySummariesFilterRequest> NotOkMessages()
    {
        yield return New(x => x.ContestId = string.Empty);
        yield return New(x => x.Query = RandomStringUtil.GenerateSimpleSingleLineText(101));
        yield return New(x => x.State = (AttachmentState)100);
    }
}
