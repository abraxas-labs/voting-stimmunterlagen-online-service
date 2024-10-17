// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.DomainOfInfluenceVotingCardBrick;

public class ListDomainOfInfluenceVotingCardBrickRequestValidatorTest
    : ProtoValidatorBaseTest<ListDomainOfInfluenceVotingCardBrickRequest>
{
    protected override IEnumerable<ListDomainOfInfluenceVotingCardBrickRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.TemplateId = 1);
        yield return New(x => x.TemplateId = 1000000);
    }

    protected override IEnumerable<ListDomainOfInfluenceVotingCardBrickRequest> NotOkMessages()
    {
        yield return New(x => x.TemplateId = 0);
        yield return New(x => x.TemplateId = 1000001);
    }

    private static ListDomainOfInfluenceVotingCardBrickRequest New(Action<ListDomainOfInfluenceVotingCardBrickRequest>? customizer = null)
    {
        var req = new ListDomainOfInfluenceVotingCardBrickRequest
        {
            TemplateId = 55,
        };
        customizer?.Invoke(req);
        return req;
    }
}
