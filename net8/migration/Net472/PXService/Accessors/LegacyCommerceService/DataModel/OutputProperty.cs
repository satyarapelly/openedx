// <copyright file="OutputProperty.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
    using Microsoft.Practices.EnterpriseLibrary.Validation;

    /// <summary>
    /// This class performs validation for an output property, ensure it is null.
    /// </summary>
    internal class OutputPropertyValidator : ValueValidator
    {
        internal OutputPropertyValidator(string messageTemplate, string tag, bool negated)
            : base(messageTemplate, tag, negated)
        {
            if (negated)
            {
                throw new ArgumentException("Negation is not supported.");
            }
        }

        protected override string DefaultNegatedMessageTemplate
        {
            get { return String.Empty; }
        }

        protected override string DefaultNonNegatedMessageTemplate
        {
            get { return "Object is not valid."; }
        }

        /// <summary>
        /// Does the action validation
        /// </summary>
        /// <param name="objectToValidate">The actual object to validate</param>
        /// <param name="currentTarget">The root object of the validation tree</param>
        /// <param name="key">The key for the validation</param>
        /// <param name="validationResults">The result collection where to put validation results.</param>
        public override void DoValidate(object objectToValidate, object currentTarget, string key, ValidationResults validationResults)
        {
            if (objectToValidate != null)
            {
                var defaultValue = GetDefaultValue(objectToValidate.GetType());
                if (!objectToValidate.Equals(defaultValue))
                {
                    LogValidationResult(validationResults,
                        "OutputPropertyValidator value is not null or default. Valid default: " + defaultValue,
                        currentTarget, key);
                }
            }
        }

        private static object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }

    /// <summary>
    /// This class is the attribute to enable the inheritance validation.
    /// </summary>
    internal sealed class OutputPropertyAttribute : ValueValidatorAttribute
    {
        protected override Validator DoCreateValidator(Type targetType)
        {
            return new OutputPropertyValidator(this.MessageTemplate, this.Tag, this.Negated);
        }
    }
}
