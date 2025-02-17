﻿// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.IO;
using System.Reflection;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public static class TestSourcePaths
{
    public static readonly string TestProjectSourceDirectory = Path.Join(
        FindProjectSourceDirectory(),
        "test",
        "Voting.Stimmunterlagen.IntegrationTest");

    private static string FindProjectSourceDirectory()
    {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                  ?? throw new InvalidOperationException();

        do
        {
            if (Directory.GetFiles(dir, "*.sln", SearchOption.TopDirectoryOnly).Length > 0)
            {
                return dir;
            }

            dir = Path.GetDirectoryName(dir);
        }
        while (dir != null);

        throw new InvalidOperationException();
    }
}
