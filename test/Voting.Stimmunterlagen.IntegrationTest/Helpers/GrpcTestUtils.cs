// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

internal static class GrpcTestUtils
{
    public static Timestamp CreateTimestamp(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
        => DateTime.SpecifyKind(new DateTime(year, month, day, hour, minute, second), DateTimeKind.Utc).ToTimestamp();

    public static AsyncUnaryCall<TResponse> CreateAsyncUnaryCall<TResponse>(TResponse response)
    {
        return new AsyncUnaryCall<TResponse>(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
    }

    public static AsyncUnaryCall<TResponse> CreateAsyncUnaryCall<TResponse>(StatusCode statusCode)
    {
        var status = new Status(statusCode, string.Empty);
        return new AsyncUnaryCall<TResponse>(
            Task.FromException<TResponse>(new RpcException(status)),
            Task.FromResult(new Metadata()),
            () => status,
            () => new Metadata(),
            () => { });
    }
}
