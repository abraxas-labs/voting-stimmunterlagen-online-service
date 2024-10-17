// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Database.Repositories;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.Repositories;

public class TemplateDataContainerRepo : DbRepository<DataContext, int, TemplateDataContainer>
{
    public TemplateDataContainerRepo(DataContext context)
        : base(context)
    {
    }

    public async Task<List<TemplateDataContainer>> Merge(List<TemplateDataContainer> containers)
    {
        var ids = containers.Select(x => x.Id).ToHashSet();
        var existingContainers = await Query()
            .AsTracking()
            .Include(x => x.Fields)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();
        var existingById = existingContainers.ToDictionary(x => x.Id);

        var merged = new List<TemplateDataContainer>();

        foreach (var container in containers)
        {
            if (existingById.TryGetValue(container.Id, out var existingContainer))
            {
                MergeFields(existingContainer, container);
                merged.Add(existingContainer);
                continue;
            }

            Context.TemplateDataContainers.Add(container);
            merged.Add(container);
        }

        await Context.SaveChangesAsync();
        return merged;
    }

    private void MergeFields(TemplateDataContainer existing, TemplateDataContainer updated)
    {
        var updatedFields = updated.Fields!.ToDictionary(x => x.Key);
        var existingFields = existing.Fields!.ToList();
        foreach (var existingField in existingFields)
        {
            if (!updatedFields.TryGetValue(existingField.Key, out var updatedField))
            {
                existingField.Active = false;
                continue;
            }

            existingField.Active = true;
            existingField.Name = updatedField.Name;
            updatedFields.Remove(updatedField.Key);
        }

        existingFields.AddRange(updatedFields.Values);
    }
}
