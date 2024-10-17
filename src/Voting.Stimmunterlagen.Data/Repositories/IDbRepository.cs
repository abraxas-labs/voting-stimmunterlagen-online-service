// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Voting.Lib.Database.Models;
using Voting.Lib.Database.Repositories;

namespace Voting.Stimmunterlagen.Data.Repositories;

public interface IDbRepository<TEntity> : IDbRepository<DataContext, TEntity>
    where TEntity : BaseEntity, new()
{
    Task SaveChanges();

    Task<bool> TryLockForUpdate(Guid id);
}
