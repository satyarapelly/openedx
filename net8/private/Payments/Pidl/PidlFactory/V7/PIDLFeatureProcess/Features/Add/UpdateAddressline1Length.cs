// <copyright file="UpdateAddressline1Length.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the UpdateAddressline1Length, which is to change max length for address line 1 across forms.
    /// Note: This is created for testing the maxLength change under flight before making it default in csv files.
    /// </summary>
    internal class UpdateAddressline1Length : IFeature
    {
        private readonly List<string> addressLine1PropertyNames = new List<string>
        {
            Constants.AddressDataDescriptionProperty.Line1,
            Constants.AddressDataDescriptionProperty.AddressLine1,
            Constants.AddressDataDescriptionProperty.AddressLine1NoSpace,
            Constants.AddressDataDescriptionProperty.HapiSuaLine1,
            Constants.AddressDataDescriptionProperty.AddressLineUnderscore1,
            Constants.AddressDataDescriptionProperty.ProfileAddressLine1DataBinding,
            Constants.AddressDataDescriptionProperty.ProfileLegalEntityAddressLine1DataBinding,
            Constants.AddressDataDescriptionProperty.AddressBillingGroupAddressLine1,
            Constants.AddressDataDescriptionProperty.HapiV1ModernAccountV20190531OrganizationAddressAddressLine1,
            Constants.AddressDataDescriptionProperty.OrgAddressAddressLine1,
            Constants.AddressDataDescriptionProperty.HapiV1ModernAccountV20190531OrganizationAddressBillToAddressLine1,
            Constants.AddressDataDescriptionProperty.HapiV1ModernAccountV20190531IndividualAddressAddressLine1,
            Constants.AddressDataDescriptionProperty.HapiV1ModernAccountV20190531AddressAddressLine1,
            Constants.AddressDataDescriptionProperty.HapiV1ModernAccountV20190531IndividualAddressBillToAddressLine1
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                this.UpdateMaxLength
            };
        }

        /// <summary>
        /// Updates the max length for address line 1 across all relevant PIDL forms to be 255 and same change for regex..
        /// </summary>
        /// <param name="inputResources">Input pidl resources</param>
        /// <param name="featureContext">Feature context</param>
        internal void UpdateMaxLength(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            foreach (PIDLResource pidlResource in inputResources)
            {
                foreach (string addressLine1PropertyName in this.addressLine1PropertyNames)
                {
                    PropertyDescription addressLine1PropertyDescription = pidlResource.GetPropertyDescriptionByPropertyName(addressLine1PropertyName);

                    if (addressLine1PropertyDescription != null)
                    {
                        PropertyValidation addlineLine1Validation = addressLine1PropertyDescription.Validations?.FirstOrDefault();
                        if (addlineLine1Validation != null)
                        {
                            addlineLine1Validation.Regex = "^(?!^[\\u0009\\u000A\\u000D\\u0020\\u2000-\\u200B]*$)[\\u0009\\u000A\\u000D\\u0020-\\uD7FF\\uE000-\\uFFFD\\u10000-\\u10FFFF]{1,255}$";
                        }

                        List<DisplayHint> addressline1DisplayHints = pidlResource.GetAllDisplayHints()?.Where(dh => dh.PropertyName == addressLine1PropertyName && dh is PropertyDisplayHint)?.ToList();
                        if (addressline1DisplayHints != null)
                        {
                            foreach (DisplayHint addressline1DisplayHint in addressline1DisplayHints)
                            {
                                PropertyDisplayHint addressLine1PropertyDisplayHint = addressline1DisplayHint as PropertyDisplayHint;

                                if (addressLine1PropertyDisplayHint != null)
                                {
                                    addressLine1PropertyDisplayHint.MaxLength = 255;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}