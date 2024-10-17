// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Voting.Stimmunterlagen.Data.ValidationAttributes;
using Xunit;

namespace Voting.Stimmunterlagen.Test.Validators.Attributes;

public class ValidateObjectAttributeTest : ServiceProviderValidationAttributeBaseTest<ValidateObjectAttribute>
{
    [Fact]
    public void ShouldWork()
    {
        ShouldBeValid(new MyClass { Nested = new MyClass2 { Value = "foo" } });
    }

    [Fact]
    public void ShouldFailIfNestedFails()
    {
        ShouldBeInvalid(new MyClass { Nested = new MyClass2() }, "Validation of Nested failed");
    }

    [Fact]
    public void ShouldWorkIfNull()
    {
        ShouldBeValid(new MyClass());
    }

    private class MyClass
    {
        [ValidateObject]
        public MyClass2? Nested { get; set; }
    }

    private class MyClass2
    {
        [Required]
        public string? Value { get; set; }
    }
}
