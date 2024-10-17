// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.DomainOfInfluenceVotingCardBrick;

public class UpdateDomainOfInfluenceVotingCardBrickContentRequestValidatorTest
    : ProtoValidatorBaseTest<UpdateDomainOfInfluenceVotingCardBrickContentRequest>
{
    protected override IEnumerable<UpdateDomainOfInfluenceVotingCardBrickContentRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.ContentId = 1);
        yield return New(x => x.ContentId = 1000000);
        yield return New(x => x.Content = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return New(x => x.Content = RandomStringUtil.GenerateSimpleSingleLineText(5000));
    }

    protected override IEnumerable<UpdateDomainOfInfluenceVotingCardBrickContentRequest> NotOkMessages()
    {
        yield return New(x => x.ContentId = 0);
        yield return New(x => x.ContentId = 1000001);
        yield return New(x => x.Content = string.Empty);
        yield return New(x => x.Content = RandomStringUtil.GenerateSimpleSingleLineText(5001));
    }

    private static UpdateDomainOfInfluenceVotingCardBrickContentRequest New(Action<UpdateDomainOfInfluenceVotingCardBrickContentRequest>? customizer = null)
    {
        var req = new UpdateDomainOfInfluenceVotingCardBrickContentRequest
        {
            ContentId = 1,
            Content = "Hello",
        };
        customizer?.Invoke(req);
        return req;
    }
}
