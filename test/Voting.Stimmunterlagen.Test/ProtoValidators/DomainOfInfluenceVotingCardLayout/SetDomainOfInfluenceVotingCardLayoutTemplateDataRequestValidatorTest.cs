// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.DomainOfInfluenceVotingCardLayout;

public class SetDomainOfInfluenceVotingCardLayoutTemplateDataRequestValidatorTest
: ProtoValidatorBaseTest<SetDomainOfInfluenceVotingCardLayoutTemplateDataRequest>
{
    protected override IEnumerable<SetDomainOfInfluenceVotingCardLayoutTemplateDataRequest> OkMessages()
    {
        yield return New();
    }

    protected override IEnumerable<SetDomainOfInfluenceVotingCardLayoutTemplateDataRequest> NotOkMessages()
    {
        yield return New(x => x.DomainOfInfluenceId = string.Empty);
        yield return New(x => x.Fields[0].FieldKey = string.Empty);
    }

    private static SetDomainOfInfluenceVotingCardLayoutTemplateDataRequest New(Action<SetDomainOfInfluenceVotingCardLayoutTemplateDataRequest>? customizer = null)
    {
        var req = new SetDomainOfInfluenceVotingCardLayoutTemplateDataRequest
        {
            DomainOfInfluenceId = "552419c5-01d4-43f3-8c29-8872f2193eb1",
            Fields = { SetTemplateDataFieldRequestValidatorTest.New() },
        };
        customizer?.Invoke(req);
        return req;
    }
}
