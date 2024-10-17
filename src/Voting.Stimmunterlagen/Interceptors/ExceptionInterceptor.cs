// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Exceptions;
using LibExceptionInterceptor = Voting.Lib.Grpc.Interceptors.ExceptionInterceptor;

namespace Voting.Stimmunterlagen.Interceptors;

public class ExceptionInterceptor : LibExceptionInterceptor
{
    public ExceptionInterceptor(ApiConfig config, ILogger<ExceptionInterceptor> logger)
        : base(logger, config.EnableDetailedErrors)
    {
    }

    protected override StatusCode MapExceptionToStatusCode(Exception ex) => ExceptionMapping.MapToGrpcStatusCode(ex);

    protected override bool ExposeExceptionType(Exception ex) => ExceptionMapping.ExposeExceptionType(ex);
}
