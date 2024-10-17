// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.Models;

public enum DomainOfInfluenceType
{
    Unspecified,

    /// <summary>
    /// Schweiz / Bund.
    /// </summary>
    Ch,

    /// <summary>
    /// Kanton.
    /// </summary>
    Ct,

    /// <summary>
    /// Bezirk.
    /// </summary>
    Bz,

    /// <summary>
    /// Gemeinde.
    /// </summary>
    Mu,

    /// <summary>
    /// Stadtkreis.
    /// </summary>
    Sk,

    /// <summary>
    /// Schulgemeinde.
    /// </summary>
    Sc,

    /// <summary>
    /// Kirchgemeinde.
    /// </summary>
    Ki,

    /// <summary>
    /// Ortsbürgergemeinde.
    /// </summary>
    Og,

    /// <summary>
    /// Koprorationen.
    /// </summary>
    Ko,

    /// <summary>
    /// Andere.
    /// </summary>
    An,
}
