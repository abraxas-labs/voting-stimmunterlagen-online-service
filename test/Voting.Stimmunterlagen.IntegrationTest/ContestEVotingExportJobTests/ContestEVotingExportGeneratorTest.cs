// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Ech0045_4_0;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.Managers.EVoting;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;
using Voting.Stimmunterlagen.EVoting;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.OfflineClient.Shared.ContestConfiguration;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.ContestEVotingExportJobTests;

public class ContestEVotingExportGeneratorTest : BaseWriteableDbTest
{
    private static readonly Guid _defaultJobId = ContestEVotingExportJobMockData.BundFutureApprovedGuid;
    private static readonly string _defaultMessageId = CantonSettingsMockData.StGallen.VotingDocumentsEVotingEaiMessageType;
    private readonly ContestEVotingStoreMock _storeMock;

    public ContestEVotingExportGeneratorTest(TestApplicationFactory factory)
        : base(factory)
    {
        _storeMock = GetService<ContestEVotingStoreMock>();
        _storeMock.Clear();

        GetService<ContestEVotingExportThrottlerMock>().ShouldBlock = false;
    }

    [Fact]
    public async Task ShouldWork()
    {
        await ModifyDbEntities<ContestCountingCircle>(
            x => x.Id == CountingCircleMockData.ContestBundFutureApprovedStadtUzwilGuid
                 || x.Id == CountingCircleMockData.ContestBundFutureApprovedStadtGossauGuid
                 || x.Id == CountingCircleMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            x => x.EVoting = true);

        await RunOnDb(async db =>
        {
            var attachment = await db.Attachments
                .AsTracking()
                .Include(a => a.PoliticalBusinessEntries)
                .SingleAsync(a => a.Id == AttachmentMockData.BundFutureApprovedBund1Guid);
            attachment.Station = 11;
            attachment.PoliticalBusinessEntries.Add(new() { PoliticalBusinessId = VoteMockData.BundFutureApprovedKantonStGallen1Guid });

            // Voters need to belong to a job (= No duplicate) to be considered in the E-Voting export and "Gut zum Druck" step must be completed..
            var voters = await db.Voters
                .AsTracking()
                .Where(v => v.ListId == VoterListMockData.BundFutureApprovedGemeindeArneggEVoterGuid
                    || v.ListId == VoterListMockData.BundFutureApprovedStadtGossauEVoterGuid)
                .ToListAsync();

            foreach (var voter in voters)
            {
                voter.JobId = VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid;
            }

            var dois = await db.ContestDomainOfInfluences
                .AsTracking()
                .Where(doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid || doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid)
                .ToListAsync();

            foreach (var doi in dois)
            {
                doi.GenerateVotingCardsTriggered = MockedClock.UtcNowDate;
            }

            await db.SaveChangesAsync();
        });

        _storeMock.SaveFileInMemory = true;

        await SetState(_defaultJobId, ExportJobState.ReadyToRun);
        await Run(_defaultJobId);
        var job = await GetDbEntity<ContestEVotingExportJob>(x =>
            x.Id == _defaultJobId);

        job.Started.Should().Be(MockedClock.UtcNowDate);
        job.Completed.Should().Be(MockedClock.UtcNowDate);
        job.State.Should().Be(ExportJobState.Completed);
        job.FileHash.Should().NotBeEmpty();
        _storeMock.AssertFileWritten(_defaultMessageId, job.FileName);

        var fileContent = _storeMock.GetFile(_defaultMessageId);

        using var baseArchiveMs = new MemoryStream(fileContent);
        using var baseArchive = new ZipArchive(baseArchiveMs);
        using var eVotingConfigurationArchiveMs = new MemoryStream(ReadFileBytesFromArchive(baseArchive, EVotingDefaults.EVotingConfigurationArchiveName));
        using var eVotingConfigurationArchive = new ZipArchive(eVotingConfigurationArchiveMs);

        var baseArchiveFileNameEntries = baseArchive.Entries.Select(e => e.FullName).ToList();
        var eVotingConfigurationArchiveNameEntries = eVotingConfigurationArchive.Entries.Select(e => e.FullName).OrderBy(e => e).ToList();
        baseArchiveFileNameEntries.MatchSnapshot("baseArchiveFileNames");
        eVotingConfigurationArchiveNameEntries.MatchSnapshot("eVotingConfigurationArchiveFileNames");

        var configJson = ReadFileStringFromArchive(eVotingConfigurationArchive, EVotingDefaults.ConfigurationFileName);
        var ech45Xml = ReadFileStringFromArchive(baseArchive, "file-name.xml");
        var ech45XmlStream = new MemoryStream(Encoding.UTF8.GetBytes(ech45Xml));
        var ech45Deserialized = new XmlSerializer(typeof(VoterDelivery)).Deserialize(ech45XmlStream);

        JsonConvert.DeserializeObject<Configuration>(configJson).MatchSnapshot("configJson");
        ech45Deserialized.MatchSnapshot("eCH-45");

        using var sha512 = SHA512.Create();
        var expectedFileHash = Convert.ToBase64String(sha512.ComputeHash(fileContent));
        job.FileHash.Should().Be(expectedFileHash);
    }

