// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Extensions;

public class CollectionExtensionsTest
{
    [Fact]
    public void AddRangeShouldAddAllElements()
    {
        ICollection<int> collection = Enumerable.Range(1, 10).ToList();

        collection.AddRange(Enumerable.Range(11, 10));
        collection
            .Should()
            .BeEquivalentTo(Enumerable.Range(1, 20));
    }
}
