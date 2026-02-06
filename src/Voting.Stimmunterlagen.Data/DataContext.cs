// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Data.ModelBuilders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions options)
        : base(options)
    {
    }

    // The language used in this DbContext. Used to filter some translations/queries.
    public string? Language { get; set; }

    public DbSet<EventProcessingState> EventProcessingStates { get; set; } = null!;

    public DbSet<ContestOrderNumberState> ContestOrderNumberStates { get; set; } = null!;

    public DbSet<DomainOfInfluence> DomainOfInfluences { get; set; } = null!;

    public DbSet<DomainOfInfluenceHierarchyEntry> DomainOfInfluenceHierarchyEntries { get; set; } = null!;

    public DbSet<CountingCircle> CountingCircles { get; set; } = null!;

    public DbSet<DomainOfInfluenceCountingCircle> DomainOfInfluenceCountingCircles { get; set; } = null!;

    public DbSet<Contest> Contests { get; set; } = null!;

    public DbSet<ContestDomainOfInfluence> ContestDomainOfInfluences { get; set; } = null!;

    public DbSet<ContestDomainOfInfluenceHierarchyEntry> ContestDomainOfInfluenceHierarchyEntries { get; set; } = null!;

    public DbSet<ContestCountingCircle> ContestCountingCircles { get; set; } = null!;

    public DbSet<ContestDomainOfInfluenceCountingCircle> ContestDomainOfInfluenceCountingCircles { get; set; } = null!;

    public DbSet<PoliticalBusiness> PoliticalBusinesses { get; set; } = null!;

    public DbSet<PoliticalBusinessPermissionEntry> PoliticalBusinessPermissions { get; set; } = null!;

    public DbSet<StepState> StepStates { get; set; } = null!;

    public DbSet<Attachment> Attachments { get; set; } = null!;

    public DbSet<PoliticalBusinessAttachmentEntry> PoliticalBusinessAttachmentEntries { get; set; } = null!;

    public DbSet<PoliticalBusinessVoterListEntry> PoliticalBusinessVoterListEntries { get; set; } = null!;

    public DbSet<Voter> Voters { get; set; } = null!;

    public DbSet<VoterList> VoterLists { get; set; } = null!;

    public DbSet<VoterListImport> VoterListImports { get; set; } = null!;

    public DbSet<DomainOfInfluenceVoterDuplicate> DomainOfInfluenceVoterDuplicates { get; set; } = null!;

    public DbSet<ContestVotingCardLayout> ContestVotingCardLayouts { get; set; } = null!;

    public DbSet<DomainOfInfluenceVotingCardLayout> DomainOfInfluenceVotingCardLayouts { get; set; } = null!;

    public DbSet<DomainOfInfluenceAttachmentCount> DomainOfInfluenceAttachmentCounts { get; set; } = null!;

    public DbSet<Template> Templates { get; set; } = null!;

    public DbSet<TemplateDataContainer> TemplateDataContainers { get; set; } = null!;

    public DbSet<TemplateDataField> TemplateDataFields { get; set; } = null!;

    public DbSet<TemplateDataFieldValue> TemplateDataFieldValues { get; set; } = null!;

    public DbSet<DomainOfInfluenceVotingCardConfiguration> DomainOfInfluenceVotingCardConfigurations { get; set; } = null!;

    public DbSet<VotingCardGeneratorJob> VotingCardGeneratorJobs { get; set; } = null!;

    public DbSet<ManualVotingCardGeneratorJob> ManualVotingCardGeneratorJobs { get; set; } = null!;

    public DbSet<PrintJob> PrintJobs { get; set; } = null!;

    public DbSet<ContestEVotingExportJob> ContestEVotingExportJobs { get; set; } = null!;

    public DbSet<CantonSettings> CantonSettings { get; set; } = null!;

    public DbSet<VotingCardPrintFileExportJob> VotingCardPrintFileExportJobs { get; set; } = null!;

    public DbSet<AdditionalInvoicePosition> AdditionalInvoicePositions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        DbContextAccessor.DbContext = this;
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}
