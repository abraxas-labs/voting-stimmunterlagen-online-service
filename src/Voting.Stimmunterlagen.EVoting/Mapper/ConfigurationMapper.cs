// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Voting.Stimmunterlagen.OfflineClient.Shared.ContestConfiguration;
using Contest = Voting.Stimmunterlagen.EVoting.Models.Contest;
using DomainOfInfluence = Voting.Stimmunterlagen.EVoting.Models.DomainOfInfluence;
using DomainOfInfluenceVotingCardReturnAddress = Voting.Stimmunterlagen.EVoting.Models.DomainOfInfluenceVotingCardReturnAddress;
using VotingCardShippingFranking = Voting.Stimmunterlagen.Data.Models.VotingCardShippingFranking;
using VotingCardShippingMethod = Voting.Stimmunterlagen.Data.Models.VotingCardShippingMethod;

namespace Voting.Stimmunterlagen.EVoting.Mapper;

internal static class ConfigurationMapper
{
    internal static Configuration ToConfiguration(
        this Contest contest,
        List<DomainOfInfluence> testDomainOfInfluences,
        DomainOfInfluence testDomainOfInfluenceDefaults,
        Dictionary<string, ETextBlocks> eTextBlocksByBfs,
        List<string> certificates)
    {
        return new()
        {
            Polldate = ConvertDateTimeToString(contest.Date),
            Printings = new() { contest.ToPrinting(testDomainOfInfluences, testDomainOfInfluenceDefaults, eTextBlocksByBfs) },
            Certificates = certificates,
        };
    }

    private static Printing ToPrinting(this Contest contest, List<DomainOfInfluence> testDomainOfInfluences, DomainOfInfluence testDomainOfInfluenceDefaults, Dictionary<string, ETextBlocks> eTextBlocksByBfs)
    {
        var printing = new Printing
        {
            Name = EVotingDefaults.PrintingName,
        };

        printing.Municipalities = testDomainOfInfluences.ConvertAll(testDoi =>
        {
            testDoi.AppendTestDefaults(testDomainOfInfluenceDefaults);
            return testDoi.ToMunicipality(contest.Date, eTextBlocksByBfs);
        });

        if (contest.ContestDomainOfInfluences?.Any() != true)
        {
            return printing;
        }

        printing.Municipalities.AddRange(contest.ContestDomainOfInfluences.Select(doi => doi.ToMunicipality(contest.Date, eTextBlocksByBfs)));

        return printing;
    }

    private static Municipality ToMunicipality(this DomainOfInfluence domainOfInfluence, DateTime contestDate, Dictionary<string, ETextBlocks> eTextBlocksByBfs)
    {
        return new()
        {
            Bfs = domainOfInfluence.Bfs,
            Name = domainOfInfluence.Name,
            Logo = domainOfInfluence.Logo,
            DeliveryType = ConvertShippingMethodToString(domainOfInfluence.PrintData.ShippingMethod),
            ForwardDeliveryType = ConvertShippingFrankingToString(domainOfInfluence.PrintData.ShippingAway),
            ReturnDeliveryType = ConvertShippingFrankingToString(domainOfInfluence.PrintData.ShippingReturn),
            Etemplate = EVotingDefaults.AuslandschweizerBfs.Equals(domainOfInfluence.Bfs) ? EVotingDefaults.ETemplateAuslandschweizer : EVotingDefaults.ETemplate,
            Template = EVotingDefaults.AuslandschweizerBfs.Equals(domainOfInfluence.Bfs) ? EVotingDefaults.TemplateAuslandschweizer : EVotingDefaults.Template,
            PollOpening = ConvertDateTimeToString(contestDate),
            PollClosing = ConvertDateTimeToString(contestDate.AddDays(1)),
            ReturnDeliveryAddress = domainOfInfluence.ReturnAddress.ToDeliveryAddress(),
            AttachmentStations = domainOfInfluence.AttachmentStations,
            ETextBlocks = eTextBlocksByBfs.GetValueOrDefault(domainOfInfluence.Bfs),
        };
    }

    private static DeliveryAddress ToDeliveryAddress(this DomainOfInfluenceVotingCardReturnAddress data)
    {
        return new()
        {
            Plz = data.ZipCode,
            Municipality = data.City,
            Street = data.Street,
            AddressField1 = data.AddressLine1,
            AddressField2 = data.AddressLine2,
            Country = data.Country,
        };
    }

    private static void AppendTestDefaults(this DomainOfInfluence domainOfInfluence, DomainOfInfluence testDomainOfInfluenceDefaults)
    {
        domainOfInfluence.ReturnAddress = testDomainOfInfluenceDefaults.ReturnAddress;
        domainOfInfluence.PrintData = testDomainOfInfluenceDefaults.PrintData;
    }

    private static string ConvertDateTimeToString(DateTime dateTime)
    {
        return dateTime.ToString("dd.MM.yyyy HH:mm:ss");
    }

    private static string ConvertShippingFrankingToString(VotingCardShippingFranking franking)
    {
        return franking switch
        {
            VotingCardShippingFranking.GasA => "A",
            VotingCardShippingFranking.GasB => "B",
            VotingCardShippingFranking.WithoutFranking => "F",
            _ => franking.ToString(),
        };
    }

    private static string ConvertShippingMethodToString(VotingCardShippingMethod shippingMethod)
    {
        return shippingMethod switch
        {
            VotingCardShippingMethod.PrintingPackagingShippingToCitizen => "A",
            VotingCardShippingMethod.PrintingPackagingShippingToMunicipality => "B",
            VotingCardShippingMethod.OnlyPrintingPackagingToMunicipality => "C",
            _ => shippingMethod.ToString(),
        };
    }
}
