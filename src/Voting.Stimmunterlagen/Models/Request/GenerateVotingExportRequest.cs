// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Models.Request;

public class GenerateVotingExportRequest
{
    public string Key { get; set; } = string.Empty;

    public Guid DomainOfInfluenceId { get; set; }

    public Guid? VoterListId { get; set; }
}
