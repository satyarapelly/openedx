// <copyright file="PIDLTransformationFactory.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;

    public sealed class PIDLTransformationFactory
    {
        /// <summary>
        /// Some phone number transformations break when using the new ToPhoneNumberE164VNext transformation type.
        /// This contains the transformation parameters that should be blocked from accepting the flighted vNext override,
        /// with the following parameter signature: PidlIdentity[country], PidlIdentity[description_type], PidlIdentity[operation], PidlIdentity[type], PropertyName, TransformationTarget
        /// </summary>
        private static readonly HashSet<Tuple<string, string, string, string, string, string>> blockedVNextTransformationParameters = new HashSet<Tuple<string, string, string, string, string, string>>()
        {
            { new Tuple<string, string, string, string, string, string>("de", "data", "add", "mobile_billing_non_sim_details", "msisdn", "forSubmit") },
        };

        public static PidlTransformationResult<string> TransformProperty(PidlTransformationParameter transformationParameter, List<string> exposedFlightFeatures = null)
        {
            if (!IsValidTransformationParameter(transformationParameter))
            {
                throw new PIDLArgumentException("Transformation parameter passed is not valid", Constants.ErrorCodes.PIDLInvalidTransformationParameter);
            }

            Dictionary<string, string> pidlIdentity = transformationParameter.PidlIdentity;
            string inputValue = transformationParameter.Value;

            var propertyDescription = PIDLResourceFactory.Instance.GetPropertyDescriptionByPropertyName(
                transformationParameter.PidlIdentity[Constants.DescriptionIdentityFields.DescriptionType],
                transformationParameter.PidlIdentity[Constants.DescriptionIdentityFields.Type],
                GlobalConstants.Defaults.OperationKey,
                transformationParameter.PidlIdentity[Constants.DescriptionIdentityFields.Country],
                transformationParameter.PropertyName);

            var propertyTransformations = PIDLResourceFactory.Instance.GetPropertyTransformation(
                propertyDescription.PropertyDescriptionId,
                transformationParameter.PidlIdentity[Constants.DescriptionIdentityFields.Country]);

            if (!propertyTransformations.ContainsKey(transformationParameter.TransformationTarget))
            {
                throw new PIDLException(
                    string.Format("No Transformation found for the property : {0}", transformationParameter.PropertyName),
                    Constants.ErrorCodes.PIDLTransformationNotFoundForProperty);
            }

            var transformation = propertyTransformations[transformationParameter.TransformationTarget];
            string transformationType = transformation.UrlTransformationType;

            if (string.Equals(transformation.UrlTransformationType, Constants.TransformationType.ToPhoneNumberE164, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!pidlIdentity.ContainsKey("country"))
                {
                    return new PidlTransformationResult<string>()
                    {
                        Status = PidlExecutionResultStatus.Failed,
                        ErrorCode = Constants.ErrorCodes.PIDLMissingCountryCodeInIdentity,
                        ErrorMessage = "The Pidl Identity should contain a country field for the input pidl"
                    };
                }

                transformationType = DoesTransformationParameterBlockVNext(transformationParameter, exposedFlightFeatures) ? transformationType : Constants.TransformationType.ToPhoneNumberE164VNext;

                string countryCode = pidlIdentity["country"];
                PhoneNumberTransformer phoneTransformer = new PhoneNumberTransformer();
                return phoneTransformer.Transform(transformationType, inputValue, countryCode);
            }
            else if (string.Equals(transformation.UrlTransformationType, Constants.TransformationType.IndiaStateFullNameToInitials, StringComparison.InvariantCultureIgnoreCase))
            {
                return new PidlTransformationResult<string>()
                {
                    Status = PidlExecutionResultStatus.Passed,
                    TransformedValue = Constants.IndiaStateMapping.GetMappingState(inputValue)
                };
            }
            else
            {
                return new PidlTransformationResult<string>()
                {
                    Status = PidlExecutionResultStatus.Failed,
                    ErrorCode = Constants.ErrorCodes.PIDLTransformationTypeNotSupported,
                    ErrorMessage = string.Format("The transformation type {0} is not supported by the factory", transformationType)
                };
            }
        }

        private static bool IsValidTransformationParameter(PidlTransformationParameter transformationParameter)
        {
            if (transformationParameter == null)
            {
                return false;
            }

            if (transformationParameter.PidlIdentity == null
                || string.IsNullOrEmpty(transformationParameter.Value)
                || string.IsNullOrEmpty(transformationParameter.PropertyName)
                || string.IsNullOrEmpty(transformationParameter.TransformationTarget))
            {
                return false;
            }

            // TaxService uses "country_code" as field "Country"'s property name, all other services use "country".
            // This is a workaround to replace the property name in PidlIdentity from "country_code" to "country".
            if (transformationParameter.PidlIdentity.ContainsKey(Constants.DescriptionIdentityFields.CountryCode))
            {
                transformationParameter.PidlIdentity.Add(Constants.DescriptionIdentityFields.Country, transformationParameter.PidlIdentity[Constants.DescriptionIdentityFields.CountryCode]);
                transformationParameter.PidlIdentity.Remove(Constants.DescriptionIdentityFields.CountryCode);
            }

            if (transformationParameter.PidlIdentity.ContainsKey(Constants.DescriptionIdentityFields.DescriptionType)
                && transformationParameter.PidlIdentity.ContainsKey(Constants.DescriptionIdentityFields.Country)
                && transformationParameter.PidlIdentity.ContainsKey(Constants.DescriptionIdentityFields.Type))
            {
                return true;
            }

            return false;
        }

        private static bool DoesTransformationParameterBlockVNext(PidlTransformationParameter transformationParameter, List<string> exposedFlightFeatures)
        {
            bool isBlocked = false;
            if (transformationParameter?.PidlIdentity != null)
            {
                string country = transformationParameter.PidlIdentity.ContainsKey("country") ? transformationParameter.PidlIdentity["country"] : null;
                string description_type = transformationParameter.PidlIdentity.ContainsKey("description_type") ? transformationParameter.PidlIdentity["description_type"] : null;
                string operation = transformationParameter.PidlIdentity.ContainsKey("operation") ? transformationParameter.PidlIdentity["operation"] : null;
                string type = transformationParameter.PidlIdentity.ContainsKey("type") ? transformationParameter.PidlIdentity["type"] : null;

                Tuple<string, string, string, string, string, string> transformationParameterKey =
                    new Tuple<string, string, string, string, string, string>(country, description_type, operation, type, transformationParameter.PropertyName, transformationParameter.TransformationTarget);

                isBlocked = PIDLTransformationFactory.blockedVNextTransformationParameters.Contains(transformationParameterKey);
            }

            return isBlocked;
        }
    }
}