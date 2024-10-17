// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Core.Models;

public class SimpleTemplateFieldValue
{
    public string ContainerKey { get; set; } = string.Empty;

    public string FieldKey { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
