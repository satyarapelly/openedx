// <copyright file="PidlPropertyValidationFactory.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;

    /// <summary>
    /// Class that provides the mapping for a specific Validator to a validation type
    /// </summary>
    public sealed class PidlPropertyValidationFactory
    {
        public static PidlExecutionResult ValidateProperty(PidlValidationParameter validationParameter, string language)
        {
            if (!IsValidValidationParameter(validationParameter))
            {
                throw new PIDLArgumentException("Validation parameter passed is not valid", Constants.ErrorCodes.PIDLInvalidValidationParameter);
            }

            Dictionary<string, string> pidlIdentity = validationParameter.PidlIdentity;
            string inputValue = validationParameter.Value;
            language = Helper.TryToLower(language);

            string validationTypeWithParams = string.Empty;
            if (!string.IsNullOrEmpty(validationParameter.UrlValidationType))
            {
                // Clients are expected to send a dot delimited UrlValidationType property.  
                validationTypeWithParams = validationParameter.UrlValidationType;
            }
            else
            {
                // Task 17438397: Remove propertyName and pidlIdentity and make urlValidationType a required property after clients move to the new contract
                var propertyDescription = PIDLResourceFactory.Instance.GetPropertyDescriptionByPropertyName(
                                            validationParameter.PidlIdentity[Constants.DescriptionIdentityFields.DescriptionType],
                                            validationParameter.PidlIdentity[Constants.DescriptionIdentityFields.Type],
                                            GlobalConstants.Defaults.OperationKey,
                                            validationParameter.PidlIdentity[Constants.DescriptionIdentityFields.Country],
                                            validationParameter.PropertyName);

                // Find first validation object with service validationType, to support multiple service validations,
                // more context will need to be passed in validationParameter
                PropertyValidation propertyValidation = PIDLResourceFactory.Instance.GetPropertyValidationList(
                    propertyDescription.PropertyDescriptionId,
                    validationParameter.PidlIdentity[Constants.DescriptionIdentityFields.Country]).Find(
                        validation => validation.ValidationType.Equals(Constants.ValidationTypes.Service));

                validationTypeWithParams = propertyValidation.UrlValidationType;
            }

            // In the dot delimited validationTypeWithParams, the first value is expected to be the validationType.  Subsequest values are dependent on the validationType. 
            // Example is "phonenumber.us" since the phonenumber validationType requires a country parameter.
            string validationType = validationTypeWithParams.Split(new char[] { '.' })[0];
            if (string.Equals(validationType, Constants.ServerValidationType.PhoneNumber, StringComparison.InvariantCultureIgnoreCase))
            {
                string countryCode = string.Empty;

                if (!string.IsNullOrEmpty(validationParameter.UrlValidationType))
                {
                    countryCode = validationParameter.UrlValidationType.Contains(".")
                        ? validationParameter.UrlValidationType.Split(new char[] { '.' })[1]
                        : string.Empty;

                    if (string.IsNullOrEmpty(countryCode))
                    {
                        return new PidlExecutionResult()
                        {
                            Status = PidlExecutionResultStatus.Failed,
                            ErrorCode = Constants.ErrorCodes.PIDLMissingCountryCodeInPhoneNumberValidation,
                            ErrorMessage = "The UrlValidationType property should contain a country code in this format: phonenumber.<CountryCode>"
                        };
                    }
                }
                else
                {
                    // Task 17438397: Remove propertyName and pidlIdentity and make urlValidationType a required property after clients move to the new contract
                    if (!pidlIdentity.ContainsKey("country"))
                    {
                        return new PidlExecutionResult()
                        {
                            Status = PidlExecutionResultStatus.Failed,
                            ErrorCode = Constants.ErrorCodes.PIDLMissingCountryCodeInIdentity,
                            ErrorMessage = "The Pidl Identity should contain a country field for the input pidl"
                        };
                    }

                    countryCode = pidlIdentity["country"];
                }
                
                validationType = Constants.ServerValidationType.PhoneNumberVNext;

                PhoneNumberValidator phoneValidator = new PhoneNumberValidator();
                return phoneValidator.Validate(validationType, inputValue, countryCode);
            }
            else
            {
                return new PidlExecutionResult()
                {
                    Status = PidlExecutionResultStatus.Failed,
                    ErrorCode = Constants.ErrorCodes.PIDLValidationTypeNotSupported,
                    ErrorMessage = string.Format("The validation type {0} is not supported by the factory", validationType)
                };
            }
        }

        // Task 17438397: Remove propertyName and pidlIdentity and make urlValidationType a required property after clients move to the new contract
        private static bool IsValidValidationParameter(PidlValidationParameter validationParameter)
        {
            if (validationParameter == null)
            {
                return false;
            }

            // new contract
            if (!string.IsNullOrEmpty(validationParameter.UrlValidationType)
                && !string.IsNullOrEmpty(validationParameter.Value))
            {
                return true;
            }

            // old contract
            if (validationParameter.PidlIdentity != null
                && !string.IsNullOrEmpty(validationParameter.Value)
                && !string.IsNullOrEmpty(validationParameter.PropertyName)
                && validationParameter.PidlIdentity.ContainsKey(Constants.DescriptionIdentityFields.DescriptionType)
                && validationParameter.PidlIdentity.ContainsKey(Constants.DescriptionIdentityFields.Country)
                && validationParameter.PidlIdentity.ContainsKey(Constants.DescriptionIdentityFields.Type))
            {
                return true;
            }

            return false;
        }
    }
}