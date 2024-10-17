// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.Step;

public class ApproveStepRequestValidatorTest : ProtoValidatorBaseTest<ApproveStepRequest>
{
    protected override IEnumerable<ApproveStepRequest> OkMessages()
    {
        yield return new ApproveStepRequest
        {
            Step = ProtoModels.Step.PoliticalBusinessesApproval,
            DomainOfInfluenceId = "4fd8767c-0039-4a50-9172-f9859fa248cc",
        };
    }

    protected override IEnumerable<ApproveStepRequest> NotOkMessages()
    {
        yield return new ApproveStepRequest { Step = ProtoModels.Step.PoliticalBusinessesApproval, DomainOfInfluenceId = string.Empty };
        yield return new ApproveStepRequest { Step = (ProtoModels.Step)(-1), DomainOfInfluenceId = "dce28930-c23d-4d5b-98f8-c73b88c08304" };
        yield return new ApproveStepRequest { Step = ProtoModels.Step.Unspecified, DomainOfInfluenceId = "dce28930-c23d-4d5b-98f8-c73b88c08304" };
    }
}
