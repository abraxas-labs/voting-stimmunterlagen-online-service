// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Lib.Database.Models;
using Voting.Stimmunterlagen.Data.ModelBuilders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.Extensions;

internal static class EntityTypeBuilderExtensions
{
    internal static void HasLanguageQueryFilter<T>(this EntityTypeBuilder<T> builder)
        where T : TranslationEntity
    {
        // Note that we need to use the same DbContext instance here that has been called with OnModelCreating, so that EF Core can recognize the Language field
        // During requests, the correct DbContext instance will be used (that is, the instance that is executing the query, not the one from the DbContextAccessor)
        builder
            .HasQueryFilter(t => DbContextAccessor.DbContext.Language == null || t.Language == DbContextAccessor.DbContext.Language);
    }

    internal static void HasGinTrigramIndex<T>(this EntityTypeBuilder<T> builder, Expression<Func<T, object?>> indexExpr)
        where T : BaseEntity
    {
        var indexType = indexExpr.Body.Type;
        var count = indexType.FullName!.Contains("AnonymousType")
            ? indexType.GetProperties().Length
            : 1;

        builder
            .HasIndex(indexExpr)
            .HasMethod("gin")
            .HasOperators(Enumerable.Repeat("gin_trgm_ops", count).ToArray());
    }
}
