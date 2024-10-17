// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public class VotingCardPrintFileStoreMock : IVotingCardPrintFileStore
{
    private readonly Dictionary<string, (string FileName, byte[] FileContent)> _writtenFilesByMessageId = new();

    public bool SaveFileInMemory { get; set; }

    public void AssertFileWritten(string messageId, string fileName) => _writtenFilesByMessageId.GetValueOrDefault(messageId).FileName.Should().Be(fileName);

    public void AssertFileNotWritten(string messageId, string fileName) => _writtenFilesByMessageId.GetValueOrDefault(messageId).FileName.Should().NotBe(fileName);

    public byte[] GetFile(string messageId) => _writtenFilesByMessageId.GetValueOrDefault(messageId).FileContent;

    public void Clear() => _writtenFilesByMessageId.Clear();

    public Task SavePrintFile(string fileName, byte[] content, string messageId, CancellationToken ct)
    {
        _writtenFilesByMessageId.Add(messageId, (fileName, SaveFileInMemory ? content : Array.Empty<byte>()));
        return Task.CompletedTask;
    }
}
