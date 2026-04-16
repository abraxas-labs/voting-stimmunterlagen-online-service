// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Voting.Stimmunterlagen.Core.Managers.Stistat;

public interface IStistatFileStore
{
    Task Save(string fileName, Stream content, string messageType, CancellationToken ct);
}
