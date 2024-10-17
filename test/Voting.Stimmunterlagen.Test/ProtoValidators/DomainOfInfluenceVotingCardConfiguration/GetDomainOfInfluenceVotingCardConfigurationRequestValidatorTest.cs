// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.DomainOfInfluenceVotingCardConfiguration;

public class GetDomainOfInfluenceVotingCardConfigurationRequestValidatorTest
    : ProtoValidatorBaseTest<GetDomainOfInfluenceVotingCardConfigurationRequest>
{
    protected override IEnumerable<GetDomainOfInfluenceVotingCardConfigurationRequest> OkMessages()
    {
        yield return new() { DomainOfInfluenceId = "1843208f-d1ac-4f29-bea9-599f6915e33e" };
    }

    protected override IEnumerable<GetDomainOfInfluenceVotingCardConfigurationRequest> NotOkMessages()
    {
        yield return new();
    }
}
