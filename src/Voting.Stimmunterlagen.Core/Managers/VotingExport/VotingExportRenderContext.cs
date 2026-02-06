// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Managers.VotingExport;

public class VotingExportRenderContext
{
    private const string FileNameGroupSeparator = "_";

    public VotingExportRenderContext(string key, ContestDomainOfInfluence domainOfInfluence, VoterList? voterList)
    {
        Key = key;
        DomainOfInfluence = domainOfInfluence;
        VoterList = voterList;
    }

    public string Key { get; }

    public ContestDomainOfInfluence DomainOfInfluence { get; }

    public VoterList? VoterList { get; }

    public string[] BuildVotingJournalFileNameArgs()
    {
        var fileNameParams = new List<string> { DomainOfInfluence.Bfs };

        if (VoterList != null)
        {
            fileNameParams.Add(VoterList.Import!.Name);

            var vcTypeFileNamePart = VoterList.VotingCardType switch
            {
                VotingCardType.EVoting => "E_Voting",
                VotingCardType.SwissAbroad => "Swiss_Abroad",
                _ => string.Empty,
            };

            if (!string.IsNullOrEmpty(vcTypeFileNamePart))
            {
                fileNameParams.Add(vcTypeFileNamePart);
            }
        }

        return new[] { string.Join(FileNameGroupSeparator, fileNameParams) };
    }
}
