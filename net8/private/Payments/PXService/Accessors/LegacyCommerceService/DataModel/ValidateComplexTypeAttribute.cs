using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// A minimal replacement for the ASP.NET Core ValidateComplexTypeAttribute.
    /// Validates the object graph of the decorated property using DataAnnotations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    internal sealed class ValidateComplexTypeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }

            var results = new List<ValidationResult>();
            Validator.TryValidateObject(value, new ValidationContext(value), results, validateAllProperties: true);
            return results.Count == 0 ? ValidationResult.Success : new ValidationResult(validationContext.DisplayName + " is invalid");
        }
    }
}

