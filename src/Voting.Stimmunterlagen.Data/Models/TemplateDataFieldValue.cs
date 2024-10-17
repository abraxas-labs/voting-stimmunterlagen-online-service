// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class TemplateDataFieldValue : BaseEntity
{
    public string Value { get; set; } = string.Empty;

    public TemplateDataField? Field { get; set; }

    public Guid? FieldId { get; set; }

    public DomainOfInfluenceVotingCardLayout? Layout { get; set; }

    public Guid? LayoutId { get; set; }
}
