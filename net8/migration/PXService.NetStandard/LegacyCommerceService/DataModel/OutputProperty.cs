// <copyright file="OutputProperty.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Attribute applied to properties that are intended to be output-only.
    /// Validation fails when the property is not null or the default value for its type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class OutputPropertyAttribute : ValidationAttribute
    {
        public OutputPropertyAttribute()
            : base("Output property must be null or default.")
        {
        }

        /// <summary>
        /// Validates that the property value is either null or its type's default value.
        /// </summary>
        /// <param name="value">The value of the member to validate.</param>
        /// <param name="validationContext">Context information about the validation operation.</param>
        /// <returns><see cref="ValidationResult.Success"/> when the value is valid; otherwise a <see cref="ValidationResult"/> with an error message.</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var defaultValue = GetDefaultValue(value.GetType());
            if (value.Equals(defaultValue))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }

        private static object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
