// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Database.Repositories;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.Repositories;

public class PoliticalBusinessAttachmentEntryRepo : DbRepository<DataContext, PoliticalBusinessAttachmentEntry>
{
    public PoliticalBusinessAttachmentEntryRepo(DataContext context)
        : base(context)
    {
    }

    public async Task Replace(Guid attachmentId, ICollection<PoliticalBusinessAttachmentEntry> entries)
    {
        var existingEntries = await Set.Where(x => x.AttachmentId == attachmentId).ToListAsync();

        Set.RemoveRange(existingEntries);
        Set.AddRange(entries);
        await Context.SaveChangesAsync();
    }
}
