// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Attachment;

public class ListDomainOfInfluenceAttachmentCountsRequestValidatorTest : ProtoValidatorBaseTest<ListDomainOfInfluenceAttachmentCountsRequest>
{
    protected override IEnumerable<ListDomainOfInfluenceAttachmentCountsRequest> OkMessages()
    {
        yield return new() { AttachmentId = "1d50e7c9-9c21-4ad6-991f-ba56b7492c23" };
    }

    protected override IEnumerable<ListDomainOfInfluenceAttachmentCountsRequest> NotOkMessages()
    {
        yield return new();
        yield return new() { AttachmentId = string.Empty };
    }
}
