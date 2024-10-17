// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class AuditedEntity : BaseEntity
{
    public DateTime Created { get; set; }

    public User CreatedBy { get; set; } = new();

    public DateTime Modified { get; set; }

    public User ModifiedBy { get; set; } = new();
}
