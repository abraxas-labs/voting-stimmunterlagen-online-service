// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;

namespace Voting.Stimmunterlagen.Core.Models;

public class VoterPagesInfo
{
    public List<VoterPageInfo> Pages { get; set; } = new();
}
