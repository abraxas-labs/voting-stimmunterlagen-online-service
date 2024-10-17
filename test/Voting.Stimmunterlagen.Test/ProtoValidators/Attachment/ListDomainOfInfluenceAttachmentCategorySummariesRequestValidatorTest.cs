// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Attachment;

public class ListDomainOfInfluenceAttachmentCategorySummariesRequestValidatorTest : ProtoValidatorBaseTest<ListDomainOfInfluenceAttachmentCategorySummariesRequest>
{
    protected override IEnumerable<ListDomainOfInfluenceAttachmentCategorySummariesRequest> OkMessages()
    {
        yield return New();
    }

    protected override IEnumerable<ListDomainOfInfluenceAttachmentCategorySummariesRequest> NotOkMessages()
    {
        yield return New(x => x.DomainOfInfluenceId = string.Empty);
    }

    private static ListDomainOfInfluenceAttachmentCategorySummariesRequest New(Action<ListDomainOfInfluenceAttachmentCategorySummariesRequest>? customizer = null)
    {
        var req = new ListDomainOfInfluenceAttachmentCategorySummariesRequest
        {
            DomainOfInfluenceId = "09dd3682-bfcb-4a6e-8e53-a63220298ecc",
        };
        customizer?.Invoke(req);
        return req;
    }
}
