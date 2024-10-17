// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.DomainOfInfluenceVotingCardConfiguration;

public class GetDomainOfInfluenceVotingCardPdfPreviewRequestValidatorTest
    : ProtoValidatorBaseTest<GetDomainOfInfluenceVotingCardPdfPreviewRequest>
{
    protected override IEnumerable<GetDomainOfInfluenceVotingCardPdfPreviewRequest> OkMessages()
    {
        yield return New();
    }

    protected override IEnumerable<GetDomainOfInfluenceVotingCardPdfPreviewRequest> NotOkMessages()
    {
        yield return New(x => x.DomainOfInfluenceId = string.Empty);
        yield return New(x => x.VotingCardType = VotingCardType.Unspecified);
        yield return New(x => x.VotingCardType = (VotingCardType)9999);
    }

    private static GetDomainOfInfluenceVotingCardPdfPreviewRequest New(Action<GetDomainOfInfluenceVotingCardPdfPreviewRequest>? customizer = null)
    {
        var req = new GetDomainOfInfluenceVotingCardPdfPreviewRequest
        {
            VotingCardType = VotingCardType.Swiss,
            DomainOfInfluenceId = "8dacbec2-574c-4182-bab0-00771983e2fb",
        };
        customizer?.Invoke(req);
        return req;
    }
}
