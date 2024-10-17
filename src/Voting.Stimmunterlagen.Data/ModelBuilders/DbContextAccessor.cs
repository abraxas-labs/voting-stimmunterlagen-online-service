// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

internal static class DbContextAccessor
{
    // Needed to have access to the DataContext inside model builders
    internal static DataContext DbContext { get; set; } = null!;
}
