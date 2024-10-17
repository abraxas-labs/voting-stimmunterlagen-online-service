// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.ManualVotingCardGeneratorJob;

public class CreateCountryRequestValidatorTest : ProtoValidatorBaseTest<CreateCountryRequest>
{
    public static CreateCountryRequest New(Action<CreateCountryRequest>? customizer = null)
    {
        var req = new CreateCountryRequest
        {
            Iso2 = "CH",
            Name = "Schweiz",
        };
        customizer?.Invoke(req);
        return req;
    }

    protected override IEnumerable<CreateCountryRequest> OkMessages()
    {
        yield return New(x => x.Iso2 = RandomStringUtil.GenerateAlphabetic(2));
        yield return New(x => x.Name = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return New(x => x.Name = RandomStringUtil.GenerateSimpleSingleLineText(50));
    }

    protected override IEnumerable<CreateCountryRequest> NotOkMessages()
    {
        yield return New(x => x.Iso2 = RandomStringUtil.GenerateAlphabetic(1));
        yield return New(x => x.Iso2 = RandomStringUtil.GenerateAlphabetic(3));
        yield return New(x => x.Name = string.Empty);
        yield return New(x => x.Name = RandomStringUtil.GenerateSimpleSingleLineText(51));
    }
}
