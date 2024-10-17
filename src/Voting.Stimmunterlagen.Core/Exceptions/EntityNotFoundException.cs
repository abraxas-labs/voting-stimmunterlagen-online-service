// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Core.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string type, object id)
        : base($"{type} with id {id} not found")
    {
    }
}
