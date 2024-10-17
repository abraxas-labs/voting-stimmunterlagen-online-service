// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.DomainOfInfluenceVotingCardLayout;

public class GetDomainOfInfluenceVotingCardLayoutTemplateDataRequestValidatorTest :
    ProtoValidatorBaseTest<GetDomainOfInfluenceVotingCardLayoutTemplateDataRequest>
{
    protected override IEnumerable<GetDomainOfInfluenceVotingCardLayoutTemplateDataRequest> OkMessages()
    {
        yield return new() { DomainOfInfluenceId = "4ed28793-5f8d-4677-8444-82a6dacf8567" };
    }

    protected override IEnumerable<GetDomainOfInfluenceVotingCardLayoutTemplateDataRequest> NotOkMessages()
    {
        yield return new();
        yield return new() { DomainOfInfluenceId = string.Empty };
    }
}
