// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.DomainOfInfluenceVotingCardLayout;

public class SetDomainOfInfluenceVotingCardLayoutRequestValidatorTest :
    ProtoValidatorBaseTest<SetDomainOfInfluenceVotingCardLayoutRequest>
{
    protected override IEnumerable<SetDomainOfInfluenceVotingCardLayoutRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.TemplateId = null);
        yield return New(x => x.TemplateId = 1);
        yield return New(x => x.TemplateId = 1000000);
    }

    protected override IEnumerable<SetDomainOfInfluenceVotingCardLayoutRequest> NotOkMessages()
    {
        yield return New(x => x.DomainOfInfluenceId = string.Empty);
        yield return New(x => x.TemplateId = 0);
        yield return New(x => x.TemplateId = 1000001);
        yield return New(x => x.VotingCardType = VotingCardType.Unspecified);
        yield return New(x => x.VotingCardType = (VotingCardType)6);
    }

    private static SetDomainOfInfluenceVotingCardLayoutRequest New(Action<SetDomainOfInfluenceVotingCardLayoutRequest>? customizer = null)
    {
        var req = new SetDomainOfInfluenceVotingCardLayoutRequest
        {
            TemplateId = 2,
            VotingCardType = VotingCardType.Swiss,
            DomainOfInfluenceId = "4590b3b6-1c0f-45a7-bbda-7d3cf7e7b3c2",
        };
        customizer?.Invoke(req);
        return req;
    }
}
