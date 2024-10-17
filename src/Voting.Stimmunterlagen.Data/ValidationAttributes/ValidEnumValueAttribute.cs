// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Voting.Stimmunterlagen.Data.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class ValidEnumValueAttribute : ValidationAttribute
{
    private readonly object[] _forbiddenValues;

    public ValidEnumValueAttribute(params object[] forbiddenValues)
    {
        _forbiddenValues = forbiddenValues;
    }

    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return true;
        }

        return Enum.IsDefined(value.GetType(), value) && !_forbiddenValues.Contains(value);
    }
}
