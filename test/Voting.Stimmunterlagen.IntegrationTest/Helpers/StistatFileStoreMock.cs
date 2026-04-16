// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Voting.Stimmunterlagen.Core.Managers.Stistat;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public class StistatFileStoreMock : IStistatFileStore
{
    private readonly Dictionary<string, (string FileName, byte[] FileContent)> _writtenFilesByMessageType = new();

    public bool SaveFileInMemory { get; set; }

    public void AssertFileWritten(string messageType, string fileName) => _writtenFilesByMessageType.GetValueOrDefault(messageType).FileName.Should().Be(fileName);

    public void AssertNoFileWritten(string messageType) => _writtenFilesByMessageType.Should().NotContainKey(messageType);

    public byte[] GetFile(string messageType) => _writtenFilesByMessageType.GetValueOrDefault(messageType).FileContent;

    public void Clear()
    {
        _writtenFilesByMessageType.Clear();
        SaveFileInMemory = false;
    }

    public async Task Save(string fileName, Stream content, string messageType, CancellationToken ct)
    {
        if (SaveFileInMemory)
        {
            await using var ms = new MemoryStream();
            await content.CopyToAsync(ms, ct);
            _writtenFilesByMessageType[messageType] = (fileName, ms.ToArray());
        }
        else
        {
            _writtenFilesByMessageType[messageType] = (fileName, []);
        }
    }
}
