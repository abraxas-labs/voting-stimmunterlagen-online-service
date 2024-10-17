// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;

namespace Voting.Stimmunterlagen.Core.Configuration;

public class InvoiceConfig
{
    public List<MaterialConfig> Materials { get; set; } = new();
}
