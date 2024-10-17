// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.ObjectStorage.Config;

namespace Voting.Stimmunterlagen.Core.Configuration;

public class DomainOfInfluenceLogoObjectStorageBucketConfig : ObjectStorageBucketConfig
{
    // config needs to be in sync with voting basis
    public DomainOfInfluenceLogoObjectStorageBucketConfig()
    {
        BucketName = "voting";
        ObjectPrefix = "domain-of-influence-logos/";
        DefaultPublicDownloadLinkTtl = TimeSpan.FromMinutes(1);
    }

    public TimeSpan LogoCacheTtl { get; set; } = TimeSpan.FromMinutes(5);

    public TimeSpan LogoNotFoundCacheTtl { get; set; } = TimeSpan.FromMinutes(1);
}
