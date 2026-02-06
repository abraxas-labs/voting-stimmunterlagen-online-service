// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Data.Models;

public class ContestEVotingExportJob : ExportJobBase, IHasContest
{
    public Guid ContestId { get; set; }

    public Contest? Contest { get; set; }

    public string FileHash { get; set; } = string.Empty;

    public Ech0045Version Ech0045Version { get; set; }

    public override void Reset()
    {
        base.Reset();
        FileHash = string.Empty;
    }
}
