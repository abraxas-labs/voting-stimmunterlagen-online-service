// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Validation;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Test.ProtoValidators.ManualVotingCardGeneratorJob;

public class CreateManualVotingCardVoterRequestValidatorTest : ProtoValidatorBaseTest<CreateManualVotingCardVoterRequest>
{
    public static CreateManualVotingCardVoterRequest New(Action<CreateManualVotingCardVoterRequest>? customizer = null)
    {
        var req = new CreateManualVotingCardVoterRequest
        {
            VotingCardType = VotingCardType.Swiss,
            Salutation = Salutation.Mister,
            Title = "Dr.",
            FirstName = "Max",
            LastName = "Muster",
            AddressLine1 = "Zeile 1",
            AddressLine2 = "Hinter der Linde",
            Street = "Musterstrasse",
            HouseNumber = "22",
            DwellingNumber = "2. Etage",
            Locality = "3XX",
            Town = "Entenhausen",
            SwissZipCode = 2000,
            Country = CreateCountryRequestValidatorTest.New(),
            LanguageOfCorrespondence = "CH",
            PersonId = "12345",
        };
        customizer?.Invoke(req);
        return req;
    }

    protected override IEnumerable<CreateManualVotingCardVoterRequest> OkMessages()
    {
        yield return New();
        yield return New(x => x.Title = string.Empty);
        yield return New(x => x.Title = RandomStringUtil.GenerateSimpleSingleLineText(50));
        yield return New(x => x.Salutation = Salutation.Unspecified);
        yield return New(x => x.FirstName = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return New(x => x.FirstName = RandomStringUtil.GenerateSimpleSingleLineText(100));
        yield return New(x => x.LastName = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return New(x => x.LastName = RandomStringUtil.GenerateSimpleSingleLineText(100));
        yield return New(x => x.AddressLine1 = string.Empty);
        yield return New(x => x.AddressLine1 = RandomStringUtil.GenerateSimpleSingleLineText(150));
        yield return New(x => x.AddressLine2 = string.Empty);
        yield return New(x => x.AddressLine2 = RandomStringUtil.GenerateSimpleSingleLineText(150));
        yield return New(x => x.Street = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return New(x => x.Street = RandomStringUtil.GenerateSimpleSingleLineText(150));
        yield return New(x => x.HouseNumber = string.Empty);
        yield return New(x => x.HouseNumber = RandomStringUtil.GenerateSimpleSingleLineText(30));
        yield return New(x => x.Locality = string.Empty);
        yield return New(x => x.Locality = RandomStringUtil.GenerateSimpleSingleLineText(40));
        yield return New(x => x.Town = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return New(x => x.Town = RandomStringUtil.GenerateSimpleSingleLineText(40));
        yield return New(x => x.SwissZipCode = 1000);
        yield return New(x => x.SwissZipCode = 9999);
        yield return New(x => x.ForeignZipCode = string.Empty);
        yield return New(x => x.ForeignZipCode = RandomStringUtil.GenerateSimpleSingleLineText(40));
        yield return New(x => x.LanguageOfCorrespondence = RandomStringUtil.GenerateAlphabetic(2));
        yield return New(x => x.PersonId = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return New(x => x.PersonId = RandomStringUtil.GenerateSimpleSingleLineText(36));
        yield return New(x => x.DateOfBirth = null);
    }

    protected override IEnumerable<CreateManualVotingCardVoterRequest> NotOkMessages()
    {
        yield return New(x => x.Title = RandomStringUtil.GenerateSimpleSingleLineText(51));
        yield return New(x => x.Salutation = (Salutation)10);
        yield return New(x => x.FirstName = string.Empty);
        yield return New(x => x.FirstName = RandomStringUtil.GenerateSimpleSingleLineText(101));
        yield return New(x => x.LastName = string.Empty);
        yield return New(x => x.LastName = RandomStringUtil.GenerateSimpleSingleLineText(101));
        yield return New(x => x.AddressLine1 = RandomStringUtil.GenerateSimpleSingleLineText(151));
        yield return New(x => x.AddressLine2 = RandomStringUtil.GenerateSimpleSingleLineText(151));
        yield return New(x => x.Street = string.Empty);
        yield return New(x => x.Street = RandomStringUtil.GenerateSimpleSingleLineText(151));
        yield return New(x => x.HouseNumber = RandomStringUtil.GenerateSimpleSingleLineText(31));
        yield return New(x => x.Locality = RandomStringUtil.GenerateSimpleSingleLineText(41));
        yield return New(x => x.Town = string.Empty);
        yield return New(x => x.Town = RandomStringUtil.GenerateSimpleSingleLineText(41));
        yield return New(x => x.SwissZipCode = 999);
        yield return New(x => x.SwissZipCode = 10000);
        yield return New(x => x.SwissZipCode = -1000);
        yield return New(x => x.ForeignZipCode = RandomStringUtil.GenerateSimpleSingleLineText(41));
        yield return New(x => x.LanguageOfCorrespondence = RandomStringUtil.GenerateAlphabetic(1));
        yield return New(x => x.LanguageOfCorrespondence = RandomStringUtil.GenerateAlphabetic(3));
        yield return New(x => x.PersonId = string.Empty);
        yield return New(x => x.PersonId = RandomStringUtil.GenerateSimpleSingleLineText(37));
    }
}
