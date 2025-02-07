// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Core.Models.Invoice;

public enum MaterialCategory
{
    Unspecified,

    /// <summary>
    /// One time charge (1x).
    /// </summary>
    Flatrate,

    /// <summary>
    /// Charge for each voter.
    /// </summary>
    Voter,

    /// <summary>
    /// Charge for each voter excluding <see cref="Data.Models.Voter.SendVotingCardsToDomainOfInfluenceReturnAddress"/>.
    /// </summary>
    VoterExcludeRest,

    /// <summary>
    /// Charge for each attachment station.
    /// </summary>
    AttachmentStationsSetup,

    /// <summary>
    /// Charges the highest <see cref="AttachmentCategorySummary.TotalRequiredForVoterListsCount"/> and the attachment stations.
    /// Requires a <see cref="Configuration.MaterialConfig.Stations"/> value.
    /// </summary>
    AttachmentStations,

    /// <summary>
    /// Charge separatly (user-defined).
    /// </summary>
    AdditionalInvoicePosition,

    /// <summary>
    /// Charge if standard ballot envelope is set (but not if custom) for each voter excluding rest.
    /// </summary>
    BallotEnvelopeStandardExclCustom,
}
