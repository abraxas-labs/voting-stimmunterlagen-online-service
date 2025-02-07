// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Voting.Stimmunterlagen.Core.Models.TemplateData;
using Voting.Stimmunterlagen.Core.ObjectStorage;
using Voting.Stimmunterlagen.Data.Models;
using Contest = Voting.Stimmunterlagen.Data.Models.Contest;
using DomainOfInfluence = Voting.Stimmunterlagen.Core.Models.TemplateData.DomainOfInfluence;
using Voter = Voting.Stimmunterlagen.Core.Models.TemplateData.Voter;
using VotingCardColor = Voting.Stimmunterlagen.Core.Models.TemplateData.VotingCardColor;

namespace Voting.Stimmunterlagen.Core.Managers.Templates;

public class TemplateDataBuilder
{
    private static readonly HashSet<string> ProvidedContainerNames = new()
    {
        TemplateBag.ContestContainerName,
        TemplateBag.VoterContainerName,
        TemplateBag.DomainOfInfluenceContainerName,
        TemplateBag.JobContainerName,
    };

    private static readonly IReadOnlyCollection<Voter> DummyVoter = new List<Voter>()
    {
        new()
        {
            Salutation = Salutation.Mister,
            FirstName = "Sven",
            LastName = "Muster",
            Street = "Rathausplatz",
            HouseNumber = "12a",
            Town = "St. Gallen",
            SwissZipCode = 9000,
            Country =
            {
                Iso2 = "CH",
                Name = "Schweiz",
            },
            PersonId = "00123456789",
            DateOfBirth = "2001-01-01",
            ShipmentNumber = "123456789",
        },
    };

    private readonly IMapper _mapper;
    private readonly DomainOfInfluenceLogoStorage _logoStorage;

    public TemplateDataBuilder(IMapper mapper, DomainOfInfluenceLogoStorage logoStorage)
    {
        _mapper = mapper;
        _logoStorage = logoStorage;
    }

    internal ContestDomainOfInfluence GetDummyDomainOfInfluence(string tennantId)
    {
        return new ContestDomainOfInfluence()
        {
            Name = "Test-Gemeinde XY",
            ShortName = "XY",
            SecureConnectId = tennantId,
            ReturnAddress = new() { AddressLine1 = "Gemeindeverwaltung XY", AddressLine2 = "Adresszeile 2", Street = "Strasse 99", AddressAddition = "Adress Zusatz", ZipCode = "9999", City = "XY", Country = "SWITZERLAND" },
            PrintData = new() { ShippingAway = VotingCardShippingFranking.A, ShippingReturn = VotingCardShippingFranking.GasA, ShippingMethod = VotingCardShippingMethod.PrintingPackagingShippingToCitizen, ShippingVotingCardsToDeliveryAddress = false },
            SwissPostData = new() { InvoiceReferenceNumber = "000000000", FrankingLicenceReturnNumber = "000000000" },
            LogoRef = string.Empty,
        };
    }

    internal IEnumerable<TemplateDataFieldValue> BuildUserEnteredValues(
        IEnumerable<TemplateDataContainer> containers,
        IEnumerable<TemplateDataFieldValue>? existingValues)
    {
        var existingValuesDict = existingValues?.ToDictionary(x => (x.Field!.Container!.Key, x.Field!.Key), x => x.Value);
        return containers
            .Where(x => !ProvidedContainerNames.Contains(x.Key))
            .SelectMany(c => c.Fields!
                .Where(x => x.Active)
                .Select(f => new TemplateDataFieldValue
                {
                    FieldId = f.Id,
                    Value = existingValuesDict?.GetValueOrDefault((c.Key, f.Key), string.Empty) ?? string.Empty,
                }));
    }

    internal Task<TemplateBagWrapper> BuildBag(
        DateTime? contestDate,
        Contest contest,
        ContestDomainOfInfluence? domainOfInfluence,
        IEnumerable<Data.Models.Voter>? voters,
        IEnumerable<TemplateDataContainer> containers)
    {
        var values = containers.Where(x => !ProvidedContainerNames.Contains(x.Key))
            .ToDictionary(
                x => x.Key,
                x => (object)x.Fields!.ToDictionary(f => f.Key, f => f.Name));
        return BuildBag(contestDate, contest, domainOfInfluence, voters, values);
    }

    internal Task<TemplateBagWrapper> BuildBag(
        DateTime? contestDate,
        Contest contest,
        ContestDomainOfInfluence? domainOfInfluence,
        IEnumerable<Data.Models.Voter>? voters,
        IEnumerable<TemplateDataFieldValue> values)
    {
        var templateValues = values
            .GroupBy(x => x.Field!.Container!.Key)
            .ToDictionary(
                x => x.Key,
                x => (object)x.ToDictionary(y => y.Field!.Key, y => y.Value));

        return BuildBag(contestDate, contest, domainOfInfluence, voters, templateValues);
    }

    private async Task<TemplateBagWrapper> BuildBag(
        DateTime? contestDate, // set to null if preview for template selection
        Contest contest,
        ContestDomainOfInfluence? domainOfInfluence,
        IEnumerable<Data.Models.Voter>? voters,
        IDictionary<string, object> values)
    {
        DomainOfInfluence? templateDoi = null;
        if (domainOfInfluence != null)
        {
            templateDoi = _mapper.Map<DomainOfInfluence>(domainOfInfluence);
            templateDoi.Logo = await _logoStorage.TryFetchAsBase64(domainOfInfluence);
            templateDoi.VotingCardColor = BuildVotingCardColor(domainOfInfluence);
        }

        var templateContest = _mapper.Map<Models.TemplateData.Contest>(contest);
        var templateVoters = voters == null ? DummyVoter : _mapper.Map<List<Voter>>(voters);
        return new TemplateBagWrapper(new TemplateBag(
            contestDate.HasValue
                ? new JobData { BricksVersion = contestDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") }
                : null,
            templateContest,
            templateDoi,
            templateVoters,
            values));
    }

    private VotingCardColor? BuildVotingCardColor(ContestDomainOfInfluence doi)
    {
        var color = doi.VotingCardColor switch
        {
            Data.Models.VotingCardColor.Blue => new VotingCardColor(20, 0, 0, 0),
            Data.Models.VotingCardColor.Chamois => new VotingCardColor(0, 5, 20, 0),
            Data.Models.VotingCardColor.Yellow => new VotingCardColor(0, 0, 20, 0),
            Data.Models.VotingCardColor.Gold => new VotingCardColor(0, 6, 60, 5),
            Data.Models.VotingCardColor.Grey => new VotingCardColor(0, 0, 0, 10),
            Data.Models.VotingCardColor.Pink => new VotingCardColor(0, 20, 0, 0),
            Data.Models.VotingCardColor.Red => new VotingCardColor(0, 21, 24, 0),
            Data.Models.VotingCardColor.Green => new VotingCardColor(15, 0, 15, 0),
            Data.Models.VotingCardColor.Unspecified => null,
            _ => throw new InvalidOperationException($"{doi.VotingCardColor} is not supported"),
        };

        if (color != null)
        {
            color.Name = doi.VotingCardColor.ToString();
        }

        return color;
    }
}
