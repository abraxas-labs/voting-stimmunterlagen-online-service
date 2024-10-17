// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.PrintJob;

public class SetPrintJobProcessEndedRequestValidatorTest : ProtoValidatorBaseTest<SetPrintJobProcessEndedRequest>
{
    protected override IEnumerable<SetPrintJobProcessEndedRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.VotingCardsShipmentWeight = 0);
        yield return New(x => x.VotingCardsShipmentWeight = 1000000);
        yield return New(x => x.VotingCardsPrintedAndPackedCount = 0);
        yield return New(x => x.VotingCardsPrintedAndPackedCount = 1000000);
    }

    protected override IEnumerable<SetPrintJobProcessEndedRequest> NotOkMessages()
    {
        yield return New(x => x.DomainOfInfluenceId = string.Empty);
        yield return New(x => x.VotingCardsShipmentWeight = -0.01);
        yield return New(x => x.VotingCardsShipmentWeight = 1000000.01);
        yield return New(x => x.VotingCardsPrintedAndPackedCount = -1);
        yield return New(x => x.VotingCardsPrintedAndPackedCount = 1000001);
    }

    private static SetPrintJobProcessEndedRequest New(Action<SetPrintJobProcessEndedRequest>? customizer = null)
    {
        var req = new SetPrintJobProcessEndedRequest
        {
            DomainOfInfluenceId = "cccec354-2c06-4a29-9908-e65d64b7f649",
            VotingCardsShipmentWeight = 15,
            VotingCardsPrintedAndPackedCount = 50,
        };
        customizer?.Invoke(req);
        return req;
    }
}
