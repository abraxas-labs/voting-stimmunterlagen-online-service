// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Stimmunterlagen.EVoting.Configuration;
using Voting.Stimmunterlagen.EVoting.Models;
using Voting.Stimmunterlagen.OfflineClient.Shared.ContestConfiguration;
using Contest = Voting.Stimmunterlagen.EVoting.Models.Contest;

namespace Voting.Stimmunterlagen.EVoting;

public class EVotingExportContext
{
    public required Contest Contest { get; init; }

    public required string Ech0045XmlFileName { get; init; }

    public required List<DomainOfInfluence> TestDomainOfInfluences { get; init; }

    public required DomainOfInfluence TestDomainOfInfluenceDefaults { get; init; }

    public required Dictionary<string, EVotingDomainOfInfluenceConfig> EVotingDomainOfInfluenceConfigByBfs { get; init; }

    public required Dictionary<string, List<Value>> BfsETextBlockValuesDict { get; init; }
}
