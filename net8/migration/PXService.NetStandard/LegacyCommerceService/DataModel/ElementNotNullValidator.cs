namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Collections;
    using System.ComponentModel.DataAnnotations;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ElementNotNullAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IEnumerable enumerable)
            {
                int index = 0;
                foreach (var element in enumerable)
                {
                    if (element is null)
                    {
                        var memberName = validationContext.MemberName ?? validationContext.DisplayName;
                        return new ValidationResult($"{memberName} contains a null element at index {index}.", new[] { memberName });
                    }
                    index++;
                }
            }
            return ValidationResult.Success;
        }
    }
}
