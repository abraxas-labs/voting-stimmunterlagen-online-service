// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.ManualVotingCardGeneratorJob;

public class CreateManualVotingCardGeneratorJobRequestValidatorTest : ProtoValidatorBaseTest<CreateManualVotingCardGeneratorJobRequest>
{
    protected override IEnumerable<CreateManualVotingCardGeneratorJobRequest> OkMessages()
    {
        yield return New();
    }

    protected override IEnumerable<CreateManualVotingCardGeneratorJobRequest> NotOkMessages()
    {
        yield return New(x => x.DomainOfInfluenceId = string.Empty);
        yield return New(x => x.Voter = null);
    }

    private static CreateManualVotingCardGeneratorJobRequest New(Action<CreateManualVotingCardGeneratorJobRequest>? customizer = null)
    {
        var req = new CreateManualVotingCardGeneratorJobRequest
        {
            DomainOfInfluenceId = "792c5a01-62c8-403e-97d2-b04a070e6f3d",
            Voter = CreateManualVotingCardVoterRequestValidatorTest.New(),
        };
        customizer?.Invoke(req);
        return req;
    }
}
