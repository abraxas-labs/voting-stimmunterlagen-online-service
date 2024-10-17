// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Attachment;

public class SetAttachmentStateRequestValidatorTest : ProtoValidatorBaseTest<SetAttachmentStateRequest>
{
    protected override IEnumerable<SetAttachmentStateRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.State = AttachmentState.Delivered);
        yield return New(x => x.State = AttachmentState.Rejected);
        yield return New(x => x.Comment = string.Empty);
    }

    protected override IEnumerable<SetAttachmentStateRequest> NotOkMessages()
    {
        yield return New(x => x.Id = string.Empty);
        yield return New(x => x.State = AttachmentState.Unspecified);
        yield return New(x => x.State = (AttachmentState)(-1));
    }

    private static SetAttachmentStateRequest New(Action<SetAttachmentStateRequest>? customizer = null)
    {
        var req = new SetAttachmentStateRequest
        {
            Id = "09dd3682-bfcb-4a6e-8e53-a63220298ecc",
            State = AttachmentState.Defined,
            Comment = "Comment",
        };
        customizer?.Invoke(req);
        return req;
    }
}
