// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public static class EventProcessingStateUtil
{
    public static async Task SetEventProcessingState(DataContext db, ulong? lastProcessedEventNumber, ulong latestEverProcessedEventNumber)
    {
        var state = await db.EventProcessingStates.FirstOrDefaultAsync();
        if (state == null)
        {
            state = new EventProcessingState();
            db.EventProcessingStates.Add(state);
        }

        state.LastProcessedEventCommitPosition = lastProcessedEventNumber;
        state.LatestEverProcessedEventNumber = latestEverProcessedEventNumber;
        state.LastProcessedEventPreparePosition = lastProcessedEventNumber;
        state.LastProcessedEventCommitPosition = lastProcessedEventNumber;
        await db.SaveChangesAsync();
    }
}
