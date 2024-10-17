// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace System.Collections.Generic;

public static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> toAdd)
    {
        foreach (var item in toAdd)
        {
            collection.Add(item);
        }
    }
}
