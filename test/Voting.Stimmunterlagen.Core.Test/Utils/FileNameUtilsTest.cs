// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Voting.Stimmunterlagen.Core.Utils;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Utils;

public class FileNameUtilsTest
{
    [Theory]
    [InlineData("test", "test")]
    [InlineData("test/", "test")]
    [InlineData("/test", "test")]
    [InlineData("/test/", "test")]
    [InlineData("test/a_b\0c", "test_a_b_c")]
    public void ShouldWork(string invalidFileName, string expectedReplacement)
    {
        FileNameUtils.SanitizeFileName(invalidFileName)
            .Should()
            .Be(expectedReplacement);
    }
}
