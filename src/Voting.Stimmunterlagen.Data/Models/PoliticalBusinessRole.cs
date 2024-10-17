// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.Models;

public enum PoliticalBusinessRole
{
    /// <summary>
    /// The manager of this business (secure connect tenant of the domain of influence of this business).
    /// </summary>
    Manager,

    /// <summary>
    /// An attendee of this business
    /// (secure connect tenant of the DOI which is the manager of a political business or any of the DOIs child secure connect tenants).
    /// </summary>
    Attendee,
}
