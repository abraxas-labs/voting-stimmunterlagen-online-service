// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Attachment;

public class SetAttachmentStationRequestValidatorTest : ProtoValidatorBaseTest<SetAttachmentStationRequest>
{
    protected override IEnumerable<SetAttachmentStationRequest> OkMessages()
    {
        yield return new SetAttachmentStationRequest { Id = "09dd3682-bfcb-4a6e-8e53-a63220298ecc", Station = 1 };
    }

    protected override IEnumerable<SetAttachmentStationRequest> NotOkMessages()
    {
        yield return new SetAttachmentStationRequest { Id = string.Empty, Station = 1 };
        yield return new SetAttachmentStationRequest { Id = "09dd3682-bfcb-4a6e-8e53-a63220298ecc", Station = 0 };
        yield return new SetAttachmentStationRequest { Id = "09dd3682-bfcb-4a6e-8e53-a63220298ecc", Station = -1 };
        yield return new SetAttachmentStationRequest { Id = "09dd3682-bfcb-4a6e-8e53-a63220298ecc", Station = 100 };
    }
}
