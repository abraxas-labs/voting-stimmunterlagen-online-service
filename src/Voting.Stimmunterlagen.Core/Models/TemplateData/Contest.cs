// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Core.Models.TemplateData;

public class Contest
{
    public DateTime Date { get; set; }

    public string OrderNumber { get; set; } = string.Empty;
}
