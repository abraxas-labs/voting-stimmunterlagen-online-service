// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.DomainOfInfluenceVotingCardBrick;

public class GetDomainOfInfluenceVotingCardBrickContentEditorUrlRequestValidatorTest
    : ProtoValidatorBaseTest<GetDomainOfInfluenceVotingCardBrickContentEditorUrlRequest>
{
    protected override IEnumerable<GetDomainOfInfluenceVotingCardBrickContentEditorUrlRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.BrickId = 1);
        yield return New(x => x.BrickId = 1000000);
        yield return New(x => x.ContentId = 1);
        yield return New(x => x.ContentId = 1000000);
    }

    protected override IEnumerable<GetDomainOfInfluenceVotingCardBrickContentEditorUrlRequest> NotOkMessages()
    {
        yield return New(x => x.BrickId = 0);
        yield return New(x => x.BrickId = 1000001);
        yield return New(x => x.ContentId = 0);
        yield return New(x => x.ContentId = 1000001);
    }

    private static GetDomainOfInfluenceVotingCardBrickContentEditorUrlRequest New(Action<GetDomainOfInfluenceVotingCardBrickContentEditorUrlRequest>? customizer = null)
    {
        var req = new GetDomainOfInfluenceVotingCardBrickContentEditorUrlRequest
        {
            BrickId = 1,
            ContentId = 1,
        };
        customizer?.Invoke(req);
        return req;
    }
}
