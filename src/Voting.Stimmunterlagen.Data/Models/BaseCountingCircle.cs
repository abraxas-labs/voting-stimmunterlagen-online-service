// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public abstract class BaseCountingCircle : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Bfs { get; set; } = string.Empty;

    public bool EVoting { get; set; }
}
