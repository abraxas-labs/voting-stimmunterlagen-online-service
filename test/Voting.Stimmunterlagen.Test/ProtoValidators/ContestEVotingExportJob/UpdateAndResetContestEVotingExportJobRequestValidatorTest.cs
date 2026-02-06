// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.ContestEVotingExportJob;

public class UpdateAndResetContestEVotingExportJobRequestValidatorTest : ProtoValidatorBaseTest<UpdateAndResetContestEVotingExportJobRequest>
{
    protected override IEnumerable<UpdateAndResetContestEVotingExportJobRequest> OkMessages()
    {
        yield return New();
    }

    protected override IEnumerable<UpdateAndResetContestEVotingExportJobRequest> NotOkMessages()
    {
        yield return New(x => x.ContestId = string.Empty);
        yield return New(x => x.Ech0045Version = Proto.V1.Models.Ech0045Version.Unspecified);
        yield return New(x => x.Ech0045Version = (Proto.V1.Models.Ech0045Version)5);
    }

    private static UpdateAndResetContestEVotingExportJobRequest New(Action<UpdateAndResetContestEVotingExportJobRequest>? customizer = null)
    {
        var req = new UpdateAndResetContestEVotingExportJobRequest
        {
            ContestId = "6f1311de-5205-4976-9712-516752a373dc",
            Ech0045Version = Proto.V1.Models.Ech0045Version._4,
        };

        customizer?.Invoke(req);
        return req;
    }
}
