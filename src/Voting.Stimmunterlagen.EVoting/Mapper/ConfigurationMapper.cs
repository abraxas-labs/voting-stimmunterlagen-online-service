// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Voting.Stimmunterlagen.EVoting.Configuration;
using Voting.Stimmunterlagen.OfflineClient.Shared.ContestConfiguration;
using Contest = Voting.Stimmunterlagen.EVoting.Models.Contest;
using ContestConfiguration = Voting.Stimmunterlagen.OfflineClient.Shared.ContestConfiguration.Configuration;
using DomainOfInfluence = Voting.Stimmunterlagen.EVoting.Models.DomainOfInfluence;
using DomainOfInfluenceVotingCardReturnAddress = Voting.Stimmunterlagen.EVoting.Models.DomainOfInfluenceVotingCardReturnAddress;
using VotingCardShippingFranking = Voting.Stimmunterlagen.Data.Models.VotingCardShippingFranking;

namespace Voting.Stimmunterlagen.EVoting.Mapper;

internal static class ConfigurationMapper
{
    private const string DefaultEVotingDeliveryType = "A";

    internal static ContestConfiguration ToConfiguration(
        this Contest contest,
        List<DomainOfInfluence> testDomainOfInfluences,
        DomainOfInfluence testDomainOfInfluenceDefaults,
        Dictionary<string, EVotingDomainOfInfluenceConfig> eVotingDomainOfInfluenceConfigByBfs,
        List<string> certificates,
        Dictionary<string, List<Value>> bfsETextBlockValuesDictuesDict)
    {
        return new()
        {
            Polldate = ConvertDateTimeToString(contest.Date),
            Printings = new() { contest.ToPrinting(testDomainOfInfluences, testDomainOfInfluenceDefaults, eVotingDomainOfInfluenceConfigByBfs, bfsETextBlockValuesDictuesDict) },
            Certificates = certificates,
        };
    }

    private static Printing ToPrinting(
        this Contest contest,
        List<DomainOfInfluence> testDomainOfInfluences,
        DomainOfInfluence testDomainOfInfluenceDefaults,
        Dictionary<string, EVotingDomainOfInfluenceConfig> eVotingDomainOfInfluenceConfigByBfs,
        Dictionary<string, List<Value>> bfsETextBlockValuesDict)
    {
        var printing = new Printing
        {
            Name = EVotingDefaults.PrintingName,
        };

        printing.Municipalities = testDomainOfInfluences.ConvertAll(testDoi =>
        {
            testDoi.AppendTestDefaults(testDomainOfInfluenceDefaults);
            return testDoi.ToMunicipality(contest.Date, eVotingDomainOfInfluenceConfigByBfs, bfsETextBlockValuesDict);
        });

        if (contest.ContestDomainOfInfluences?.Any() != true)
        {
            return printing;
        }

        printing.Municipalities.AddRange(contest.ContestDomainOfInfluences.Select(doi => doi.ToMunicipality(contest.Date, eVotingDomainOfInfluenceConfigByBfs, bfsETextBlockValuesDict)));

        return printing;
    }

    private static Municipality ToMunicipality(this DomainOfInfluence domainOfInfluence, DateTime contestDate, Dictionary<string, EVotingDomainOfInfluenceConfig> eVotingDomainOfInfluenceConfigByBfs, Dictionary<string, List<Value>> bfsETextBlockValuesDict)
    {
        var eVotingDomainOfInfluenceConfig = eVotingDomainOfInfluenceConfigByBfs.GetValueOrDefault(domainOfInfluence.Bfs);
        var eVotingDomainOfInfluenceValues = GetVotingDomainOfInfluenceValues(domainOfInfluence, bfsETextBlockValuesDict, eVotingDomainOfInfluenceConfig);

        var eTextBlocks = eVotingDomainOfInfluenceValues != null && eVotingDomainOfInfluenceConfig?.ETextBlockColumnQuantity != null
            ? new ETextBlocks
            {
                ColumnQuantity = eVotingDomainOfInfluenceConfig?.ETextBlockColumnQuantity,
                Values = eVotingDomainOfInfluenceValues ?? eVotingDomainOfInfluenceConfig?.ETextBlockValues,
            }
            : null;

        return new()
        {
            Bfs = domainOfInfluence.Bfs,
            Name = domainOfInfluence.Name,
            Logo = domainOfInfluence.Logo,
            DeliveryType = DefaultEVotingDeliveryType,
            ForwardDeliveryType = ConvertShippingFrankingToString(domainOfInfluence.PrintData.ShippingAway),
            ReturnDeliveryType = ConvertShippingFrankingToString(domainOfInfluence.PrintData.ShippingReturn),
            Etemplate = EVotingDefaults.AuslandschweizerBfs.Contains(domainOfInfluence.Bfs) ? EVotingDefaults.ETemplateAuslandschweizer : EVotingDefaults.ETemplate,
            Template = EVotingDefaults.AuslandschweizerBfs.Contains(domainOfInfluence.Bfs) ? EVotingDefaults.TemplateAuslandschweizer : EVotingDefaults.Template,
            PollOpening = ConvertDateTimeToString(contestDate),
            PollClosing = ConvertDateTimeToString(contestDate.AddDays(1)),
            ReturnDeliveryAddress = domainOfInfluence.ReturnAddress.ToDeliveryAddress(),
            ETextBlocks = eTextBlocks,
            Stistat = eVotingDomainOfInfluenceConfig?.Stistat != null ? eVotingDomainOfInfluenceConfig.Stistat.Value : domainOfInfluence.StistatMunicipality,
            AttachmentStations = domainOfInfluence.AttachmentStations,
        };

        static List<Value>? GetVotingDomainOfInfluenceValues(DomainOfInfluence domainOfInfluence, Dictionary<string, List<Value>> bfsETextBlockValuesDict, EVotingDomainOfInfluenceConfig? eVotingDomainOfInfluenceConfig)
        {
            List<Value>? eVotingDomainOfInfluenceValues = null;
            if (!BfsIsSwissAbroad(domainOfInfluence.Bfs))
            {
                // if bfs is from test ballot starting with 8... brick should be mapped from real bfs starting with 3...
                var bfs = domainOfInfluence.Bfs.StartsWith("8")
                    ? "3" + domainOfInfluence.Bfs.Substring(1)
                    : domainOfInfluence.Bfs;

                eVotingDomainOfInfluenceValues = bfsETextBlockValuesDict.GetValueOrDefault(bfs);
            }

            if (eVotingDomainOfInfluenceValues == null && eVotingDomainOfInfluenceConfig != null)
            {
                eVotingDomainOfInfluenceValues = eVotingDomainOfInfluenceConfig.ETextBlockValues;
            }

            return eVotingDomainOfInfluenceValues;
        }
    }

    private static bool BfsIsSwissAbroad(string bfs)
    {
        return bfs == "9170" || bfs == "8170";
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
            AddressAddition = data.AddressAddition,
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
}
