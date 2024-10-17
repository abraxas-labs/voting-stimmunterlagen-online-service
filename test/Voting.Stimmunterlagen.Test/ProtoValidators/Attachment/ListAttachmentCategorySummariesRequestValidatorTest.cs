// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Attachment;

public class ListAttachmentCategorySummariesRequestValidatorTest : ProtoValidatorBaseTest<ListAttachmentCategorySummariesRequest>
{
    protected override IEnumerable<ListAttachmentCategorySummariesRequest> OkMessages()
    {
        yield return new() { DomainOfInfluenceId = "c8184eb4-b27b-46f8-b16d-673f7c31ab6c" };
        yield return new() { Filter = ListAttachmentCategorySummariesFilterRequestTest.New() };
    }

    protected override IEnumerable<ListAttachmentCategorySummariesRequest> NotOkMessages()
    {
        yield return new() { Filter = ListAttachmentCategorySummariesFilterRequestTest.New(x => x.ContestId = string.Empty) };
    }
}
