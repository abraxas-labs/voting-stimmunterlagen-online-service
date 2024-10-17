// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Attachment;

public class SetDomainOfInfluenceAttachmentRequiredCountRequestValidatorTest : ProtoValidatorBaseTest<SetDomainOfInfluenceAttachmentRequiredCountRequest>
{
    protected override IEnumerable<SetDomainOfInfluenceAttachmentRequiredCountRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.RequiredCount = 0);
        yield return New(x => x.RequiredCount = 1000000);
    }

    protected override IEnumerable<SetDomainOfInfluenceAttachmentRequiredCountRequest> NotOkMessages()
    {
        yield return New(x => x.DomainOfInfluenceId = string.Empty);
        yield return New(x => x.Id = string.Empty);
        yield return New(x => x.RequiredCount = -1);
        yield return New(x => x.RequiredCount = 1000001);
    }

    private static SetDomainOfInfluenceAttachmentRequiredCountRequest New(Action<SetDomainOfInfluenceAttachmentRequiredCountRequest>? customizer = null)
    {
        var req = new SetDomainOfInfluenceAttachmentRequiredCountRequest
        {
            Id = "09dd3682-bfcb-4a6e-8e53-a63220298ecc",
            DomainOfInfluenceId = "ed18b0aa-3a95-4208-ac17-a6d6d54b30a4",
            RequiredCount = 1000,
        };
        customizer?.Invoke(req);
        return req;
    }
}
