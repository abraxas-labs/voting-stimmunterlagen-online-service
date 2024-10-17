// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.Models;

public enum Salutation
{
    /// <summary>
    /// Unspecified.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// Mrs. (married).
    /// Frau.
    /// </summary>
    Mistress,

    /// <summary>
    /// Mr.
    /// Herr.
    /// </summary>
    Mister,

    /// <summary>
    /// Ms. (not married).
    /// Fräulein.
    /// </summary>
    Miss,
}
