// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class ContestOrderNumberStatesMockData
{
    private static readonly Guid _stateGuid = Guid.Parse("4b4b2e77-20d0-456f-aa47-bf9d50e62b41");

    private static ContestOrderNumberState _state = new()
    {
        Id = _stateGuid,
        LastSetOrderNumber = 950000,
    };

    public static Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        return runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();
            db.Add(_state);
            await db.SaveChangesAsync();
        });
    }
}
