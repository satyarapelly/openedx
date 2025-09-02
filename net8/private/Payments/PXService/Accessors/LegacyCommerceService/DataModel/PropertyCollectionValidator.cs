namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PropertyCollectionValidatorAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IEnumerable enumerable)
            {
                int index = 0;
                foreach (var item in enumerable)
                {
                    if (item is null)
                    {
                        var memberName = validationContext.MemberName ?? validationContext.DisplayName;
                        return new ValidationResult($"{memberName} contains a null element at index {index}.", new[] { memberName });
                    }

                    var context = new ValidationContext(item);
                    var results = new List<ValidationResult>();
                    if (!Validator.TryValidateObject(item, context, results, true))
                    {
                        var first = results.First();
                        string? memberName = validationContext.MemberName != null ? $"{validationContext.MemberName}[{index}]" : null;
                        return new ValidationResult(first.ErrorMessage, new[] { memberName });
                    }
                    index++;
                }
            }
            return ValidationResult.Success;
        }
    }
}
