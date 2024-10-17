// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Attachment;

public class UnassignPoliticalBusinessAttachmentRequestValidatorTest : ProtoValidatorBaseTest<UnassignPoliticalBusinessAttachmentRequest>
{
    protected override IEnumerable<UnassignPoliticalBusinessAttachmentRequest> OkMessages()
    {
        yield return new UnassignPoliticalBusinessAttachmentRequest
        {
            Id = "ecf5cfb6-13d7-4ea2-8366-a580288b3b96",
            PoliticalBusinessId = "ecf5cfb6-13d7-4ea2-8366-a580288b3b96",
        };
    }

    protected override IEnumerable<UnassignPoliticalBusinessAttachmentRequest> NotOkMessages()
    {
        yield return new UnassignPoliticalBusinessAttachmentRequest
        {
            Id = string.Empty,
            PoliticalBusinessId = "ecf5cfb6-13d7-4ea2-8366-a580288b3b96",
        };
        yield return new UnassignPoliticalBusinessAttachmentRequest
        {
            Id = "ecf5cfb6-13d7-4ea2-8366-a580288b3b96",
            PoliticalBusinessId = string.Empty,
        };
        yield return new UnassignPoliticalBusinessAttachmentRequest();
    }
}
