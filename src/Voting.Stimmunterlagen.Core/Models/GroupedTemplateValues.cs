// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Models;

public class GroupedTemplateValues
{
    public GroupedTemplateValues(TemplateDataContainer container, IEnumerable<TemplateDataFieldValue> fieldValues)
    {
        Container = container;
        FieldValues = fieldValues;
    }

    public TemplateDataContainer Container { get; }

    public IEnumerable<TemplateDataFieldValue> FieldValues { get; }
}
