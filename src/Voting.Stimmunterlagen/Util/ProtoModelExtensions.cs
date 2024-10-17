// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Proto.V1.Requests;

public static class ProtoModelExtensions
{
    public static Guid GetId(this IdValueRequest value) => Guid.Parse(value.Id);
}
