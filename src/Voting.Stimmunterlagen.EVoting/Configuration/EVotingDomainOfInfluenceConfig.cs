// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Stimmunterlagen.OfflineClient.Shared.ContestConfiguration;

namespace Voting.Stimmunterlagen.EVoting.Configuration;

public class EVotingDomainOfInfluenceConfig
{
    public string? ETextBlockColumnQuantity { get; set; }

    public List<Value>? ETextBlockValues { get; set; }

    public bool? Stistat { get; set; }
}
