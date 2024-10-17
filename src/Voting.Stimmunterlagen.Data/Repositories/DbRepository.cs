// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Voting.Lib.Database.Models;
using Voting.Lib.Database.Repositories;

namespace Voting.Stimmunterlagen.Data.Repositories;

public class DbRepository<TEntity> : DbRepository<DataContext, TEntity>, IDbRepository<TEntity>
    where TEntity : BaseEntity, new()
{
    private const string CouldNotAcquireLockSqlState = "55P03";
    private string? _lockForUpdateSqlTemplate;

    public DbRepository(DataContext context)
        : base(context)
    {
    }

    public Task SaveChanges() => Context.SaveChangesAsync();

    public async Task<bool> TryLockForUpdate(Guid id)
    {
        try
        {
            _lockForUpdateSqlTemplate ??= BuildLockSqlTemplate();
            await Context.Database.ExecuteSqlRawAsync(_lockForUpdateSqlTemplate, id);
            return true;
        }
        catch (PostgresException ex) when (ex.SqlState == CouldNotAcquireLockSqlState)
        {
            return false;
        }
    }

    private string BuildLockSqlTemplate()
        => $"SELECT 1 FROM {DelimitedSchemaAndTableName} WHERE {GetDelimitedColumnName(x => x.Id)} = {{0}} FOR UPDATE NOWAIT";
}
