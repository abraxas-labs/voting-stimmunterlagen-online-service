// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Voting.Stimmunterlagen.EVoting.Configuration;
using Voting.Stimmunterlagen.EVoting.Mapper;
using Voting.Stimmunterlagen.EVoting.Models;
using Contest = Voting.Stimmunterlagen.EVoting.Models.Contest;

namespace Voting.Stimmunterlagen.EVoting;

public static class EVotingExportDataBuilder
{
    private const string CertificatesDirectoryPath = "certificates/";
    private const string CertificateExtension = ".cer";
    private const string TemplatesPath = "Export/Templates";

    public static byte[] BuildEVotingExport(
        byte[] eVotingZipBytes,
        Contest contest,
        byte[] ech0045XmlBytes,
        string ech0045XmlFileName,
        List<DomainOfInfluence> testDomainOfInfluences,
        DomainOfInfluence testDomainOfInfluenceDefaults,
        Dictionary<string, EVotingDomainOfInfluenceConfig> eVotingDomainOfInfluenceConfigByBfs)
    {
        using var ms = new MemoryStream();

        // archive needs to be disposed, otherwise the memory stream will not receive the update.
        using (var baseArchive = new ZipArchive(ms, ZipArchiveMode.Update))
        {
            AppendEVotingConfiguration(baseArchive, eVotingZipBytes, contest, testDomainOfInfluences, testDomainOfInfluenceDefaults, eVotingDomainOfInfluenceConfigByBfs);
            AppendEch0045(baseArchive, ech0045XmlBytes, ech0045XmlFileName);
        }

        return ms.ToArray();
    }

    private static void AppendEVotingConfiguration(
        ZipArchive baseArchive,
        byte[] eVotingZipBytes,
        Contest contest,
        List<DomainOfInfluence> testDomainOfInfluences,
        DomainOfInfluence testDomainOfInfluenceDefaults,
        Dictionary<string, EVotingDomainOfInfluenceConfig> eVotingDomainOfInfluenceConfigByBfs)
    {
        using var ms = new MemoryStream();
        ms.Write(eVotingZipBytes);

        // adds the config (params.json) and templates into the nested archive.
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Update))
        {
            var certificates = archive.Entries
                .Where(e => e.FullName.StartsWith(CertificatesDirectoryPath) && e.FullName.EndsWith(CertificateExtension))
                .Select(e => e.FullName)
                .ToList();

            var config = contest.ToConfiguration(testDomainOfInfluences, testDomainOfInfluenceDefaults, eVotingDomainOfInfluenceConfigByBfs, certificates);

            // config has newtonsoft json annotations
            var configJson = JsonConvert.SerializeObject(config);

            // add config
            AddFile(archive, EVotingDefaults.ConfigurationFileName, Encoding.UTF8.GetBytes(configJson));

            // add templates
            var templatesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, TemplatesPath);
            AddFolder(archive, EVotingDefaults.TemplatesFolderName, templatesPath);
        }

        AddFile(baseArchive, EVotingDefaults.EVotingConfigurationArchiveName, ms.ToArray());
    }

    private static void AppendEch0045(ZipArchive baseArchive, byte[] xmlBytes, string xmlFileName)
    {
        AddFile(baseArchive, xmlFileName, xmlBytes);
    }

    private static void AddFile(ZipArchive archive, string fileName, byte[] fileContent)
    {
        var configEntry = archive.CreateEntry(fileName);
        using var configEntryStream = configEntry.Open();
        configEntryStream.Write(fileContent);
    }

    private static void AddFolder(ZipArchive archive, string folderName, string folderPath)
    {
        archive.CreateEntry(folderName + "/");

        var files = Directory.GetFiles(folderPath);
        var directories = Directory.GetDirectories(folderPath);

        foreach (var filePath in files)
        {
            var fileName = string.Concat(folderName, "/", Path.GetFileName(filePath));
            AddFile(archive, fileName, File.ReadAllBytes(filePath));
        }

        foreach (var directory in directories)
        {
            var subFolderPath = string.Concat(folderName, "/", Path.GetFileName(directory));
            AddFolder(archive, subFolderPath, directory);
        }
    }
}
