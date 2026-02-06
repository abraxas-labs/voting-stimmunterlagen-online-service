// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using Voting.Lib.Scheduler;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.EVoting.Configuration;
using DomainOfInfluence = Voting.Stimmunterlagen.EVoting.Models.DomainOfInfluence;

namespace Voting.Stimmunterlagen.Core.Configuration;

public class ContestEVotingExportConfig
{
    public string OutDirectoryPath { get; set; }
        = Path.Join(
            StimmunterlagenOutDirectoryUtils.OutDirectoryBasePath,
            "contest-evoting-exports");

    public int ParallelTasks { get; set; } = 3; // export could contain data up to 1gb.

    public JobConfig Scheduler { get; set; } = new() { Interval = TimeSpan.FromHours(12) };

    public uint MaxLogoHeight { get; set; } = 250; // approx 50-150kb.

    /// <summary>
    /// Gets or sets the "test domain of influence defaults" (Testurnen), which are exported for eVoting.
    /// </summary>
    public DomainOfInfluence TestDomainOfInfluenceDefaults { get; set; } = new();

    /// <summary>
    /// Gets or sets the "test domain of influences" (Testurnen), which are exported for eVoting.
    /// </summary>
    public Dictionary<DomainOfInfluenceCanton, List<DomainOfInfluence>> TestDomainOfInfluences { get; set; } = new();

    /// <summary>
    /// Gets or sets the domain of influence configs that are applied are applied for the domain of influences, which are exported for eVoting.
    /// </summary>
    public Dictionary<string, EVotingDomainOfInfluenceConfig> EVotingDomainOfInfluences { get; set; } = new();
}
