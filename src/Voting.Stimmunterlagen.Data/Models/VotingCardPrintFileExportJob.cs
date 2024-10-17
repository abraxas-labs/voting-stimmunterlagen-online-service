// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Data.Models;

public class VotingCardPrintFileExportJob : ExportJobBase
{
    public Guid VotingCardGeneratorJobId { get; set; }

    public VotingCardGeneratorJob? VotingCardGeneratorJob { get; set; }
}
