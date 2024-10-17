// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Attachment;

public class AssignPoliticalBusinessAttachmentRequestValidatorTest : ProtoValidatorBaseTest<AssignPoliticalBusinessAttachmentRequest>
{
    protected override IEnumerable<AssignPoliticalBusinessAttachmentRequest> OkMessages()
    {
        yield return new AssignPoliticalBusinessAttachmentRequest
        {
            Id = "ecf5cfb6-13d7-4ea2-8366-a580288b3b96",
            PoliticalBusinessId = "ecf5cfb6-13d7-4ea2-8366-a580288b3b96",
        };
    }

    protected override IEnumerable<AssignPoliticalBusinessAttachmentRequest> NotOkMessages()
    {
        yield return new AssignPoliticalBusinessAttachmentRequest
        {
            Id = string.Empty,
            PoliticalBusinessId = "ecf5cfb6-13d7-4ea2-8366-a580288b3b96",
        };
        yield return new AssignPoliticalBusinessAttachmentRequest
        {
            Id = "ecf5cfb6-13d7-4ea2-8366-a580288b3b96",
            PoliticalBusinessId = string.Empty,
        };
        yield return new AssignPoliticalBusinessAttachmentRequest();
    }
}
