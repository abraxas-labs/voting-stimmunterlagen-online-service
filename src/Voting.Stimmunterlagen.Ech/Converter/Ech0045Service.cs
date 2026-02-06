// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Ech.Converter;

public class Ech0045Service
{
    private readonly IServiceProvider _sp;

    public Ech0045Service(IServiceProvider sp)
    {
        _sp = sp;
    }

    public byte[] WriteEch0045Xml(
        Ech0045Version version,
        Contest contest,
        VoterList voterList,
        DomainOfInfluenceCanton canton,
        Dictionary<Guid, List<ContestDomainOfInfluence>> doiHierarchyByDoiId)
    {
        var converter = GetConverter(version);
        return converter.WriteEch0045Xml(contest, voterList, canton, doiHierarchyByDoiId);
    }

    public XmlReader GetEch0045Reader(Ech0045Version version, Stream stream)
    {
        var converter = GetConverter(version);
        return converter.GetEch0045Reader(stream);
    }

    public IAsyncEnumerable<Voter> ReadVoters(
        Ech0045Version version,
        XmlReader reader,
        bool shippingVotingCardsToDeliveryAddress,
        bool eVotingEnabled,
        CancellationToken cancellationToken)
    {
        var converter = GetConverter(version);
        return converter.ReadVoters(reader, shippingVotingCardsToDeliveryAddress, eVotingEnabled, cancellationToken);
    }

    public async Task<bool> IsFromElectoralRegister(Ech0045Version version, XmlReader reader, CancellationToken cancellationToken)
    {
        var converter = GetConverter(version);
        return await converter.IsFromElectoralRegister(reader, cancellationToken);
    }

    private IEch0045Converter GetConverter(Ech0045Version version) =>
        _sp.GetKeyedService<IEch0045Converter>(version)
        ?? throw new InvalidOperationException("Cannot resolve eCH-0045 converter for unspecified version");
}
