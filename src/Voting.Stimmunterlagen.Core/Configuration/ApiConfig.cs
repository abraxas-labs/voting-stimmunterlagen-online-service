// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.DokConnector.Configuration;
using Voting.Lib.Ech.Configuration;
using Voting.Lib.ObjectStorage.Config;

namespace Voting.Stimmunterlagen.Core.Configuration;

public class ApiConfig
{
    public bool EnableDetailedErrors { get; set; }

    public bool EnableGrpcWeb { get; set; } // this should only be enabled for testing purposes

    public DmDocConfig DmDoc { get; set; } = new();

    public DocPipeConfig DocPipe { get; set; } = new();

    public bool SaveConnectFilesToFileSystem { get; set; }

    public VotingCardGeneratorConfig VotingCardGenerator { get; set; } = new();

    public ContestEVotingExportConfig ContestEVotingExport { get; set; } = new();

    public VotingCardPrintFileExportConfig VotingCardPrintFileExport { get; set; } = new();

    public InvoiceConfig Invoice { get; set; } = new();

    /// <summary>
    /// Gets or sets the batch size of voters when inserting voter lists into the database.
    /// Higher values use a lot more memory (since more objects are created in the same batch).
    /// Lower value in turn take much longer to process.
    /// Based on trial and error, batch sizes around 100 are a good trade off in terms of memory and performance.
    /// </summary>
    public int VoterListInsertBatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the configuration of the configuration with the VO Stimmregister.
    /// </summary>
    public StimmregisterConfig Stimmregister { get; set; } = new();

    public ObjectStorageConfig ObjectStorage { get; set; } = new()
    {
        Endpoint = "localhost:9000",
        AccessKey = "user",
        SecretKey = "password",
    };

    public DomainOfInfluenceLogoObjectStorageBucketConfig DomainOfInfluenceLogos { get; set; } = new();

    public EVotingZipObjectStorageBucketConfig EVotingZip { get; set; } = new();

    public DokConnectorConfig DokConnector { get; set; } = new();

    public HashSet<string> LanguageHeaderIgnoredPaths { get; set; } = new()
    {
        "/healthz",
        "/metrics",
    };

    public EchConfig Ech { get; set; } = new(typeof(AppConfig).Assembly);
}
