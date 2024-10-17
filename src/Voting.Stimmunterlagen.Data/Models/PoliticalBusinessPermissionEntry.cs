// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class PoliticalBusinessPermissionEntry : BaseEntity
{
    public ContestDomainOfInfluence? DomainOfInfluence { get; set; }

    public Guid DomainOfInfluenceId { get; set; }

    public string SecureConnectId { get; set; } = string.Empty;

    public Guid PoliticalBusinessId { get; set; }

    public PoliticalBusiness? PoliticalBusiness { get; set; }

    public PoliticalBusinessRole Role { get; set; }
}
