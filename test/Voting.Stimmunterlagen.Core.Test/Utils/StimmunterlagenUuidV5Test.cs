// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using FluentAssertions;
using Voting.Stimmunterlagen.Core.Utils;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Utils;

// this test ensures the generated uuid v5 stay consistent across changes
public class StimmunterlagenUuidV5Test
{
    [Fact]
    public void TestContestDomainOfInfluence()
    {
        StimmunterlagenUuidV5.BuildContestDomainOfInfluence(
                Guid.Parse("bba83d8e-532e-488d-b697-ecbf90cd9d12"),
                Guid.Parse("6d35256d-e05f-4914-a09d-c1783b5a1f9c"))
            .Should()
            .Be(Guid.Parse("fe00b61f-de7e-5a56-8538-88f187c4a709"));
    }
}
