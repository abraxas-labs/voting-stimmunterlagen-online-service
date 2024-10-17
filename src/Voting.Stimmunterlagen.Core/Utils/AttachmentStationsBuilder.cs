// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Utils;

public static class AttachmentStationsBuilder
{
    public static Dictionary<Guid, string> BuildAttachmentStationsByVoterListId(IReadOnlyCollection<VoterList> voterLists, IReadOnlyCollection<Attachment> attachments, bool isPoliticalAssembly)
    {
        return voterLists.ToDictionary(
            vl => vl.Id,
            vl =>
            {
                var pbIds = vl.PoliticalBusinessEntries!.Select(e => e.PoliticalBusinessId).ToList();
                var relatedAttachments = attachments
                    .Where(a => a.DomainOfInfluenceAttachmentCounts!.SingleOrDefault()?.RequiredCount > 0
                        && (a.State == AttachmentState.Defined || a.State == AttachmentState.Delivered)
                        && (isPoliticalAssembly || a.PoliticalBusinessEntries.Any(pbe => pbIds.Contains(pbe.PoliticalBusinessId))))
                    .OrderBy(a => a.Station)
                    .ToList();
                return GetAttachmentStations(relatedAttachments);
            });
    }

    public static Dictionary<Guid, string> BuildAttachmentStationsByDomainOfInfluenceId(IReadOnlyCollection<VoterList> voterLists, IReadOnlyCollection<Attachment> attachments, bool isPoliticalAssembly)
    {
        var voterListsByDoiId = voterLists
            .GroupBy(vl => vl.DomainOfInfluenceId)
            .ToDictionary(x => x.Key, x => x.ToList());

        return voterListsByDoiId.ToDictionary(
            e => e.Key,
            e =>
            {
                var pbIds = e.Value.SelectMany(vl => vl.PoliticalBusinessEntries!.Select(e => e.PoliticalBusinessId)).ToHashSet();
                var relatedAttachments = attachments
                    .Where(a => a.DomainOfInfluenceAttachmentCounts!.Any(doiAc => doiAc.DomainOfInfluenceId == e.Key && doiAc.RequiredCount > 0)
                        && (a.State == AttachmentState.Defined || a.State == AttachmentState.Delivered)
                        && (isPoliticalAssembly || a.PoliticalBusinessEntries.Any(pbe => pbIds.Contains(pbe.PoliticalBusinessId))))
                    .OrderBy(a => a.Station)
                    .ToList();
                return GetAttachmentStations(relatedAttachments);
            });
    }

    private static string GetAttachmentStations(List<Attachment> attachments)
    {
        var attachmentStationCodes = attachments.ConvertAll(GetAttachmentStationCode)
            .WhereNotNull()
            .ToHashSet();

        return string.Concat(attachmentStationCodes);
    }

    private static string? GetAttachmentStationCode(Attachment attachment)
    {
        if (attachment.Station == null)
        {
            return null;
        }

        var station = attachment.Station.Value!;
        if (station >= 0 && station <= 12)
        {
            return station.ToString("X");
        }

        return null;
    }
}
