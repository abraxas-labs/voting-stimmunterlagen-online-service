// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Snapper;
using Voting.Stimmunterlagen.Core.Utils;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Utils;

public class EmptyVoterBuilderTest
{
    [Fact]

    public void BuildEmtyVoterGenerationListShouldWork()
    {
        var voters = EmptyVoterBuilder.BuildEmptyVoters("1234", 7, 2);
        voters.ShouldMatchSnapshot();
    }
}
