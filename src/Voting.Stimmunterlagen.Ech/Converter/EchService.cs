// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Ech0045_4_0;
using Voting.Lib.Ech;
using Voting.Lib.Ech.Ech0045_4_0.Converter;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Ech.Mapping;

namespace Voting.Stimmunterlagen.Ech.Converter;

public class EchService
{
    private const string ElectoralRegisterManufacturer = "Abraxas Informatik AG";
    private const string ElectoralRegisterProduct = "Voting.Stimmregister";

    private readonly DeliveryHeaderProvider _deliveryHeaderProvider;
    private readonly Ech0045Serializer _ech0045Serializer;
    private readonly Ech0045Deserializer _ech0045Deserializer;

    public EchService(DeliveryHeaderProvider deliveryHeaderProvider, Ech0045Serializer ech0045Serializer, Ech0045Deserializer ech0045Deserializer)
    {
        _deliveryHeaderProvider = deliveryHeaderProvider;
        _ech0045Serializer = ech0045Serializer;
        _ech0045Deserializer = ech0045Deserializer;
    }

    public VoterDelivery ToDelivery(
        Contest contest,
        VoterList voterList,
        DomainOfInfluenceCanton canton,
        Dictionary<Guid, List<ContestDomainOfInfluence>> doiHierarchyByDoiId)
    {
        return new VoterDelivery
        {
            DeliveryHeader = _deliveryHeaderProvider.BuildHeader(!contest.TestingPhaseEnded),
            VoterList = voterList.ToEchVoterList(contest, canton, doiHierarchyByDoiId),
        };
    }

    public byte[] WriteEch0045Xml(VoterDelivery delivery)
    {
        return _ech0045Serializer.ToXmlBytes(delivery);
    }

    public XmlReader GetEch0045Reader(Stream stream)
    {
        return _ech0045Deserializer.BuildReader(stream);
    }

    public async IAsyncEnumerable<Voter> ReadVoters(XmlReader reader, bool shippingVotingCardsToDeliveryAddress, bool eVotingEnabled, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var (i, votingPersonType) in _ech0045Deserializer.ReadVoters(reader, cancellationToken))
        {
            yield return votingPersonType.ToVoter(i, shippingVotingCardsToDeliveryAddress, eVotingEnabled);
        }
    }

    public async Task<bool> IsFromElectoralRegister(XmlReader reader, CancellationToken cancellationToken)
    {
        var deliveryHeader = await _ech0045Deserializer.ReadDeliveryHeader(reader, cancellationToken);
        if (deliveryHeader == null || deliveryHeader.SendingApplication == null)
        {
            throw new ArgumentException("Ech0045 does not provide a delivery header with a sending application");
        }

        return deliveryHeader.SendingApplication.Manufacturer.Equals(ElectoralRegisterManufacturer, StringComparison.Ordinal)
            && deliveryHeader.SendingApplication.Product.Equals(ElectoralRegisterProduct, StringComparison.Ordinal);
    }
}
