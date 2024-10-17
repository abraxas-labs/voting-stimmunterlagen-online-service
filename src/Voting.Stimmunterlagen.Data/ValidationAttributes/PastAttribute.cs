// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Common;

namespace Voting.Stimmunterlagen.Data.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class PastAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        var clock = validationContext.GetRequiredService<IClock>();
        if (value is DateTime dt && dt.ToUniversalTime() < clock.UtcNow)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"{validationContext.DisplayName} needs to be before {clock.UtcNow}");
    }
}
