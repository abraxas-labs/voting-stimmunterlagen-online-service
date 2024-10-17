// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Voting.Stimmunterlagen.Data.Utils;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Utils;

public class SqlUtilsTest
{
    [Theory]
    [InlineData("test", "test")]
    [InlineData("", "")]
    [InlineData("$test", "$$test")]
    [InlineData("te$st", "te$$st")]
    [InlineData("test$", "test$$")]
    [InlineData("tes%%t", "tes$%$%t")]
    [InlineData("t_est", "t$_est")]
    public void ShouldWork(string query, string expectedResult)
    {
        SqlUtils.EscapeLike(query)
            .Should()
            .Be(expectedResult);
    }
}
