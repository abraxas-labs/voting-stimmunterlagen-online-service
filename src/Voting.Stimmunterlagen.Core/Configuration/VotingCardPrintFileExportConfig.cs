// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Scheduler;

namespace Voting.Stimmunterlagen.Core.Configuration;

public class VotingCardPrintFileExportConfig
{
    public int ParallelTasks { get; set; } = 5; // export could contain data up to 1gb.

    public JobConfig Scheduler { get; set; } = new() { Interval = TimeSpan.FromHours(12) };
}
