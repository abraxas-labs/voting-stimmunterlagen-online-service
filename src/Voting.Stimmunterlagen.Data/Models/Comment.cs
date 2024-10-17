// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public abstract class Comment : BaseEntity
{
    public User CreatedBy { get; set; } = new User();

    public DateTime CreatedAt { get; set; }

    public string Content { get; set; } = string.Empty;
}
