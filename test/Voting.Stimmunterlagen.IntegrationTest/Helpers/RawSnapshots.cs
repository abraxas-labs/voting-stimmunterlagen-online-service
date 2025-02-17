﻿// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

#if UPDATE_SNAPSHOTS
using System;
#else
using FluentAssertions;
#endif
using System.IO;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public static class RawSnapshots
{
    public static void MatchRawSnapshot(this string content, params string[] pathSegments)
    {
        var path = Path.Join(TestSourcePaths.TestProjectSourceDirectory, Path.Join(pathSegments));

#if UPDATE_SNAPSHOTS
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        if (!File.Exists(path) || !File.ReadAllText(path).Equals(content, StringComparison.Ordinal))
        {
            File.WriteAllText(path, content);
        }
#else
        File.ReadAllText(path)
            .Should()
            .Be(content);
#endif
    }
}
