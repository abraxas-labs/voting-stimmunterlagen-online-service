// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.IO;
using Voting.Lib.Scheduler;
using Voting.Stimmunterlagen.Core.Utils;

namespace Voting.Stimmunterlagen.Core.Configuration;

public class VotingCardGeneratorConfig
{
    public string MessageId { get; set; } = string.Empty;

    public string OutDirectoryPath { get; set; }
        = Path.Join(
            StimmunterlagenOutDirectoryUtils.OutDirectoryBasePath,
            "generated-voting-cards");

    public string FileNameGroupSeparator { get; set; } = "_";

    public int ParallelTasks { get; set; } = 5; // value evaluated by trial and error (how much can dmdoc handle?)

    public JobConfig Scheduler { get; set; } = new() { Interval = TimeSpan.FromHours(1) };
}
