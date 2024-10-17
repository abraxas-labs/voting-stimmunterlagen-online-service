// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Managers.VotingExport;

public class VotingExportRenderContext
{
    public VotingExportRenderContext(string key, ContestDomainOfInfluence domainOfInfluence)
    {
        Key = key;
        DomainOfInfluence = domainOfInfluence;
    }

    public string Key { get; }

    public ContestDomainOfInfluence DomainOfInfluence { get; }
}
