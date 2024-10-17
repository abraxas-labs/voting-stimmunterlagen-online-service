// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.VoterListImportTests;

public class GetVoterListImportTest : BaseWriteableDbGrpcTest<VoterListImportService.VoterListImportServiceClient>
{
    public GetVoterListImportTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldReturn()
    {
        var response = await GemeindeArneggElectionAdminClient.GetAsync(new IdValueRequest { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggId });
        response.MatchSnapshot();
    }

    [Fact]
    public async Task ShouldThrowIfNotFound()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.DeleteAsync(new IdValueRequest { Id = "66875c4c-4bc4-4eba-9f18-a5ecefaa6c99" }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfNotOwner()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.DeleteAsync(new IdValueRequest { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggId }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(VoterListImportService.VoterListImportServiceClient service)
    {
        await service.DeleteAsync(new IdValueRequest { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
