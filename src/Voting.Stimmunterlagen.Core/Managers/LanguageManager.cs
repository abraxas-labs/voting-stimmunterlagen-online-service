// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using Microsoft.Extensions.Logging;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Data;

namespace Voting.Stimmunterlagen.Core.Managers;

public class LanguageManager
{
    private const string FallbackLanguage = Languages.German;
    private readonly ILogger<LanguageManager> _logger;
    private readonly DataContext _dataContext;

    public LanguageManager(ILogger<LanguageManager> logger, DataContext dataContext)
    {
        _logger = logger;
        _dataContext = dataContext;
    }

    public void SetLanguage(string? lang)
    {
        if (!Languages.All.Contains(lang))
        {
            _logger.LogWarning("Language {Lang} is not valid, using fallback language", lang);
            lang = FallbackLanguage;
        }
        else
        {
            _logger.LogDebug("Using language {Lang} for this request", lang);
        }

        _dataContext.Language = lang;
    }
}
