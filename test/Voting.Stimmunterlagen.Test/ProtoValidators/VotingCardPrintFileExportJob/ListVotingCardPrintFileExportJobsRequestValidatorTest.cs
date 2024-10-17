// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.VotingCardPrintFileExportJob;

public class ListVotingCardPrintFileExportJobsRequestValidatorTest : ProtoValidatorBaseTest<ListVotingCardPrintFileExportJobsRequest>
{
    protected override IEnumerable<ListVotingCardPrintFileExportJobsRequest> OkMessages()
    {
        yield return new() { DomainOfInfluenceId = "5dfdd555-99d1-4ea5-a52e-481dc88e78b0" };
    }

    protected override IEnumerable<ListVotingCardPrintFileExportJobsRequest> NotOkMessages()
    {
        yield return new() { DomainOfInfluenceId = string.Empty };
    }
}
