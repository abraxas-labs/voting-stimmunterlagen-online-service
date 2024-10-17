// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Core.Models;

public class VoterPageInfo
{
    public Guid Id { get; set; }

    public int PageFrom { get; set; }

    public int PageTo { get; set; }
}
