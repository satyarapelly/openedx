// <copyright file="PropertyCollectionValidator.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
    using Microsoft.Practices.EnterpriseLibrary.Validation;

    /// <summary>
    /// This class performs validation for a Property collection.
    /// </summary>
    public class PropertyCollectionValidator : ValueValidator
    {
        internal PropertyCollectionValidator(string messageTemplate, string tag, bool negated)
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
            if (objectToValidate == null)
            {
                return;
            }

            IEnumerable<Property> propertyCollection = objectToValidate as IEnumerable<Property>;

            // cannot contains null entry
            var nullNumber = propertyCollection.Count(x => x == null);
            if (nullNumber > 0)
            {
                LogValidationResult(validationResults, "PropertyCollection could not contain null Property entry", currentTarget, key);
                return;
            }

            if (propertyCollection != null)
            {
                var propertyDup = from property in propertyCollection
                                  group property by new { property.Namespace, property.Name } into g
                                  where g.Count() > 1
                                  select g.Key;
                if (propertyDup.Count() > 0)
                {
                    string message = string.Format("The following Property are duplicate in PropertyBag, Namespace:{0}, Name:{1}",
                        propertyDup.First().Namespace, propertyDup.First().Name);
                    LogValidationResult(validationResults, message, currentTarget, key);
                }
            }
            else
            {
                LogValidationResult(validationResults, "PropertyCollectionValidatorTargetNotPropertyCollection", currentTarget, key);
            }
        }
    }

    /// <summary>
    /// This class is the attribute to enable the inheritance validation.
    /// </summary>
    public sealed class PropertyCollectionValidatorAttribute : ValueValidatorAttribute
    {
        protected override Validator DoCreateValidator(Type targetType)
        {
            return new PropertyCollectionValidator(this.MessageTemplate, this.Tag, this.Negated);
        }
    }
}
