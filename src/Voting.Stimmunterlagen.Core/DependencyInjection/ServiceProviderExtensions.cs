// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Iam.Models;
using Voting.Lib.Iam.Store;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceProviderExtensions
{
    public static IServiceScope CreateScopeCopyAuth(this IServiceProvider serviceProvider)
    {
        var outerAuth = serviceProvider.GetRequiredService<IAuth>();
        var scope = serviceProvider.CreateScope();
        try
        {
            var authStore = scope.ServiceProvider.GetRequiredService<IAuthStore>();
            authStore.SetValues(outerAuth.AccessToken, outerAuth.User, outerAuth.Tenant, outerAuth.Roles);
            return scope;
        }
        catch (Exception)
        {
            scope.Dispose();
            throw;
        }
    }

    public static IServiceScope CreateScopeWithTenant(this IServiceProvider serviceProvider, string tenantId)
    {
        var scope = serviceProvider.CreateScope();
        try
        {
            var authStore = scope.ServiceProvider.GetRequiredService<IAuthStore>();
            var tenant = new Tenant
            {
                Id = tenantId,
            };
            authStore.SetValues(string.Empty, new User(), tenant, null);
            return scope;
        }
        catch (Exception)
        {
            scope.Dispose();
            throw;
        }
    }
}
