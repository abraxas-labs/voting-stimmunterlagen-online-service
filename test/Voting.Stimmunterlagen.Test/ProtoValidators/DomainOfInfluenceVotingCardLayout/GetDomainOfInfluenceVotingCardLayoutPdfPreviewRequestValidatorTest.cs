// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.DomainOfInfluenceVotingCardLayout;

public class GetDomainOfInfluenceVotingCardLayoutPdfPreviewRequestValidatorTest :
    ProtoValidatorBaseTest<GetDomainOfInfluenceVotingCardLayoutPdfPreviewRequest>
{
    protected override IEnumerable<GetDomainOfInfluenceVotingCardLayoutPdfPreviewRequest> OkMessages()
    {
        yield return New();
    }

    protected override IEnumerable<GetDomainOfInfluenceVotingCardLayoutPdfPreviewRequest> NotOkMessages()
    {
        yield return New(x => x.DomainOfInfluenceId = string.Empty);
        yield return New(x => x.VotingCardType = VotingCardType.Unspecified);
        yield return New(x => x.VotingCardType = (VotingCardType)(-1));
    }

    private static GetDomainOfInfluenceVotingCardLayoutPdfPreviewRequest New(Action<GetDomainOfInfluenceVotingCardLayoutPdfPreviewRequest>? customizer = null)
    {
        var req = new GetDomainOfInfluenceVotingCardLayoutPdfPreviewRequest
        {
            DomainOfInfluenceId = "552419c5-01d4-43f3-8c29-8872f2193eb1",
            VotingCardType = VotingCardType.EVoting,
        };
        customizer?.Invoke(req);
        return req;
    }
}
