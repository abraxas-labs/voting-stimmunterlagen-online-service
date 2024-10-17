// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace Voting.Stimmunterlagen.Data.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class ValidateObjectAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        var ctx = new ValidationContext(value, validationContext.GetRequiredService<IServiceProvider>(), null);
        var ok = Validator.TryValidateObject(value, ctx, null, true);
        return ok
            ? ValidationResult.Success
            : new ValidationResult($"Validation of {validationContext.DisplayName} failed");
    }
}
