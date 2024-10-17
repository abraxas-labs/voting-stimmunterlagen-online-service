﻿// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile.Converter;
using Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile.Mapping;
using Voting.Stimmunterlagen.Core.Models.VotingCardPrintFile;
using Voting.Stimmunterlagen.Core.Utils;
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
                writer.Context.TypeConverterCache.AddConverter<VotingCardShippingMethod>(new VotingCardShippingMethodConverter());
                writer.Context.TypeConverterCache.AddConverter<VotingCardShippingFranking>(new VotingCardShippingFrankingConverter());
                writer.Context.RegisterClassMap<VotingCardPrintFileEntryMap>();
            });
    }

    internal IEnumerable<VotingCardPrintFileEntry> MapToPrintFileEntries(
        VotingCardGeneratorJob job,
        IReadOnlyCollection<Attachment> attachments,
        Contest contest)
    {
        if (job.Layout!.VotingCardType != VotingCardType.EVoting && job.Layout!.EffectiveTemplate == null)
        {
            throw new ArgumentException($"Template required, but missing on job {job.Id}/{job.LayoutId}");
        }

        var contestOrderNumber = DatamatrixMapping.MapContestOrderNumber(contest.OrderNumber);

        var attachmentStationsByVoterListId = AttachmentStationsBuilder.BuildAttachmentStationsByVoterListId(
            job.Voter.Select(v => v.List).WhereNotNull().ToHashSet(),
            attachments,
            contest.IsPoliticalAssembly);

        var hasC4ShippmentFormat = attachments.Any(x => x.Format == AttachmentFormat.A4);
        var hasA4Format = job.Layout!.VotingCardType == VotingCardType.EVoting || !job.Layout.EffectiveTemplate!.InternName.Contains("_a5");
        var docId = job.DomainOfInfluence!.Contest!.IsPoliticalAssembly
            ? hasC4ShippmentFormat ? DocIdPoliticalAssemblyWithAttachmentFormatA4 : hasA4Format
                ? DocIdPoliticalAssemblyWithAttachmentFormatA4ShippmentC5 : DocIdPoliticalAssemblyWithAttachmentFormatA5
            : hasC4ShippmentFormat ? DocIdContestWithAttachmentFormatA4 : hasA4Format
                ? DocIdContestWithAttachmentFormatA4ShippmentC5 : DocIdContestWithAttachmentFormatA5;
        var form = hasA4Format ? DefaultForm : ThickForm;

        foreach (var voter in job.Voter)
        {
            yield return MapToPrintFileEntry(voter, job.DomainOfInfluence!, attachmentStationsByVoterListId, contestOrderNumber, docId, form);
        }
    }

    private VotingCardPrintFileEntry MapToPrintFileEntry(
        Voter voter,
        ContestDomainOfInfluence domainOfInfluence,
        Dictionary<Guid, string> attachmentStationsByVoterListId,
        string contestOrderNumber,
        string docId,
        string form)
    {
        var zipCode = voter.SwissZipCode?.ToString() ?? voter.ForeignZipCode;
        var pageFrom = voter.PageInfo?.PageFrom ?? 0;
        var pageTo = voter.PageInfo?.PageTo ?? 0;

        return new()
        {
            DocId = docId,
            Bfs = voter.Bfs,
            Form = form,
            PersonId = voter.PersonId,
            FullName = voter.FullName,
            SendSort = string.Empty,
            CustomerSubdivision = string.Empty,
            IsDuplexPrint = false,
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
            ShippingAway = domainOfInfluence.PrintData!.ShippingAway,
            ShippingReturn = domainOfInfluence.PrintData.ShippingReturn,
            ShippingMethod = !voter.SendVotingCardsToDomainOfInfluenceReturnAddress
                ? domainOfInfluence.PrintData.ShippingMethod
                : VotingCardShippingMethod.OnlyPrintingPackagingToMunicipality,
            Language = voter.LanguageOfCorrespondence,
            AdditionalBarcode = string.Empty,
            AttachmentStations = voter.ListId.HasValue
                ? (attachmentStationsByVoterListId.GetValueOrDefault(voter.ListId.Value) ?? string.Empty)
                : string.Empty,
            ContestOrderNumber = contestOrderNumber,
        };
    }
}
