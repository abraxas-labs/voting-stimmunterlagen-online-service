// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.DomainOfInfluenceVotingCardConfiguration;

public class SetDomainOfInfluenceVotingCardConfigurationRequestValidatorTest
    : ProtoValidatorBaseTest<SetDomainOfInfluenceVotingCardConfigurationRequest>
{
    protected override IEnumerable<SetDomainOfInfluenceVotingCardConfigurationRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.SampleCount = 2);
        yield return New(x => x.SampleCount = 50);
        yield return New(x => x.VotingCardGroups.Clear());
        yield return New(x => x.VotingCardSorts.Clear());
    }

    protected override IEnumerable<SetDomainOfInfluenceVotingCardConfigurationRequest> NotOkMessages()
    {
        yield return New(x => x.DomainOfInfluenceId = string.Empty);
        yield return New(x => x.SampleCount = 1);
        yield return New(x => x.SampleCount = 51);
        yield return New(x => x.VotingCardGroups.Add(VotingCardGroup.Unspecified));
        yield return New(x => x.VotingCardGroups.Add((VotingCardGroup)7));
        yield return New(x => x.VotingCardSorts.Add(VotingCardSort.Unspecified));
        yield return New(x => x.VotingCardSorts.Add((VotingCardSort)7));
    }

    private static SetDomainOfInfluenceVotingCardConfigurationRequest New(Action<SetDomainOfInfluenceVotingCardConfigurationRequest>? customizer = null)
    {
        var req = new SetDomainOfInfluenceVotingCardConfigurationRequest
        {
            DomainOfInfluenceId = "8dacbec2-574c-4182-bab0-00771983e2fb",
            SampleCount = 25,
            VotingCardGroups = { VotingCardGroup.Language },
            VotingCardSorts = { VotingCardSort.Street },
        };
        customizer?.Invoke(req);
        return req;
    }
}
