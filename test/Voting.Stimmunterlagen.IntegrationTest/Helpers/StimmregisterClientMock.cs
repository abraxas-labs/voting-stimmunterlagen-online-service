// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using Grpc.Core;
using Moq;
using RichardSzalay.MockHttp;
using Voting.Stimmregister.Proto.V1.Services;
using Voting.Stimmregister.Proto.V1.Services.Requests;
using Voting.Stimmregister.Proto.V1.Services.Responses;
using Voting.Stimmunterlagen.IntegrationTest.MockData.Stimmregister;
using Voting.Stimmunterlagen.IntegrationTest.TestFiles;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public static class StimmregisterClientMock
{
    private static readonly string TestFile1Content = File.ReadAllText(Ech0045TestFiles.File1);

    public static HttpMessageHandler CreateHttpClientMock()
    {
        var mock = new MockHttpMessageHandler();
        mock.When("*/v1/export/ech-0045")
            .Respond("text/xml", TestFile1Content);
        return mock;
    }

    public static void ConfigureFilterClientMock(Mock<FilterService.FilterServiceClient> mock)
    {
        mock
            .Setup(x => x.GetAllAsync(It.IsAny<FilterServiceGetAllRequest>(), null, null, CancellationToken.None))
            .Returns(GrpcTestUtils.CreateAsyncUnaryCall(new FilterServiceGetAllResponse
            {
                Filters = { StimmregisterFilterMockData.All },
            }));

        mock
            .Setup(x => x.GetSingleAsync(It.IsAny<FilterServiceGetSingleRequest>(), null, null, CancellationToken.None))
            .Returns((FilterServiceGetSingleRequest request, Metadata _, DateTime? _, CancellationToken _) =>
            {
                if (StimmregisterFilterMockData.Get(request.FilterId) is { } filter)
                {
                    return GrpcTestUtils.CreateAsyncUnaryCall(new FilterServiceGetSingleResponse { Filter = filter });
                }

                return GrpcTestUtils.CreateAsyncUnaryCall<FilterServiceGetSingleResponse>(StatusCode.NotFound);
            });

        mock
            .Setup(x => x.GetSingleVersionAsync(It.IsAny<FilterServiceGetSingleVersionRequest>(), null, null, CancellationToken.None))
            .Returns((FilterServiceGetSingleVersionRequest request, Metadata _, DateTime? _, CancellationToken _) =>
            {
                if (StimmregisterFilterMockData.GetByVersionId(request.FilterVersionId) is { } filterAndVersion)
                {
                    return GrpcTestUtils.CreateAsyncUnaryCall(new FilterServiceGetSingleFilterVersionResponse
                    {
                        Filter = filterAndVersion.Filter,
                        FilterVersion = filterAndVersion.Version,
                    });
                }

                return GrpcTestUtils.CreateAsyncUnaryCall<FilterServiceGetSingleFilterVersionResponse>(StatusCode.NotFound);
            });

        mock
            .Setup(x => x.GetMetadataAsync(It.IsAny<FilterServicePreviewMetadataRequest>(), null, null, CancellationToken.None))
            .Returns((FilterServicePreviewMetadataRequest request, Metadata _, DateTime? _, CancellationToken _) =>
            {
                if (StimmregisterFilterMockData.Get(request.FilterId) != null)
                {
                    return GrpcTestUtils.CreateAsyncUnaryCall(new FilterServicePreviewMetadataResponse
                    {
                        CountOfPersons = 10,
                        CountOfInvalidPersons = 3,
                    });
                }

                return GrpcTestUtils.CreateAsyncUnaryCall<FilterServicePreviewMetadataResponse>(StatusCode.NotFound);
            });

        mock
            .Setup(x => x.CreateVersionAsync(It.IsAny<FilterServiceCreateFilterVersionRequest>(), null, null, CancellationToken.None))
            .Returns((FilterServiceCreateFilterVersionRequest request, Metadata _, DateTime? _, CancellationToken _) =>
            {
                if (request.FilterId == StimmregisterFilterMockData.SwissWithVotingRightId)
                {
                    return GrpcTestUtils.CreateAsyncUnaryCall(new FilterServiceCreateVersionResponse
                    {
                        Id = StimmregisterFilterMockData.SwissWithVotingRightVersion4NewCreatedId,
                    });
                }

                return GrpcTestUtils.CreateAsyncUnaryCall<FilterServiceCreateVersionResponse>(StatusCode.NotFound);
            });
    }
}
