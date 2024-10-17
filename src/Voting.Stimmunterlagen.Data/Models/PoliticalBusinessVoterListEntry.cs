// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class PoliticalBusinessVoterListEntry : BaseEntity
{
    public PoliticalBusiness? PoliticalBusiness { get; set; }

    public Guid PoliticalBusinessId { get; set; }

    public VoterList? VoterList { get; set; }

    public Guid VoterListId { get; set; }
}
