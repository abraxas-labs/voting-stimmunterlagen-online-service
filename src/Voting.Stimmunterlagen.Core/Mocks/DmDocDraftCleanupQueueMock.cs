// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.DmDoc;
using Voting.Lib.DmDoc.Models;

namespace Voting.Stimmunterlagen.Core.Mocks;

public class DmDocDraftCleanupQueueMock : IDmDocDraftCleanupQueue
{
    public void Enqueue(int draftId, DraftCleanupMode draftCleanupMode)
    {
        // Nothing to be enqueued for mock service.
    }

    public bool TryDequeue(out DraftCleanupItem? draftCleanupItem)
    {
        draftCleanupItem = null;
        return true;
    }
}
