// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Utils;

public static class DataConfigurationValidator
{
    public static void Validate(VotingCardLayoutDataConfiguration dataConfiguration, bool isStistatMunicipality, bool isPoliticalAssembly)
    {
        if (isStistatMunicipality && !isPoliticalAssembly)
        {
            dataConfiguration.IncludePersonId = true;
            dataConfiguration.IncludeDateOfBirth = true;
        }

        if (dataConfiguration.IncludeReligion && !dataConfiguration.IncludeDateOfBirth)
        {
            throw new ValidationException("Enabling the \"religion\" option requires the \"date of birth\" option be enabled");
        }

        if (dataConfiguration.IncludeDomainOfInfluenceChurch && !dataConfiguration.IncludeReligion)
        {
            throw new ValidationException("Enabling the \"domain of influence church\" option requires the \"religion\" option be enabled");
        }
    }
}
