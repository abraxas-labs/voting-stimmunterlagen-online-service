// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace System.Collections.Generic;

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<TSource[]> Chunked<TSource>(
        this IAsyncEnumerable<TSource> source,
        int size)
    {
        await using var e = source.GetAsyncEnumerator();
        while (await e.MoveNextAsync())
        {
            var array = new TSource[size];
            array[0] = e.Current;

            var newSize = 1;
            for (; newSize < array.Length && await e.MoveNextAsync(); newSize++)
            {
                array[newSize] = e.Current;
            }

            if (newSize != array.Length)
            {
                Array.Resize(ref array, newSize);
            }

            yield return array;
        }
    }
}
