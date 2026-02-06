// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Ech.Mapping;

namespace Voting.Stimmunterlagen.Core.Utils;

public static class DatamatrixMapping
{
    public const int PersonIdLength = 11;

    public static string MapContestOrderNumber(int orderNumber)
        => orderNumber.ToString().PadLeft(6, '0');

    public static string MapVoterShipmentNumber(int shipmentNumber)
        => shipmentNumber.ToString().PadLeft(9, '0');

    public static string MapPersonId(string personId)
        => personId.PadLeft(PersonIdLength, '0');

    public static string MapReligion(string? religion, bool isMinor, VoterType voterType)
    {
        if (string.IsNullOrWhiteSpace(religion))
        {
            return "X";
        }

        var isForeignerOrMinor = voterType == VoterType.Foreigner || isMinor;

        return (religion, isForeignerOrMinor) switch
        {
            ("111", true) => "E WR",
            ("121", true) => "K WR",
            ("111", false) => "E",
            ("121", false) => "K",
            _ => "X",
        };
    }

    public static bool IsMinor(string birthDate, DateTime contestDate)
    {
        var dateOfBirthDate = (!string.IsNullOrEmpty(birthDate) && birthDate != DatePartiallyKnownMapping.UnspecifiedDateString
            ? DatePartiallyKnownMapping.ToDateTime(birthDate)
            : (DateTime?)null) ?? throw new ValidationException($"Date format mismatch, cannot calculate minor flag with birthdate: {birthDate}");
        return dateOfBirthDate != default && dateOfBirthDate.AddYears(18) > contestDate;
    }

    public static string MapDomainOfInfluences(ICollection<VoterDomainOfInfluence>? domainOfInfluences, DomainOfInfluenceType doiType)
    {
        if (domainOfInfluences is null || domainOfInfluences.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(
            " ",
            domainOfInfluences
                .Where(d => d.DomainOfInfluenceType == doiType)
                .OrderBy(d => d.DomainOfInfluenceIdentification)
                .Select(d => d.DomainOfInfluenceIdentification)
                .Where(id => !string.IsNullOrWhiteSpace(id)));
    }
}
