// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using PrintJobState = Voting.Stimmunterlagen.Data.Models.PrintJobState;

namespace Voting.Stimmunterlagen.IntegrationTest.PrintJobTests;

public abstract class SetPrintJobStateBaseTest : BaseWriteableDbGrpcTest<PrintJobService.PrintJobServiceClient>
{
    protected SetPrintJobStateBaseTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    protected static string DefaultDomainOfInfluenceId { get; } = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId;

    protected static Guid DefaultDomainOfInfluenceGuid { get; } = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid;

    protected async Task UpdateState(PrintJobState state)
    {
        await ModifyDbEntities<Data.Models.PrintJob>(
            x => x.DomainOfInfluenceId == DefaultDomainOfInfluenceGuid,
            x =>
            {
                x.State = state;
                switch (state)
                {
                    case PrintJobState.ProcessStarted:
                        x.ProcessStartedOn = MockedClock.GetDate(5);
                        break;
                    case PrintJobState.ProcessEnded:
                        x.ProcessStartedOn = MockedClock.GetDate(5);
                        x.ProcessEndedOn = MockedClock.GetDate(6);
                        break;
                    case PrintJobState.Done:
                        x.ProcessStartedOn = MockedClock.GetDate(5);
                        x.ProcessEndedOn = MockedClock.GetDate(6);
                        x.DoneOn = MockedClock.GetDate(7);
                        x.DoneComment = "done comment";
                        break;
                }
            });
    }
}
