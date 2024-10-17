// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;

namespace Voting.Stimmunterlagen.Core.ObjectStorage;

public interface IEVotingZipStorage
{
    Task<byte[]> Fetch();
}
