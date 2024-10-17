// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmunterlagen.Core.Mocks;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class TemplateMockData
{
    public static Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        return runScoped(sp =>
        {
            var db = sp.GetRequiredService<DataContext>();

            var dataContainers = DmDocServiceMock.MockedDataContainers.Where(d => !d.Global).Select(c => new TemplateDataContainer
            {
                Id = c.Id,
                Key = c.InternName,
                Name = c.DataContainerName,
                Fields = c.Fields.ConvertAll(f => new TemplateDataField
                {
                    Key = f.Key,
                    Name = f.Name,
                }),
            }).ToList();
            var dataContainersById = dataContainers.ToDictionary(x => x.Id);

            var all = DmDocServiceMock.Templates
                .Where(t => t.Id < 500)
                .Select(t => new Template
                {
                    Id = t.Id,
                    Name = t.Name,
                    InternName = t.InternName,
                    DataContainers = t.DataContainers
                        .Where(d => !d.Global)
                        .Select(c => dataContainersById[c.Id])
                        .ToList(),
                });

            db.Templates.AddRange(all);
            return db.SaveChangesAsync();
        });
    }
}
