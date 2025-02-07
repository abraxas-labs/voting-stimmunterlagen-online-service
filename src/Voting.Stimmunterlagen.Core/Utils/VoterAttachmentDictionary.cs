// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Utils;

public class VoterAttachmentDictionary
{
    private static readonly Guid PoliticalAssemblyPoliticalBusinessGuid = Guid.Empty;
    private readonly Dictionary<(Guid PoliticalBusinessId, bool IsHouseholder), HashSet<string>> _attachmentStationCodesByPoliticalBusinessIdAndIsHouseholder;
    private readonly bool _isPoliticalAssembly;

    public VoterAttachmentDictionary(IReadOnlyCollection<Attachment> attachments, bool isPoliticalAssembly)
    {
        _isPoliticalAssembly = isPoliticalAssembly;
        _attachmentStationCodesByPoliticalBusinessIdAndIsHouseholder = GetAttachmentStationsByPoliticalBusinessIdAndIsHouseholder(attachments);
    }

    public string GetAttachmentStations(IReadOnlyCollection<Guid> politicalBusinessIds, bool isHouseholder)
    {
        if (_isPoliticalAssembly)
        {
            politicalBusinessIds = new[] { PoliticalAssemblyPoliticalBusinessGuid };
        }

        var attachmentStationCodes = new HashSet<string>();

        foreach (var pbId in politicalBusinessIds)
        {
            attachmentStationCodes.AddRange(
                _attachmentStationCodesByPoliticalBusinessIdAndIsHouseholder.GetValueOrDefault((pbId, isHouseholder)) ?? new());
        }

        return string.Concat(attachmentStationCodes.OrderBy(x => x));
    }

    private Dictionary<(Guid PoliticalBusinessId, bool IsHouseholder), HashSet<string>> GetAttachmentStationsByPoliticalBusinessIdAndIsHouseholder(IReadOnlyCollection<Attachment> attachments)
    {
        var attachmentsByPbIdAndIsHouseholder = new Dictionary<(Guid, bool), HashSet<string>>();

        var pbEntries = !_isPoliticalAssembly
            ? attachments.SelectMany(a => a.PoliticalBusinessEntries).ToList()
            : attachments.Select(a => new PoliticalBusinessAttachmentEntry { AttachmentId = a.Id, PoliticalBusinessId = PoliticalAssemblyPoliticalBusinessGuid });

        foreach (var attachmentPbEntry in pbEntries)
        {
            var pbId = attachmentPbEntry.PoliticalBusinessId;
            var attachment = attachments.Single(a => a.Id == attachmentPbEntry.AttachmentId);
            var attachmentStationsCode = GetAttachmentStationCode(attachment);
            var attachmentRequired = attachment.DomainOfInfluenceAttachmentCounts!.SingleOrDefault()?.RequiredCount > 0;

            if (!attachmentsByPbIdAndIsHouseholder.ContainsKey((pbId, true)))
            {
                attachmentsByPbIdAndIsHouseholder.Add((pbId, true), new());
                attachmentsByPbIdAndIsHouseholder.Add((pbId, false), new());
            }

            if (!attachmentRequired || string.IsNullOrEmpty(attachmentStationsCode))
            {
                continue;
            }

            attachmentsByPbIdAndIsHouseholder[(pbId, true)].Add(attachmentStationsCode);

            if (!attachment.SendOnlyToHouseholder)
            {
                attachmentsByPbIdAndIsHouseholder[(pbId, false)].Add(attachmentStationsCode);
            }
        }

        return attachmentsByPbIdAndIsHouseholder;
    }

    private string? GetAttachmentStationCode(Attachment attachment)
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
