// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Voting.Stimmunterlagen.Data.ValidationAttributes;
using Xunit;

namespace Voting.Stimmunterlagen.Test.Validators.Attributes;

public class ValidLanguageAttributeTest
{
    [Theory]
    [InlineData(true, "de")]
    [InlineData(true, "fr")]
    [InlineData(true, "it")]
    [InlineData(true, "rm")]
    [InlineData(false, "en")]
    [InlineData(true, null)]
    public void ShouldWork(bool valid, object? value)
    {
        var attr = new ValidLanguageAttribute();
        attr.IsValid(value)
            .Should()
            .Be(valid);
    }
}
