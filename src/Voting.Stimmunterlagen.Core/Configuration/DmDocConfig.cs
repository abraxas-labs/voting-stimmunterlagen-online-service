// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Core.Configuration;

public class DmDocConfig : Lib.DmDoc.Configuration.DmDocConfig
{
    public DmDocConfig()
    {
        // DmDoc has a very slow document generation process.
        // Do not set a timeout until we switch to an asynchronous workflow.
        Timeout = null;
    }

    public string UserNamePrefix { get; set; } = "voting-stimmunterlagen-";

    /// <summary>
    /// Gets or sets callback base URL of the Voting Stimmunterlagen API.
    /// DmDoc sends via callback data such as voter page infos to a specific endpoint on this base address.
    /// </summary>
    public string CallbackBaseUrl { get; set; } = string.Empty;

    public bool EnableMock { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the callback of DmDoc is called immediately with mock data on the voting card generation.
    /// </summary>
    public bool MockedCallback { get; set; }

    public string GetVotingCardPdfCallbackUrl(string token)
        => $"{CallbackBaseUrl}voting-card-generator-job?token={System.Uri.EscapeDataString(token)}";
}
