// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Voting.Stimmunterlagen.Core.Models.TemplateData;

public class TemplateBag
{
    internal const string ContestContainerName = "contest";
    internal const string VoterContainerName = "voters";
    internal const string DomainOfInfluenceContainerName = "domain_of_influence";
    internal const string JobContainerName = "data_stimmunterlagen";

    internal TemplateBag(
        JobData? jobData,
        Contest? contest,
        DomainOfInfluence? domainOfInfluence,
        IReadOnlyCollection<Voter>? voters,
        IDictionary<string, object> fields)
    {
        Contest = contest;
        DomainOfInfluence = domainOfInfluence;
        Voters = voters;
        Fields = fields;
        JobData = jobData ?? new();
    }

    [JsonExtensionData]
    public IDictionary<string, object> Fields { get; }

    [JsonPropertyName(ContestContainerName)]
    public Contest? Contest { get; }

    [JsonPropertyName(VoterContainerName)]
    public IReadOnlyCollection<Voter>? Voters { get; }

    [JsonPropertyName(DomainOfInfluenceContainerName)]
    public DomainOfInfluence? DomainOfInfluence { get; }

    [JsonPropertyName(JobContainerName)]
    public JobData JobData { get; }
}
