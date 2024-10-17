// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.DomainOfInfluenceVotingCardLayout;

public class SetTemplateDataFieldRequestValidatorTest : ProtoValidatorBaseTest<SetTemplateDataFieldRequest>
{
    public static SetTemplateDataFieldRequest New(Action<SetTemplateDataFieldRequest>? customizer = null)
    {
        var req = new SetTemplateDataFieldRequest
        {
            ContainerKey = "abc",
            FieldKey = "ek",
            Value = "Field Value",
        };
        customizer?.Invoke(req);
        return req;
    }

    protected override IEnumerable<SetTemplateDataFieldRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.ContainerKey = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return New(x => x.ContainerKey = RandomStringUtil.GenerateSimpleSingleLineText(50));
        yield return New(x => x.FieldKey = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return New(x => x.FieldKey = RandomStringUtil.GenerateSimpleSingleLineText(50));
        yield return New(x => x.Value = string.Empty);
        yield return New(x => x.Value = RandomStringUtil.GenerateSimpleSingleLineText(50));
    }

    protected override IEnumerable<SetTemplateDataFieldRequest> NotOkMessages()
    {
        yield return New(x => x.ContainerKey = string.Empty);
        yield return New(x => x.ContainerKey = RandomStringUtil.GenerateSimpleSingleLineText(51));
        yield return New(x => x.FieldKey = string.Empty);
        yield return New(x => x.FieldKey = RandomStringUtil.GenerateSimpleSingleLineText(51));
        yield return New(x => x.Value = RandomStringUtil.GenerateSimpleSingleLineText(51));
    }
}
