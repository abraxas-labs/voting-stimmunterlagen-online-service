// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.ContestEVotingExportJob;

public class RetryContestEVotingExportJobRequestValidatorTest : ProtoValidatorBaseTest<RetryContestEVotingExportJobRequest>
{
    protected override IEnumerable<RetryContestEVotingExportJobRequest> OkMessages()
    {
        yield return new() { ContestId = "6f1311de-5205-4976-9712-516752a373dc" };
    }

    protected override IEnumerable<RetryContestEVotingExportJobRequest> NotOkMessages()
    {
        yield return new() { ContestId = string.Empty };
    }
}
