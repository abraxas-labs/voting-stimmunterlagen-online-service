// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Extensions;

public class AsyncEnummerableExtensionsTest
{
    [Fact]
    public async Task ChunkedShouldWork()
    {
        var expectedChunks = new[] { new[] { 0, 1, 2 }, new[] { 3, 4, 5 }, new[] { 6, 7, 8 }, new[] { 9 } };
        var i = 0;
        await foreach (var chunk in RangeAsync(0, 10).Chunked(3))
        {
            chunk
                .Should()
                .BeEquivalentTo(expectedChunks[i++]);
        }
    }

    private async IAsyncEnumerable<int> RangeAsync(int start, int count)
    {
        await Task.Delay(10);
        foreach (var x in Enumerable.Range(start, count))
        {
            yield return x;
        }
    }
}
