// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile.Converter;
using Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile.Mapping;
using Voting.Stimmunterlagen.Core.Models.VotingCardPrintFile;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Extensions;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile;

public class VotingCardPrintFileBuilder
{
    private const string DefaultForm = "BL100";
    private const string ThickForm = "BL200";
    private const string DocIdContestWithAttachmentFormatA4 = "U4";
    private const string DocIdContestWithAttachmentFormatA4ShippmentC5 = "U5";
    private const string DocIdContestWithAttachmentFormatA5 = "U5A5";
    private const string DocIdPoliticalAssemblyWithAttachmentFormatA4 = "V4";
    private const string DocIdPoliticalAssemblyWithAttachmentFormatA4ShippmentC5 = "V5";
    private const string DocIdPoliticalAssemblyWithAttachmentFormatA5 = "V5A5";
    private const string DocIdContestWithAttchmentFormatA4LayoutA5 = "U4A5";
    private static readonly VotingCardShippingMethod DefaultEVotingShippingMethod = VotingCardShippingMethod.PrintingPackagingShippingToCitizen;

    private readonly CsvService _csvService;

    public VotingCardPrintFileBuilder(CsvService csvService)
    {
        _csvService = csvService;
    }

    public async Task<byte[]> BuildPrintFile(VotingCardGeneratorJob job, IReadOnlyCollection<Attachment> attachments)
    {
        var entries = MapToPrintFileEntries(job, attachments, job.DomainOfInfluence!.Contest!);

        return await _csvService.Render(
            entries,
            writer =>
            {
                writer.Context.TypeConverterCache.AddConverter<VotingCardShippingFranking>(new VotingCardShippingFrankingConverter());
                writer.Context.RegisterClassMap<VotingCardPrintFileEntryMap>();
            });
    }

    internal IEnumerable<VotingCardPrintFileEntry> MapToPrintFileEntries(
        VotingCardGeneratorJob job,
        IReadOnlyCollection<Attachment> attachments,
        Contest contest)
    {
        var isEVotingJob = job.State == VotingCardGeneratorJobState.ReadyToRunOffline;

        if (!isEVotingJob && job.Layout!.EffectiveTemplate == null)
        {
            throw new ArgumentException($"Template required, but missing on job {job.Id}/{job.LayoutId}");
        }

        var contestOrderNumber = DatamatrixMapping.MapContestOrderNumber(contest.OrderNumber);
        var voterAttachmentDictionary = new VoterAttachmentDictionary(attachments, contest.IsPoliticalAssembly);

        var hasC4ShippmentFormat = attachments.Any(x => x.Format == AttachmentFormat.A4);
        var hasA4Format = isEVotingJob || !job.Layout!.IsA5Template();
        var isDuplex = !isEVotingJob && job.Layout!.IsDuplexTemplate();
        string docId = GetDocId(contest.IsPoliticalAssembly, hasC4ShippmentFormat, hasA4Format);
        var form = hasA4Format ? DefaultForm : ThickForm;
        var customerSubdivision = job.DomainOfInfluence!.ShortName + job.DomainOfInfluence.Type.ToString().ToUpper();

        foreach (var voter in job.Voter)
        {
            yield return MapToPrintFileEntry(
                voter,
                job.DomainOfInfluence!.VoterLists!,
                voterAttachmentDictionary,
                new PrintJobContext { ContestOrderNumber = contestOrderNumber, CustomerSubdivision = customerSubdivision, DocId = docId, Form = form, IsDuplex = isDuplex, IsEVotingJob = isEVotingJob, VotingCardLayout = job.Layout, });
        }
    }

    private static string GetDocId(bool isPoliticalAssembly, bool hasC4ShippmentFormat, bool hasA4Format)
    {
        string docId;

        if (isPoliticalAssembly)
        {
            if (hasC4ShippmentFormat)
            {
                docId = hasA4Format
                    ? DocIdPoliticalAssemblyWithAttachmentFormatA4
                    : DocIdPoliticalAssemblyWithAttachmentFormatA4ShippmentC5;
            }
            else
            {
                docId = hasA4Format
                    ? DocIdPoliticalAssemblyWithAttachmentFormatA4ShippmentC5
                    : DocIdPoliticalAssemblyWithAttachmentFormatA5;
            }
        }
        else
        {
            if (hasC4ShippmentFormat)
            {
                docId = hasA4Format
                    ? DocIdContestWithAttachmentFormatA4
                    : DocIdContestWithAttchmentFormatA4LayoutA5;
            }
            else
            {
                docId = hasA4Format
                    ? DocIdContestWithAttachmentFormatA4ShippmentC5
                    : DocIdContestWithAttachmentFormatA5;
            }
        }

        return docId;
    }

