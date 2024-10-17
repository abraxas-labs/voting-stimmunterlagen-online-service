// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Data.ValidationAttributes;
using Xunit;

namespace Voting.Stimmunterlagen.Test.Validators.Attributes;

public class PastAttributeTest : ServiceProviderValidationAttributeBaseTest<PastAttribute>
{
    [Fact]
    public void NowShouldBeInvalid()
    {
        ShouldBeInvalid(new MyData { Value = MockedClock.UtcNowDate }, "Value needs to be before");
    }

    [Fact]
    public void FutureShouldBeInvalid()
    {
        ShouldBeInvalid(new MyData { Value = MockedClock.GetDate(1) }, "Value needs to be before");
    }

    [Fact]
    public void PastShouldBeValid()
    {
        ShouldBeValid(new MyData { Value = MockedClock.GetDate(-1) });
    }

    private class MyData
    {
        [Past]
        public DateTime Value { get; set; }
    }
}
