// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Stimmunterlagen.Data.ValidationAttributes;

namespace Voting.Stimmunterlagen.Data.Models;

public class VoterSwissAbroadPerson
{
    [ValidateObject]
    public Country ResidenceCountry { get; set; } = new();

    public DateTime DateOfRegistration { get; set; }

    [ValidateObject]
    public VoterSwissAbroadExtension? Extension { get; set; }
}
