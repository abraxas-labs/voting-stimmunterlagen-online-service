// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Voter = Voting.Stimmunterlagen.Data.Models.Voter;

namespace Voting.Stimmunterlagen.MappingProfiles.Resolver;

public sealed class DomainOfInfluencesResolver : IValueResolver<CreateManualVotingCardVoterRequest, Voter, ICollection<VoterDomainOfInfluence>?>
{
    public ICollection<VoterDomainOfInfluence> Resolve(CreateManualVotingCardVoterRequest source, Voter destination, ICollection<VoterDomainOfInfluence>? destMember, ResolutionContext context)
    {
        var result = new List<VoterDomainOfInfluence>();

        result.AddRange(ParseCodes(
            source.DomainOfInfluenceIdentificationChurch,
            DomainOfInfluenceType.Ki));

        result.AddRange(ParseCodes(
            source.DomainOfInfluenceIdentificationSchool,
            DomainOfInfluenceType.Sc));
        return result;
    }

    private static IEnumerable<VoterDomainOfInfluence> ParseCodes(
            string? codes,
            DomainOfInfluenceType type)
    {
        if (string.IsNullOrWhiteSpace(codes))
        {
            yield break;
        }

        // Split on any whitespace (space, tabs, multiple spaces)
        foreach (var code in Regex.Split(codes.Trim(), @"\s+")
                                  .Where(c => !string.IsNullOrWhiteSpace(c)))
        {
            yield return new VoterDomainOfInfluence
            {
                DomainOfInfluenceType = type,
                DomainOfInfluenceIdentification = code,
                DomainOfInfluenceName = string.Empty,
            };
        }
    }
}
