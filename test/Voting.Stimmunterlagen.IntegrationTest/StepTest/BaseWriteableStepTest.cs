// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Models;
using StepState = Voting.Stimmunterlagen.Data.Models.StepState;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest;

public abstract class BaseWriteableStepTest : BaseWriteableDbGrpcTest<StepService.StepServiceClient>
{
    protected BaseWriteableStepTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    // helper method to make it easier to call with the real step object
    protected Task SetStepApproved(Guid domainOfInfluenceId, Step step, bool approved)
        => SetStepApproved(domainOfInfluenceId, (Data.Models.Step)step, approved);

    protected Task SetStepApproved(Guid domainOfInfluenceId, Data.Models.Step step, bool approved)
    {
        return ModifyDbEntities<StepState>(x => x.DomainOfInfluenceId == domainOfInfluenceId && x.Step == step, x => x.Approved = approved);
    }

    // helper method to make it easier to call with the real step object
    protected Task AssertStepApproved(Guid domainOfInfluenceId, Step step, bool approved)
        => AssertStepApproved(domainOfInfluenceId, (Data.Models.Step)step, approved);

    protected async Task AssertStepApproved(Guid domainOfInfluenceId, Data.Models.Step step, bool approved)
    {
        var stepState = await FindDbEntity<StepState>(x => x.DomainOfInfluenceId == domainOfInfluenceId && x.Step == step);
        stepState.Approved.Should().Be(approved);
    }

    protected override async Task AuthorizationTestCall(StepService.StepServiceClient service)
        => await service.ApproveAsync(new());

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
