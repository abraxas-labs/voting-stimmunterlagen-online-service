// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading.Tasks;
using Voting.Stimmunterlagen.Core.ObjectStorage;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public class EVotingZipStorageMock : IEVotingZipStorage
{
    private static readonly string EVotingZipPath = Path.Join(
        Path.GetDirectoryName(typeof(EVotingZipStorageMock).Assembly.Location),
        "ContestEVotingExportJobTests",
        "TestFiles",
        "EVoting.zip");

    public Task<byte[]> Fetch()
    {
        return File.ReadAllBytesAsync(EVotingZipPath);
    }
}
