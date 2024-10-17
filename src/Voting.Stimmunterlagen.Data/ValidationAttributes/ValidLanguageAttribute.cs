// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Voting.Lib.Common;

namespace Voting.Stimmunterlagen.Data.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class ValidLanguageAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is null
               || (value is string s && Languages.All.Contains(s));
    }
}
