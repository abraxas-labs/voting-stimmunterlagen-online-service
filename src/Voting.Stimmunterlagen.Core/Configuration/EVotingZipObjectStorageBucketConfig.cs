// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.ObjectStorage.Config;
using Voting.Stimmunterlagen.EVoting;

namespace Voting.Stimmunterlagen.Core.Configuration;

public class EVotingZipObjectStorageBucketConfig : ObjectStorageBucketConfig
{
    public EVotingZipObjectStorageBucketConfig()
    {
        BucketName = "voting-stimmunterlagen";
        ObjectPrefix = "evoting/";
    }

    public string ObjectName { get; set; } = EVotingDefaults.EVotingConfigurationArchiveName;

    public TimeSpan ZipCacheTtl { get; set; } = TimeSpan.FromMinutes(30);
}
