// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Grpc.Configuration;

namespace Voting.Stimmunterlagen.Core.Configuration;

public class StimmregisterConfig
{
    public GrpcMode Mode { get; set; } = GrpcMode.GrpcWebText;

    public Uri? GrpcEndpoint { get; set; }

    public Uri? RestEndpoint { get; set; }

    public bool UseUnsafeInsecureChannelCallCredentials { get; set; }
}
