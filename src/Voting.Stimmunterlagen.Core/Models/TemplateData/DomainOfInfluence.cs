// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Models.TemplateData;

public class DomainOfInfluence
{
    public string Name { get; set; } = string.Empty;

    public string ShortName { get; set; } = string.Empty;

    public string SecureConnectId { get; set; } = string.Empty;

    public DomainOfInfluenceVotingCardReturnAddress? ReturnAddress { get; set; }

    public DomainOfInfluenceVotingCardPrintData? PrintData { get; set; }

    public DomainOfInfluenceVotingCardSwissPostData? SwissPostData { get; set; }

    /// <summary>
    /// Gets or sets the logo as base64 encoded string as required by dmdoc.
    /// </summary>
    public string? Logo { get; set; }

    public VotingCardColor? VotingCardColor { get; set; }
}