    [Fact]
    public async Task ShouldWorkWithNoDoiEVotingReady()
    {
        await ModifyDbEntities<ContestCountingCircle>(
            x => x.Id == CountingCircleMockData.ContestBundFutureApprovedStadtUzwilGuid
                || x.Id == CountingCircleMockData.ContestBundFutureApprovedStadtGossauGuid
                || x.Id == CountingCircleMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            x => x.EVoting = true);

        await RunOnDb(async db =>
        {
            var dois = await db.ContestDomainOfInfluences
                .Where(doi => doi.ContestId == ContestMockData.BundFutureApprovedGuid)
                .ToListAsync();

            foreach (var doi in dois)
            {
                doi.LogoRef = null;
            }

            db.ContestDomainOfInfluences.UpdateRange(dois);
            await db.SaveChangesAsync();
        });

        _storeMock.SaveFileInMemory = true;

        await SetState(_defaultJobId, ExportJobState.ReadyToRun);
        await Run(_defaultJobId);

        var fileContent = _storeMock.GetFile(_defaultMessageId);

        using var baseArchiveMs = new MemoryStream(fileContent);
        using var baseArchive = new ZipArchive(baseArchiveMs);
        using var eVotingConfigurationArchiveMs = new MemoryStream(ReadFileBytesFromArchive(baseArchive, EVotingDefaults.EVotingConfigurationArchiveName));
        using var eVotingConfigurationArchive = new ZipArchive(eVotingConfigurationArchiveMs);

        var configJson = ReadFileStringFromArchive(eVotingConfigurationArchive, EVotingDefaults.ConfigurationFileName);
        var deserializedConfig = JsonConvert.DeserializeObject<Configuration>(configJson);

        var testDoiCount = GetService<ApiConfig>().ContestEVotingExport.TestDomainOfInfluences.Sum(x => x.Value.Count);
        deserializedConfig!.Printings.Single().Municipalities.Should().HaveCount(testDoiCount);
    }

    [Fact]
    public async Task AlreadyLockedShouldThrow()
    {
        using var scope = GetService<IServiceScopeFactory>().CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var jobRepo = scope.ServiceProvider.GetRequiredService<IDbRepository<ContestEVotingExportJob>>();
        var locked = await jobRepo.TryLockForUpdate(_defaultJobId);
        locked.Should().BeTrue();

        await AssertException<ValidationException>(
            async () => await Run(_defaultJobId),
            "since it is locked");
    }

    [Theory]
    [InlineData(ExportJobState.Unspecified, "job state is unspecified")]
    [InlineData(ExportJobState.Pending, "job is inactive")]
    [InlineData(ExportJobState.Running, "job is currently running")]
    [InlineData(ExportJobState.Completed, "job is already completed")]
    public async Task InvalidStatesShouldThrow(ExportJobState state, string ex)
    {
        await SetState(_defaultJobId, state);
        await AssertException<ValidationException>(
            async () => await Run(_defaultJobId),
            ex);
        await AssertIsFailed(_defaultJobId);
    }

    private async Task AssertIsFailed(Guid jobId)
    {
        var job = await GetDbEntity<ContestEVotingExportJob>(x => x.Id == jobId);
        _storeMock.AssertFileNotWritten(_defaultMessageId, job.FileName);
        job.State.Should().Be(ExportJobState.Failed);
        job.Failed.Should().Be(MockedClock.UtcNowDate);
    }

    private Task SetState(Guid jobId, ExportJobState state)
    {
        return RunOnDb(async db =>
        {
            var job = await db.ContestEVotingExportJobs.FindAsync(jobId);
            job!.State = state;
            db.Update(job);
            await db.SaveChangesAsync();
        });
    }

    private async Task Run(Guid jobId)
    {
        using var scope = GetService<IServiceScopeFactory>().CreateScope();
        var generator = scope.ServiceProvider.GetRequiredService<ContestEVotingExportGenerator>();
        await generator.Run(jobId);
    }

    private string ReadFileStringFromArchive(ZipArchive archive, string fullFileName)
    {
        var configEntry = archive.Entries.Single(x => x.FullName == fullFileName);
        using var configEntryStream = configEntry.Open();
        using var reader = new StreamReader(configEntryStream);
        return reader.ReadToEnd();
    }

    private byte[] ReadFileBytesFromArchive(ZipArchive archive, string fullFileName)
    {
        var configEntry = archive.Entries.Single(x => x.FullName == fullFileName);
        using var configEntryStream = configEntry.Open();
        using var ms = new MemoryStream();
        configEntryStream.CopyTo(ms);
        return ms.ToArray();
    }
}
