// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Utils;

public class VoterDuplicatesBuilder
{
    private readonly VoterListImport _import;

    private readonly Dictionary<Guid, Dictionary<string, VoterDuplicateCompareRecord>> _existingVoterRecordsDictByListId = new();

    public VoterDuplicatesBuilder(VoterListImport import)
    {
        _import = import;

        foreach (var voterList in import.VoterLists!)
        {
            _existingVoterRecordsDictByListId.Add(voterList.Id, new Dictionary<string, VoterDuplicateCompareRecord>());
        }
    }

    public void NextVoter(Voter voter)
    {
        var existingVoterRecordsByPersonId = _existingVoterRecordsDictByListId[voter.ListId!.Value];
        var currentVoterRecord = new VoterDuplicateCompareRecord(voter.PersonId, voter.FirstName, voter.LastName, voter.DateOfBirth, voter.Sex);

        if (!existingVoterRecordsByPersonId.ContainsKey(currentVoterRecord.PersonId))
        {
            existingVoterRecordsByPersonId.Add(currentVoterRecord.PersonId, currentVoterRecord);
            return;
        }

        var voterList = _import.VoterLists!.Single(x => x.Id == voter.ListId);
        voterList.HasVoterDuplicates = true;

        // a person id should only be included once in voter duplicates
        if (voterList.VoterDuplicates!.Any(d => d.PersonId == currentVoterRecord.PersonId))
        {
            return;
        }

        voterList.VoterDuplicates!.Add(MapToVoterDuplicate(currentVoterRecord, voterList.Id));
    }

    private VoterDuplicate MapToVoterDuplicate(VoterDuplicateCompareRecord voterRecord, Guid voterListId)
    {
        return new()
        {
            PersonId = voterRecord.PersonId,
            FirstName = voterRecord.FirstName,
            LastName = voterRecord.LastName,
            DateOfBirth = voterRecord.DateOfBirth,
            Sex = voterRecord.Sex,
            ListId = voterListId,
        };
    }

    private record VoterDuplicateCompareRecord(string PersonId, string FirstName, string LastName, string DateOfBirth, SexType Sex);
}
