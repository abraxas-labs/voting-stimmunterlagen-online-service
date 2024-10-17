// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Data.Models;

public class ContestEVotingExportJob : ExportJobBase, IHasContest
{
    public Guid ContestId { get; set; }

    public Contest? Contest { get; set; }

    public string FileHash { get; set; } = string.Empty;

    public override void PrepareToRun()
    {
        base.PrepareToRun();
        FileHash = string.Empty;
    }
}
