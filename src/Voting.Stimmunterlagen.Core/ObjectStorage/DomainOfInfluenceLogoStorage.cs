// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Voting.Lib.ObjectStorage;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.ObjectStorage;

public class DomainOfInfluenceLogoStorage : BucketObjectStorageClient
{
    private readonly ApiConfig _config;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DomainOfInfluenceLogoStorage> _logger;

    public DomainOfInfluenceLogoStorage(ApiConfig config, IObjectStorageClient client, IMemoryCache cache, ILogger<DomainOfInfluenceLogoStorage> logger)
        : base(config.DomainOfInfluenceLogos, client)
    {
        _config = config;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string?> TryFetchAsBase64(ContestDomainOfInfluence doi)
    {
        if (doi.LogoRef == null)
        {
            return null;
        }

        return await _cache.GetOrCreateAsync(doi.LogoRef, async entry =>
        {
            try
            {
                var logo = await FetchAsBase64(doi.LogoRef);
                entry.AbsoluteExpirationRelativeToNow = _config.DomainOfInfluenceLogos.LogoCacheTtl;
                entry.Size = logo.Length * sizeof(char); // estimated size, not that accurate but should be sufficient
                return logo;
            }
            catch (Exception e)
            {
                _logger.LogWarning(
                    e,
                    "Could not find referenced domain of influence logo for doi {DoiName} (Id={DoiId}, LogoRef={LogoRef})",
                    doi.Name,
                    doi.BasisDomainOfInfluenceId,
                    doi.LogoRef);
                entry.AbsoluteExpirationRelativeToNow = _config.DomainOfInfluenceLogos.LogoCacheTtl;
                entry.Size = 0;
                return null;
            }
        });
    }
}
