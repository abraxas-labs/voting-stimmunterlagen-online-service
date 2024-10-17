// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Grpc.Core;
using Voting.Lib.Grpc.Interceptors;
using Voting.Stimmunterlagen.Core.Managers;

namespace Voting.Stimmunterlagen.Interceptors;

public class LanguageInterceptor : RequestInterceptor
{
    private const string LangHeader = "x-language";
    private readonly LanguageManager _languageManager;

    public LanguageInterceptor(LanguageManager languageManager)
    {
        _languageManager = languageManager;
    }

    protected override Task InterceptRequest<TRequest>(TRequest request, ServerCallContext context)
    {
        var language = context.RequestHeaders.GetValue(LangHeader);
        _languageManager.SetLanguage(language);
        return Task.CompletedTask;
    }
}
