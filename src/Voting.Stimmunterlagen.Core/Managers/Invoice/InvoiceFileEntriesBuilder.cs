// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.Models.Invoice;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Managers.Invoice;

public class InvoiceFileEntriesBuilder
{
    private readonly IReadOnlyCollection<MaterialConfig> _materialConfigs;
    private readonly IReadOnlyCollection<MaterialConfig> _additionalInvoicePositionConfigs;
    private readonly AttachmentCategorySummaryBuilder _attachmentCategorySummaryBuilder;

    public InvoiceFileEntriesBuilder(
        ApiConfig apiConfig,
        AttachmentCategorySummaryBuilder attachmentCategorySummaryBuilder)
    {
        _materialConfigs = apiConfig.Invoice.Materials.Where(m => m.Category != MaterialCategory.AdditionalInvoicePosition).ToList();
        _additionalInvoicePositionConfigs = apiConfig.Invoice.Materials.Where(m => m.Category == MaterialCategory.AdditionalInvoicePosition).ToList();
        _attachmentCategorySummaryBuilder = attachmentCategorySummaryBuilder;
    }

    public IEnumerable<InvoiceFileEntry> BuildEntries(PrintJob printJob, DateTime timestamp)
    {
        var voterLists = printJob.DomainOfInfluence!.VoterLists!.ToList();
        var totalNumberOfVoters = voterLists.Sum(vl => vl.NumberOfVoters);

        if (totalNumberOfVoters == 0)
        {
            yield break;
        }

        var restNumberOfVoters = voterLists
            .Sum(vl => vl.CountOfSendVotingCardsToDomainOfInfluenceReturnAddress);

        var attachmentCounts = printJob.DomainOfInfluence.DomainOfInfluenceAttachmentCounts!;

        var attachmentCategorySummaries = _attachmentCategorySummaryBuilder.Build(
            attachmentCounts.Select(doiAc => doiAc.Attachment!).ToList(),
            voterLists);

        var attachmentRequiredForVoterListsCount = attachmentCategorySummaries.Count != 0
            ? attachmentCategorySummaries.Max(acs => acs.TotalRequiredForVoterListsCount)
            : 0;

        var attachmentStationsCount = attachmentCounts
            .Select(doiAc => doiAc.Attachment!.Station)
            .WhereNotNull()
            .Distinct()
            .Count();

        // if at least one attachment has format A4, the whole delivery for the domain of influence is A4.
        // default is A5 (voting cards are always A4, if the delivery is A5 then the voting card will be folded).
        var deliveryFormat = attachmentCounts.Any(a => a.Attachment!.Format == AttachmentFormat.A4)
            ? AttachmentFormat.A4
            : AttachmentFormat.A5;

        foreach (var materialConfig in _materialConfigs)
        {
            var entry = BuildInvoiceEntry(
                printJob.DomainOfInfluence,
                timestamp,
                materialConfig,
                attachmentStationsCount,
                totalNumberOfVoters,
                restNumberOfVoters,
                attachmentRequiredForVoterListsCount,
                deliveryFormat);

            if (entry != null)
            {
                yield return entry;
            }
        }

        foreach (var additionalInvoicePosition in printJob.DomainOfInfluence!.AdditionalInvoicePositions!)
        {
            // material config can be null if used with old data
            // (because additional invoice position description got migrated to material number)
            var additionalInvoicePositionConfig = _additionalInvoicePositionConfigs
                .FirstOrDefault(c => c.Number == additionalInvoicePosition.MaterialNumber);

            yield return BuildInvoiceEntry(printJob.DomainOfInfluence!, timestamp, additionalInvoicePositionConfig, additionalInvoicePosition);
        }
    }

    private InvoiceFileEntry? BuildInvoiceEntry(
        ContestDomainOfInfluence domainOfInfluence,
        DateTime timestamp,
        MaterialConfig materialConfig,
        int attachmentStationsCount,
        int totalNumberOfVoters,
        int restNumberOfVoters,
        int attachmentRequiredForVoterListsCount,
        AttachmentFormat deliveryFormat)
    {
        if (materialConfig.AttachmentFormat.HasValue && materialConfig.AttachmentFormat != deliveryFormat)
        {
            return null;
        }

        var entry = new InvoiceFileEntry()
        {
            CurrentDate = timestamp,
            SapCustomerNumber = domainOfInfluence.SapCustomerOrderNumber,
            SapMaterialNumber = materialConfig.Number,
            SapMaterialText = materialConfig.Description,
            SapOrderPosition = materialConfig.OrderPosition,
        };

        switch (materialConfig.Category)
        {
            case MaterialCategory.Flatrate:
                entry.Amount = 1;
                return entry;
            case MaterialCategory.Voter:
                entry.Amount = totalNumberOfVoters;
                break;
            case MaterialCategory.VoterExcludeRest:
                entry.Amount = totalNumberOfVoters - restNumberOfVoters;
                break;
            case MaterialCategory.AttachmentStationsSetup:
                entry.Amount = attachmentStationsCount;
                break;
            case MaterialCategory.AttachmentStations:
                if (!materialConfig.Stations.HasValue)
                {
                    throw new InvalidOperationException($"Material configuration '{materialConfig.Number}' has no stations information assigned");
                }

                if (materialConfig.Stations != attachmentStationsCount)
                {
                    return null;
                }

                entry.Amount = attachmentRequiredForVoterListsCount;
                break;
            default:
                throw new InvalidOperationException($"Invoice material configuration with number '{materialConfig.Number}' has an invalid category: '{materialConfig.Category}'");
        }

        return entry.Amount == 0 ? null : entry;
    }

    private InvoiceFileEntry BuildInvoiceEntry(
        ContestDomainOfInfluence domainOfInfluence,
        DateTime timestamp,
        MaterialConfig? additionalInvoicePositionConfig,
        AdditionalInvoicePosition additionalInvoicePosition)
    {
        return new()
        {
            CurrentDate = timestamp,
            Amount = additionalInvoicePosition.Amount,
            SapCustomerNumber = domainOfInfluence.SapCustomerOrderNumber,
            SapMaterialNumber = additionalInvoicePositionConfig?.Number ?? string.Empty,
            SapMaterialText = additionalInvoicePositionConfig?.Description ?? additionalInvoicePosition.MaterialNumber,
            SapOrderPosition = additionalInvoicePositionConfig?.OrderPosition ?? string.Empty,
        };
    }
}
