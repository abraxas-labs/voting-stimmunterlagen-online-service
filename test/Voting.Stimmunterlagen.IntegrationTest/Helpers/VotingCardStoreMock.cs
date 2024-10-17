// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Voting.Stimmunterlagen.Core.Managers.Generator;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public class VotingCardStoreMock : IVotingCardStore
{
    private readonly Dictionary<string, string> _writtenFilesByMessageId = new();

    public void AssertFileWritten(string messageId, string fileName) => _writtenFilesByMessageId.GetValueOrDefault(messageId).Should().Contain(fileName);

    public void AssertFileNotWritten(string messageId, string fileName) => _writtenFilesByMessageId.GetValueOrDefault(messageId).Should().NotContain(fileName);

    public Task SaveVotingCards(string fileName, Stream stream, string messageId, CancellationToken ct)
    {
        _writtenFilesByMessageId.Add(messageId, fileName);
        return Task.CompletedTask;
    }

    public void Clear() => _writtenFilesByMessageId.Clear();
}
