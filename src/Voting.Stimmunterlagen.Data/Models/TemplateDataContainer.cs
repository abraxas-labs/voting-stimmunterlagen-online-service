// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class TemplateDataContainer : BaseEntity<int>
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ICollection<Template>? Templates { get; set; }

    public ICollection<TemplateDataField>? Fields { get; set; }
}
