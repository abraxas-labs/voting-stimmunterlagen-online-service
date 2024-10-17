// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public class TestReadOnlyDbStartup : TestStartup
{
    public TestReadOnlyDbStartup(IConfiguration configuration)
        : base(configuration)
    {
    }

    protected override void ConfigureDatabase(DbContextOptionsBuilder db)
    {
        var connStrBuilder = new NpgsqlConnectionStringBuilder(AppConfig.Database.ConnectionString);
        connStrBuilder.Database += "-ro";
        db.UseNpgsql(connStrBuilder.ToString(), ConfigureNpgsql);
    }
}
