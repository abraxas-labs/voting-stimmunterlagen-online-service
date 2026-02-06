// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Snapper;
using Voting.Lib.Common;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.VotingCardPrintFileTests;

public class VotingCardPrintFileExportGeneratorTest : BaseWriteableDbTest
{
    private const string DefaultMessageId = "voting-card-generator-message-id-mock";
    private static readonly Guid JobId = VotingCardPrintFileExportJobMockData.BundFutureApprovedGemeindeArneggJob1Guid;
    private readonly VotingCardPrintFileStoreMock _storeMock;

    public VotingCardPrintFileExportGeneratorTest(TestApplicationFactory factory)
        : base(factory)
    {
        _storeMock = GetService<VotingCardPrintFileStoreMock>();
        _storeMock.Clear();

        GetService<VotingCardPrintFileExportThrottlerMock>().ShouldBlock = false;
    }

    [Fact]
    public async Task ShouldWork()
    {
        await SeedVoters();
        _storeMock.SaveFileInMemory = true;
        await RunJob();
        var job = await GetDbEntity<VotingCardPrintFileExportJob>(x =>
            x.Id == JobId);

        job.Started.Should().Be(MockedClock.UtcNowDate);
        job.Completed.Should().Be(MockedClock.UtcNowDate);
        job.Runner.Should().Be(Environment.MachineName);
        job.State.Should().Be(ExportJobState.Completed);

        _storeMock.AssertFileWritten(DefaultMessageId, job.FileName);

        var csv = Encoding.UTF8.GetString(_storeMock.GetFile(DefaultMessageId));
        csv.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldWorkWithEmptyVotingCards()
    {
        await SeedVoters();
        _storeMock.SaveFileInMemory = true;

        await ModifyDbEntities<Voter>(
            v => v.List!.Id == VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid,
            v =>
            {
                v.PageInfo = new()
                {
                    PageFrom = 10,
                    PageTo = 11,
                };
            });

        var jobId = VotingCardPrintFileExportJobMockData.BundFutureApprovedGemeindeArneggEmptyVcJobGuid;

        await RunJob(jobId);
        var job = await GetDbEntity<VotingCardPrintFileExportJob>(x =>
            x.Id == jobId);

        job.Started.Should().Be(MockedClock.UtcNowDate);
        job.Completed.Should().Be(MockedClock.UtcNowDate);
        job.Runner.Should().Be(Environment.MachineName);
        job.State.Should().Be(ExportJobState.Completed);

        _storeMock.AssertFileWritten(DefaultMessageId, job.FileName);

        var csv = Encoding.UTF8.GetString(_storeMock.GetFile(DefaultMessageId));
        csv.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task AlreadyLockedShouldThrow()
    {
        using var scope = GetService<IServiceScopeFactory>().CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var jobRepo = scope.ServiceProvider.GetRequiredService<IDbRepository<VotingCardPrintFileExportJob>>();
        var locked = await jobRepo.TryLockForUpdate(JobId);
        locked.Should().BeTrue();

        await AssertException<ValidationException>(
            async () => await RunJob(),
            "since it is locked");
    }

    [Theory]
    [InlineData(ExportJobState.Running, "job is currently running")]
    [InlineData(ExportJobState.Completed, "job is already completed")]
    public async Task InvalidStatesShouldThrow(ExportJobState state, string ex)
    {
        await SetState(JobId, state);
        await AssertException<ValidationException>(
            async () => await RunJob(),
            ex);
        await AssertIsFailed(JobId);
    }

    private async Task AssertIsFailed(Guid jobId)
    {
        var job = await GetDbEntity<VotingCardPrintFileExportJob>(x => x.Id == jobId);
        _storeMock.AssertFileNotWritten(DefaultMessageId, job.FileName);
        job.State.Should().Be(ExportJobState.Failed);
        job.Failed.Should().Be(MockedClock.UtcNowDate);
        job.Completed.Should().BeNull();
    }

    private Task SetState(Guid jobId, ExportJobState state)
    {
        return RunOnDb(async db =>
        {
            var job = await db.VotingCardPrintFileExportJobs.FindAsync(jobId);
            job!.State = state;
            if (state == ExportJobState.Completed)
            {
                job.Completed = MockedClock.UtcNowDate;
            }

            db.Update(job);
            await db.SaveChangesAsync();
        });
    }

    private async Task RunJob(Guid? jobId = null)
    {
        using var scope = GetService<IServiceScopeFactory>().CreateScope();
        var generator = scope.ServiceProvider.GetRequiredService<VotingCardPrintFileExportGenerator>();
        await generator.Run(jobId ?? JobId);
    }

    private async Task SeedVoters()
    {
        await RunOnDb(async db =>
        {
            var voter1 = new Voter
            {
                ListId = VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid,
                FirstName = "Arnd",
                LastName = "Thalberg",
                Street = "Damunt",
                HouseNumber = "149",
                DwellingNumber = "2. Stock",
                Town = "Gündlikon",
                ForeignZipCode = "DE-91801",
                Country = { Iso2 = "DE", Name = "Deutschland" },
                Bfs = "1234",
                LanguageOfCorrespondence = Languages.German,
                VotingCardType = VotingCardType.Swiss,
                DateOfBirth = "1980-07-21",
                Sex = SexType.Male,
                VoterType = VoterType.Swiss,
                PersonId = "3",
                PersonIdCategory = "Inlandschweizer",
                MunicipalityName = "Arnegg",
                SendVotingCardsToDomainOfInfluenceReturnAddress = true,
                PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                {
                    new()
                    {
                        Name = "St. Gallen",
                        Canton = CantonAbbreviation.SG,
                    },
                },
                PageInfo = new()
                {
                    PageFrom = 1,
                    PageTo = 2,
                },
                ContestId = ContestMockData.BundFutureApprovedGuid,
            };
            var voter2 = new Voter
            {
                ListId = VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid,
                FirstName = "Torsten",
                LastName = "Meister",
                Street = "Rhosddu Rd",
                HouseNumber = "72",
                Town = "Forest",
                ForeignZipCode = "SA4 1RQ",
                Country = { Iso2 = "GB", Name = "Vereinigtes Königreich" },
                Bfs = "1234",
                LanguageOfCorrespondence = Languages.Italian,
                VotingCardType = VotingCardType.EVoting,
                DateOfBirth = "1962-10-09",
                Sex = SexType.Male,
                VoterType = VoterType.SwissAbroad,
                PersonId = "102",
                PersonIdCategory = "Auslandschweizer",
                SwissAbroadPerson = new()
                {
                    DateOfRegistration = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ResidenceCountry = new() { Iso2 = "IT", Name = "Italien" },
                    Extension = new()
                    {
                        PostageCode = "2005",
                        Address = new()
                        {
                            Line1 = "Torsten Meister",
                            Line2 = "Rhosddu Rd 72",
                            Line3 = "Forest",
                            Line4 = "SA4 1RQ",
                            Line7 = "UNITED KINGDOM",
                        },
                        Authority = new()
                        {
                            Organisation = new()
                            {
                                Name = "Staatskanzlei",
                                AddOn1 = "St. Gallen",
                            },
                            AddressLine1 = "Kanton St. Gallen",
                            AddressLine2 = "Staatskanzlei",
                            Street = "Regierungsgebäude",
                            Town = "St. Gallen",
                            SwissZipCode = 9001,
                            Country = new()
                            {
                                Iso2 = "CH",
                                Name = "SWITZERLAND",
                            },
                        },
                    },
                },
                MunicipalityName = "Arnegg",
                PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                {
                    new()
                    {
                        Name = "Bellinzona",
                        Canton = CantonAbbreviation.TI,
                    },
                },
                PageInfo = new()
                {
                    PageFrom = 3,
                    PageTo = 4,
                },
                ContestId = ContestMockData.BundFutureApprovedGuid,
            };

            var voter1PrintDisabled = new Voter
            {
                ListId = VoterListMockData.BundFutureApprovedGemeindeArneggSwissElectoralRegisterGuid,
                FirstName = "Arnd",
                LastName = "Thalberg",
                Street = "Damunt",
                HouseNumber = "149",
                DwellingNumber = "2. Stock",
                Town = "Gündlikon",
                ForeignZipCode = "DE-91801",
                Country = { Iso2 = "DE", Name = "Deutschland" },
                Bfs = "1234",
                LanguageOfCorrespondence = Languages.German,
                VotingCardType = VotingCardType.Swiss,
                DateOfBirth = "1980-07-21",
                Sex = SexType.Male,
                VoterType = VoterType.Swiss,
                PersonId = "3",
                PersonIdCategory = "Inlandschweizer",
                MunicipalityName = "Arnegg",
                SendVotingCardsToDomainOfInfluenceReturnAddress = true,
                PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                {
                    new()
                    {
                        Name = "St. Gallen",
                        Canton = CantonAbbreviation.SG,
                    },
                },
                PageInfo = new()
                {
                    PageFrom = 1,
                    PageTo = 2,
                },
                ContestId = ContestMockData.BundFutureApprovedGuid,
                VotingCardPrintDisabled = true,
                VoterDuplicate = new DomainOfInfluenceVoterDuplicate
                {
                    FirstName = "Arnd",
                    LastName = "Thalberg",
                    DateOfBirth = "1980-07-21",
                    DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
                },
            };

            db.Voters.Add(voter1PrintDisabled);
            await db.SaveChangesAsync();

            voter1.VoterDuplicateId = voter1PrintDisabled.VoterDuplicateId;

            var job = await db.VotingCardPrintFileExportJobs
                .AsTracking()
                .Include(j => j.VotingCardGeneratorJob!.Layout!.OverriddenTemplate)
                .SingleAsync(j => j.Id == JobId);

            job.VotingCardGeneratorJob!.Voter.Add(voter1);
            job.VotingCardGeneratorJob.Voter.Add(voter2);
            job.VotingCardGeneratorJob!.Layout!.OverriddenTemplate!.InternName = "voting_template";
            await db.SaveChangesAsync();
        });
    }
}
