// <copyright file="ElementNotNullValidator.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Collections;
    using System.Linq;
    using Microsoft.Practices.EnterpriseLibrary.Validation;
    using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

    /// <summary>
    /// This class checks all elements in the collection are not null.
    /// </summary>
    internal class ElementNotNullValidator : ValueValidator
    {
        internal ElementNotNullValidator(string messageTemplate, string tag, bool negated)
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
            get { return "Element null found in collection."; }
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
            if (objectToValidate == null)
            {
                return;
            }

            IEnumerable collection = objectToValidate as IEnumerable;

            if (collection != null)
            {
                // cannot contains null entry
                var nullNumber = collection.Cast<object>().Count(x => x == null);

                if (nullNumber > 0)
                {
                    LogValidationResult(validationResults, "Target cannot contain null entry", currentTarget, key);
                    return;
                }
            }
            else
            {
                LogValidationResult(validationResults, "ElementNotNullValidator target is not IEnumerable", currentTarget, key);
            }
        }
    }

    internal sealed class ElementNotNullAttribute : ValueValidatorAttribute
    {
        protected override Validator DoCreateValidator(Type targetType)
        {
            return new ElementNotNullValidator(this.MessageTemplate, this.Tag, this.Negated);
        }
    }
}
