// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.ContestVotingCardLayout;

public class SetContestVotingCardLayoutRequestValidatorTest : ProtoValidatorBaseTest<SetContestVotingCardLayoutRequest>
{
    protected override IEnumerable<SetContestVotingCardLayoutRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.TemplateId = 1);
        yield return New(x => x.TemplateId = 1000000);
    }

    protected override IEnumerable<SetContestVotingCardLayoutRequest> NotOkMessages()
    {
        yield return New(x => x.ContestId = string.Empty);
        yield return New(x => x.VotingCardType = VotingCardType.Unspecified);
        yield return New(x => x.VotingCardType = (VotingCardType)13);
        yield return New(x => x.TemplateId = 0);
        yield return New(x => x.TemplateId = 1000001);
        yield return New(x => x.DataConfiguration = null);
    }

    private static SetContestVotingCardLayoutRequest New(Action<SetContestVotingCardLayoutRequest>? customizer = null)
    {
        var req = new SetContestVotingCardLayoutRequest
        {
            ContestId = "efd38d93-4c62-47c7-a874-38071b6bf3d3",
            VotingCardType = VotingCardType.Swiss,
            AllowCustom = true,
            TemplateId = 1,
            DataConfiguration = new(),
        };
        customizer?.Invoke(req);
        return req;
    }
}
