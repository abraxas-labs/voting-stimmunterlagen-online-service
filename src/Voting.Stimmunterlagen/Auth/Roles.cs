// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Iam.Store;

namespace Voting.Stimmunterlagen.Auth;

internal static class Roles
{
    internal const string ElectionAdmin = "Wahlverwalter";
    internal const string PrintJobManager = "Auftragsmanager";

    internal static IReadOnlyCollection<string> AllRoles => [ElectionAdmin, PrintJobManager];

    internal static void EnsureElectionAdmin(this IAuth auth)
    {
        auth.EnsureRole(ElectionAdmin);
    }

    internal static void EnsurePrintJobManager(this IAuth auth)
    {
        auth.EnsureRole(PrintJobManager);
    }

    internal static bool IsElectionAdmin(this IAuth auth)
    {
        return auth.HasRole(ElectionAdmin);
    }

    internal static bool IsPrintJobManager(this IAuth auth)
    {
        return auth.HasRole(PrintJobManager);
    }
}
