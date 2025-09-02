// <copyright file="RemoveAddressFieldsValidationForCC.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Newtonsoft.Json;

    /// <summary>
    /// Class representing the RemoveAddressFieldsValidationForCC, which removes the regex, mandatory, min/max length etc. validations for
    /// creditcard address fields and adds the address properties to the DataDescription if not already present for all countries.
    /// </summary>
    internal class RemoveAddressFieldsValidationForCC : IFeature
    {
        // Mapping of address fields to their respective property config values.
        // Value is country, where the property is used/found if needed to fetch from csv.
        private static readonly Dictionary<string, string> addressFieldsPropertyConfig = new Dictionary<string, string>
        {
            { Constants.AddressDataDescriptionProperty.AddressLine1, Constants.CountryCodes.UnitedStates },
            { Constants.AddressDataDescriptionProperty.AddressLine2, Constants.CountryCodes.UnitedStates },
            { Constants.AddressDataDescriptionProperty.AddressLine3, Constants.CountryCodes.India },
            { Constants.AddressDataDescriptionProperty.AddressCity, Constants.CountryCodes.UnitedStates },
            { Constants.AddressDataDescriptionProperty.Region, Constants.CountryCodes.UnitedStates },
            { Constants.AddressDataDescriptionProperty.AddressPostalCode, Constants.CountryCodes.UnitedStates },
            { Constants.AddressDataDescriptionProperty.Country, Constants.CountryCodes.UnitedStates },
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                RemoveAddressFieldsValidation,
            };
        }

        internal static void RemoveAddressFieldsValidation(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (string.Equals(featureContext.PaymentMethodfamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource paymentMethodPidl in inputResources)
                {
                    object addressPidlObject = null;
                    Dictionary<string, object> detailsDataDescriptions = paymentMethodPidl.GetTargetDataDescription(Constants.DataDescriptionVariableNames.Details);
                    detailsDataDescriptions?.TryGetValue(Constants.DataDescriptionIds.Address, out addressPidlObject);
                    PIDLResource addressPidl = (addressPidlObject as List<PIDLResource>)?.FirstOrDefault();
                    
                    if (addressPidl != null)
                    {
                        foreach (KeyValuePair<string, string> addressProperty in addressFieldsPropertyConfig)
                        {
                            RemoveDisplayHintValidations(paymentMethodPidl, addressProperty);

                            PropertyDescription addressPropertyDescription = addressPidl.GetPropertyDescriptionByPropertyName(addressProperty.Key);

                            // Add the property in DataDescription if not found
                            if (addressPropertyDescription == null)
                            {
                                PropertyDescription referencedPropertyDescription = PIDLResourceFactory.Instance.GetPropertyDescriptionByPropertyName(
                                    Constants.DataDescriptionIds.Address,
                                    Constants.AddressTypes.Billing,
                                    Constants.PidlOperationTypes.Add,
                                    addressProperty.Value,
                                    addressProperty.Key);

                                addressPropertyDescription = new PropertyDescription(referencedPropertyDescription, new Dictionary<string, string>(), false);
                                addressPidl.DataDescription.Add(addressProperty.Key, addressPropertyDescription);
                            }

                            // Pidl sdk requires resolvedPidl to show inline logo for cardnumber and for resolvedPidl the fields
                            // should have validations. Set the universal regex to accept all input.
                            addressPropertyDescription.Validations = new List<PropertyValidation>()
                            {
                                new PropertyValidation("^(.*?)$", "invalid_property", "Property is invalid")
                            };
                            
                            addressPropertyDescription.IsOptional = true;
                        }
                    }
                }
            }
        }

        private static void RemoveDisplayHintValidations(PIDLResource paymentMethodPidl, KeyValuePair<string, string> addressProperty)
        {
            // Process the DisplayDescription
            PropertyDisplayHint addressPropertyDisplayHint = paymentMethodPidl.GetDisplayHintByPropertyName(addressProperty.Key) as PropertyDisplayHint;

            if (addressPropertyDisplayHint != null)
            {
                addressPropertyDisplayHint.MaxLength = null;
                addressPropertyDisplayHint.MinLength = null;
                addressPropertyDisplayHint.DisplayErrorMessages = null;

                // Set the possible options and values to null to allow custom values for dropdown fields
                addressPropertyDisplayHint.PossibleOptions = null;
                addressPropertyDisplayHint.PossibleValues = null;
            }
        }
    }
}
