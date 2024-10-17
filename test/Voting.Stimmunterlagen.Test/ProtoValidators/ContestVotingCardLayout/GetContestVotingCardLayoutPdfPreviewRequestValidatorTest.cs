// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.ContestVotingCardLayout;

public class GetContestVotingCardLayoutPdfPreviewRequestValidatorTest :
    ProtoValidatorBaseTest<GetContestVotingCardLayoutPdfPreviewRequest>
{
    protected override IEnumerable<GetContestVotingCardLayoutPdfPreviewRequest> OkMessages()
    {
        yield return New();
    }

    protected override IEnumerable<GetContestVotingCardLayoutPdfPreviewRequest> NotOkMessages()
    {
        yield return New(x => x.ContestId = string.Empty);
        yield return New(x => x.VotingCardType = VotingCardType.Unspecified);
        yield return New(x => x.VotingCardType = (VotingCardType)5);
    }

    private static GetContestVotingCardLayoutPdfPreviewRequest New(Action<GetContestVotingCardLayoutPdfPreviewRequest>? customizer = null)
    {
        var req = new GetContestVotingCardLayoutPdfPreviewRequest
        {
            ContestId = "efd38d93-4c62-47c7-a874-38071b6bf3d3",
            VotingCardType = VotingCardType.Swiss,
        };
        customizer?.Invoke(req);
        return req;
    }
}
