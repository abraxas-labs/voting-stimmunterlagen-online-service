// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.Proto.V1;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest;

public class BaseReadOnlyStepTest : BaseReadOnlyGrpcTest<StepService.StepServiceClient>
{
    public BaseReadOnlyStepTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    protected override async Task AuthorizationTestCall(StepService.StepServiceClient service)
        => await service.ApproveAsync(new());

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
