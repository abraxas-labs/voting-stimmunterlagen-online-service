// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Voting.Lib.ObjectStorage;
using Voting.Stimmunterlagen.Core.Configuration;

namespace Voting.Stimmunterlagen.Core.ObjectStorage;

public class EVotingZipStorage : BucketObjectStorageClient, IEVotingZipStorage
{
    private const string CacheKey = "EVoting";

    private readonly EVotingZipObjectStorageBucketConfig _eVotingZipConfig;
    private readonly IMemoryCache _cache;
    private readonly ILogger<EVotingZipStorage> _logger;
    private readonly string _objectName;

    public EVotingZipStorage(
        ApiConfig config,
        IObjectStorageClient client,
        IMemoryCache cache,
        ILogger<EVotingZipStorage> logger)
        : base(config.EVotingZip, client)
    {
        _eVotingZipConfig = config.EVotingZip;
        _cache = cache;
        _logger = logger;
        _objectName = config.EVotingZip.ObjectName;
    }

    public async Task<byte[]> Fetch()
    {
        return (await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            try
            {
                var eVotingZip = await FetchAsBase64(_objectName);
                entry.AbsoluteExpirationRelativeToNow = _eVotingZipConfig.ZipCacheTtl;
                entry.Size = eVotingZip.Length * sizeof(char); // estimated size, not that accurate but should be sufficient
                return Convert.FromBase64String(eVotingZip);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    "Could not find e-voting zip in object storage (object name: {ObjectName})",
                    _objectName);
                entry.AbsoluteExpirationRelativeToNow = _eVotingZipConfig.ZipCacheTtl;
                entry.Size = 0;
                throw;
            }
        }))!;
    }
}
