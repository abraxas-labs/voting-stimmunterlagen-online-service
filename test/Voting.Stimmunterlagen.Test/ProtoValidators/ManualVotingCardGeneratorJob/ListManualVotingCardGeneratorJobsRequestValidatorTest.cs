// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.ManualVotingCardGeneratorJob;

public class ListManualVotingCardGeneratorJobsRequestValidatorTest : ProtoValidatorBaseTest<ListManualVotingCardGeneratorJobsRequest>
{
    protected override IEnumerable<ListManualVotingCardGeneratorJobsRequest> OkMessages()
    {
        yield return new() { DomainOfInfluenceId = "48a96a95-fba8-47e3-a780-2de564137642" };
    }

    protected override IEnumerable<ListManualVotingCardGeneratorJobsRequest> NotOkMessages()
    {
        yield return new() { DomainOfInfluenceId = string.Empty };
    }
}
