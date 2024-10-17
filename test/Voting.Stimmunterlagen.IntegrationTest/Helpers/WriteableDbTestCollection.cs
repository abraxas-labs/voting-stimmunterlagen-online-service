// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

/// <summary>
/// This ensures only one test is run in parallel which needs write access to the db.
/// </summary>
[CollectionDefinition(Name)]
public class WriteableDbTestCollection : ICollectionFixture<WriteableDbTestCollection>
{
    public const string Name = "clean db for each test";
}
