// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Grpc.Core;
using Voting.Lib.Grpc.Interceptors;

namespace Voting.Stimmunterlagen.Interceptors;

public class AppModuleInterceptor : RequestInterceptor
{
    private const string AppHeader = "x-vo-stimmunterlagen-app-module";

    private readonly AppContext _appContext;

    public AppModuleInterceptor(AppContext appContext)
    {
        _appContext = appContext;
    }

    protected override Task InterceptRequest<TRequest>(TRequest request, ServerCallContext context)
    {
        var app = context.RequestHeaders.GetValue(AppHeader);
        _appContext.SetApp(app);
        return Task.CompletedTask;
    }
}
