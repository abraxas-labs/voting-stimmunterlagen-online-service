// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Ech0155_5_1;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Ech.Mapping.V6;

internal static class CountingCircleMapping
{
    public static CountingCircleType ToEchCountingCircle(this ContestCountingCircle cc)
    {
        return new CountingCircleType
        {
            CountingCircleId = cc.BasisCountingCircleId.ToString(),
            CountingCircleName = cc.Name,
        };
    }
}
