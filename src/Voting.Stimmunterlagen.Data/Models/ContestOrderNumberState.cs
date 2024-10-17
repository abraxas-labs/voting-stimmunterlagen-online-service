// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class ContestOrderNumberState : BaseEntity
{
    public int LastSetOrderNumber { get; set; }
}
