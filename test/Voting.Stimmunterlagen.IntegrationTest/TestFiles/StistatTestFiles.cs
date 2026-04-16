// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;

namespace Voting.Stimmunterlagen.IntegrationTest.TestFiles;

public static class StistatTestFiles
{
    public const string File1Name = "stistat-testfile1.csv";
    public const string File2Name = "stistat-testfile2.csv";

    public static string File1 => GetTestFilePath(File1Name);

    public static string File2 => GetTestFilePath(File2Name);

    public static string GetTestFilePath(string fileName)
    {
        var assemblyFolder = Path.GetDirectoryName(typeof(StistatTestFiles).Assembly.Location);
        return Path.Join(assemblyFolder, "TestFiles", fileName);
    }
}
