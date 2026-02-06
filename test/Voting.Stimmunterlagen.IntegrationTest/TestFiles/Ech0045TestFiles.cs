// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;

namespace Voting.Stimmunterlagen.IntegrationTest.TestFiles;

public static class Ech0045TestFiles
{
    public const string File1Name = "eCH-0045-testfile1.xml";
    public const string File2InvalidName = "eCH-0045-testfile2-invalid.xml";
    public const string File3NameDomainOfInfluence = "eCH-0045-testfile3-domain-of-influence-church-school.xml";
    public const string File4MixedEVotingName = "eCH-0045-testfile4-mixed-evoting.xml";
    public const string File5MissingExtensionsName = "eCH-0045-testfile5-missing-extensions.xml";
    public const string FileDuplicatesName = "eCH-0045-testfile-duplicates.xml";

    public static string File1 => GetTestFilePath(File1Name);

    public static string File2Invalid => GetTestFilePath(File2InvalidName);

    public static string FileDuplicates => GetTestFilePath(FileDuplicatesName);

    public static string GetTestFilePath(string fileName)
    {
        var assemblyFolder = Path.GetDirectoryName(typeof(Ech0045TestFiles).Assembly.Location);
        return Path.Join(assemblyFolder, "TestFiles", fileName);
    }
}
