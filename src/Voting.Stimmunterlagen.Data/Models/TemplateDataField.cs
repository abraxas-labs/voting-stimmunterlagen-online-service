// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class TemplateDataField : BaseEntity
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

    public TemplateDataContainer? Container { get; set; }

    public int ContainerId { get; set; }

    public ICollection<TemplateDataFieldValue>? Values { get; set; }
}
