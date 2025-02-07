// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Stimmunterlagen.Data.Models;
using Voter = Voting.Stimmunterlagen.Data.Models.Voter;

namespace Voting.Stimmunterlagen.Core.Utils;

public class VoterHouseholdBuilder
{
    private readonly Dictionary<Guid, Dictionary<(int, int), VoterHouseholderRecord>> _existingVoterRecordsDictByListId = new();

    public VoterHouseholdBuilder(VoterListImport import)
    {
        foreach (var voterList in import.VoterLists!)
        {
            _existingVoterRecordsDictByListId.Add(voterList.Id, new Dictionary<(int, int), VoterHouseholderRecord>());
        }
    }

    public void NextVoter(Voter voter)
    {
        if (voter.ResidenceBuildingId == null || voter.ResidenceApartmentId == null)
        {
            return;
        }

        var key = (voter.ResidenceBuildingId.Value, voter.ResidenceApartmentId.Value);
        var households = _existingVoterRecordsDictByListId[voter.ListId!.Value];
        if (!voter.SendVotingCardsToDomainOfInfluenceReturnAddress && (voter.IsHouseholder || !households.ContainsKey(key)))
        {
            households[key] = new VoterHouseholderRecord(voter.PersonId, voter.IsHouseholder);
        }
    }

    public Dictionary<Guid, Dictionary<(int ResidenceBuildingId, int ResidenceApartmentId), VoterHouseholderRecord>> GetHouseholdsByListId()
    {
        return _existingVoterRecordsDictByListId;
    }

    public record VoterHouseholderRecord(string PersonId, bool IsHouseholder);
}
