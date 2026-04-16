// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Ech.Converter;

namespace Voting.Stimmunterlagen.Ech;

public class Ech0045Reader(XmlReader xmlReader, IEch0045Converter converter) : IDisposable
{
    public IAsyncEnumerable<Voter> ReadVoters(
        bool shippingVotingCardsToDeliveryAddress,
        bool eVotingEnabled,
        CancellationToken cancellationToken)
    {
        return converter.ReadVoters(xmlReader, shippingVotingCardsToDeliveryAddress, eVotingEnabled, cancellationToken);
    }

    public async Task<bool> IsFromAutoSendVotingCardsToDomainOfInfluenceReturnAddressSplitApp(CancellationToken cancellationToken)
    {
        return await converter.IsFromAutoSendVotingCardsToDomainOfInfluenceReturnAddressSplitApp(xmlReader, cancellationToken);
    }

    public void Dispose()
    {
        xmlReader.Dispose();
    }
}
