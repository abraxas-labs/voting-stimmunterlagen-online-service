// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Ech.Converter;

public interface IEch0045Converter
{
    byte[] WriteEch0045Xml(
        Contest contest,
        VoterList voterList,
        DomainOfInfluenceCanton canton,
        Dictionary<Guid, List<ContestDomainOfInfluence>> doiHierarchyByDoiId);

    XmlReader GetEch0045Reader(Stream stream);

    IAsyncEnumerable<Voter> ReadVoters(XmlReader reader, bool shippingVotingCardsToDeliveryAddress, bool eVotingEnabled, CancellationToken cancellationToken);

    Task<bool> IsFromElectoralRegister(XmlReader reader, CancellationToken cancellationToken);
}
