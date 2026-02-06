// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.Models;

public class VotingCardLayoutDataConfiguration
{
    public bool IncludePersonId { get; set; }

    public bool IncludeDateOfBirth { get; set; }

    public bool IncludeReligion { get; set; }

    public bool IncludeIsHouseholder { get; set; }

    public bool IncludeDomainOfInfluenceChurch { get; set; }

    public bool IncludeDomainOfInfluenceSchool { get; set; }
}