    private VotingCardPrintFileEntry MapToPrintFileEntry(
        Voter voter,
        ICollection<VoterList> voterLists,
        VoterAttachmentDictionary voterAttachmentDictionary,
        PrintJobContext printjob)
    {
        var zipCode = voter.SwissZipCode?.ToString() ?? voter.ForeignZipCode;
        var pageFrom = voter.PageInfo?.PageFrom ?? 0;
        var pageTo = voter.PageInfo?.PageTo ?? 0;
        var religionCode = DatamatrixMapping.MapReligion(voter.Religion, voter.IsMinor, voter.VoterType);

        // A voter can be related to multiple voter lists if he has duplicate entries.
        // Such a voter should receive just 1 voting card but all attachments of the related voter lists.
        var listIds = new HashSet<Guid> { voter.ListId!.Value };
        if (voter.VoterDuplicateId.HasValue)
        {
            if (voter.VoterDuplicate == null || voter.VoterDuplicate.Voters == null || voter.VoterDuplicate.Voters.Count < 2)
            {
                throw new InvalidOperationException($"Invalid voter duplicate {voter.VoterDuplicateId} provided");
            }

            listIds.AddRange(voter.VoterDuplicate.Voters!.Select(v => v.ListId!.Value));
        }

        var pbIds = voterLists
            .Where(vl => listIds.Contains(vl.Id))
            .SelectMany(vl => vl.PoliticalBusinessEntries!.Select(x => x.PoliticalBusinessId))
            .ToHashSet();

        return new()
        {
            DocId = printjob.DocId,
            Bfs = voter.Bfs,
            Form = printjob.Form,
            PersonId = voter.PersonId,
            FullName = voter.FullName,
            SendSort = string.Empty,
            CustomerSubdivision = printjob.CustomerSubdivision,
            IsDuplexPrint = printjob.IsDuplex,
            TotalPages = voter.PageInfo != null ? pageTo - pageFrom + 1 : 0,
            PageFrom = pageFrom,
            PageTo = pageTo,
            PrintPP = true,
            InStreamAttachment1 = string.Empty,
            InStreamAttachment2 = string.Empty,
            InStreamAttachment3 = string.Empty,
            AddressLine1 = voter.Sex == SexType.Male ? "Herr" : voter.Sex == SexType.Female ? "Frau" : string.Empty,
            AddressLine2 = voter.FullName,
            AddressLine3 = voter.AddressLine1 ?? string.Empty,
            AddressLine4 = voter.Street,
            AddressLine5 = (zipCode == null ? string.Empty : zipCode + " ") + voter.Town,
            AddressLine6 = voter.Country.Name,
            CountryIso2 = voter.Country.Iso2 ?? string.Empty,
            ShippingAway = printjob.VotingCardLayout?.PrintData?.ShippingAway ?? VotingCardShippingFranking.Unspecified,
            ShippingReturn = printjob.VotingCardLayout?.PrintData?.ShippingReturn ?? VotingCardShippingFranking.Unspecified,
            ShippingMethodCode = GetShippingMethodCode(printjob.IsEVotingJob, voter.SendVotingCardsToDomainOfInfluenceReturnAddress, printjob.VotingCardLayout?.PrintData?.ShippingMethod ?? VotingCardShippingMethod.Unspecified),
            Language = voter.LanguageOfCorrespondence,
            AdditionalBarcode = string.Empty,
            AttachmentStations = voter.ListId.HasValue
                ? voterAttachmentDictionary.GetAttachmentStations(pbIds, voter.IsHouseholder)
                : string.Empty,
            ContestOrderNumber = printjob.ContestOrderNumber,
            ReligionCode = religionCode,
        };
    }

    private string GetShippingMethodCode(
        bool isEVotingVotingCard,
        bool sendVotingCardsToDomainOfInfluenceReturnAddress,
        VotingCardShippingMethod shippingMethod)
    {
        var mappedShippingMethod = isEVotingVotingCard
            ? DefaultEVotingShippingMethod
            : !sendVotingCardsToDomainOfInfluenceReturnAddress
            ? shippingMethod
            : VotingCardShippingMethod.OnlyPrintingPackagingToMunicipality;

        return mappedShippingMethod switch
        {
            VotingCardShippingMethod.PrintingPackagingShippingToCitizen => "A",
            VotingCardShippingMethod.PrintingPackagingShippingToMunicipality => "B",
            VotingCardShippingMethod.OnlyPrintingPackagingToMunicipality =>
                !sendVotingCardsToDomainOfInfluenceReturnAddress ? "C" : "D",
            _ => throw new InvalidOperationException("Invalid shipping method"),
        };
    }

    private class PrintJobContext
    {
        public string ContestOrderNumber { get; init; } = string.Empty;

        public string CustomerSubdivision { get; init; } = string.Empty;

        public string DocId { get; init; } = string.Empty;

        public string Form { get; init; } = string.Empty;

        public bool IsDuplex { get; init; }

        public bool IsEVotingJob { get; init; }

        public DomainOfInfluenceVotingCardLayout? VotingCardLayout { get; init; }
    }
}
