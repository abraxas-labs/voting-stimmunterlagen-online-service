// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Models;

public class ContestSummary
{
    public ContestSummary(Contest contest) => Contest = contest;

    public Contest Contest { get; }

    public PrintJobState PrintJobState { get; set; }
}
