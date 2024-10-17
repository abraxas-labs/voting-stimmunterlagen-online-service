// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.Managers;

namespace Voting.Stimmunterlagen.Middlewares;

public class LanguageMiddleware
{
    private const string LangHeader = "x-language";

    private readonly RequestDelegate _next;
    private readonly ApiConfig _config;

    public LanguageMiddleware(RequestDelegate next, ApiConfig config)
    {
        _next = next;
        _config = config;
    }

    public Task Invoke(HttpContext context, LanguageManager languageManager)
    {
        if (context.Request.Path.Value == null || _config.LanguageHeaderIgnoredPaths.Contains(context.Request.Path.Value))
        {
            return _next(context);
        }

        var language = GetLanguage(context.Request.Headers);
        languageManager.SetLanguage(language);
        return _next(context);
    }

    private string? GetLanguage(IHeaderDictionary requestHeaders)
    {
        return requestHeaders.TryGetValue(LangHeader, out var headerValue)
            ? (string?)headerValue
            : null;
    }
}
