// <copyright file="EnableConditionalFieldsForBillingAddress.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    internal class EnableConditionalFieldsForBillingAddress : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
                {
                    UpdateCheckboxElement,
                    AddOnValidationFailedProperty
                };
        }

        internal static void UpdateCheckboxElement(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            bool shouldHideAddressCheckBoxIfAddressIsNotPrefilledFromServer = false;
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.EnableConditionalFieldsForBillingAddress, out featureConfig);

            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    if (displayHintCustomizationDetail?.HideAddressCheckBoxIfAddressIsNotPrefilledFromServer == true)
                    {
                        shouldHideAddressCheckBoxIfAddressIsNotPrefilledFromServer = true;
                    }
                }
            }

            foreach (PIDLResource pidlResource in inputResources)
            {
                bool shouldHideAddressGroup = AreAllRequiredAddressFieldsPrefilled(pidlResource);
                var checkboxDisplayHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.AddressCheckbox) as PropertyDisplayHint;
                if (checkboxDisplayHint != null)
                {
                    AddCheckBoxElementDataDescription(pidlResource, "hideAddressGroup", shouldHideAddressGroup);
                }

                // 1) By default, the address checkbox should not be hidden when EnableConditionalFieldsForBillingAddress is enabled without hideAddressCheckBoxIfAddressIsNotPrefilledFromServer.
                // 2.1) if hideAddressCheckBoxIfAddressIsNotPrefilledFromServer is enabled, and If there is a server side prefilled address, then the checkbox should still not be hidden.
                // 2.2) if hideAddressCheckBoxIfAddressIsNotPrefilledFromServer is enabled, and If there is no server side prefilled address, then the checkbox should be hidden (the user needs to enter the address anyway, so no need to show the checkbox to hide/show the checkbox).
                // However, client side could override hideAddressTitleAndCheckbox to be false in prefillData if there is client side prefilled address, and in this case checkbox can be shown.
                AddCheckBoxElementDataDescription(pidlResource, "hideAddressTitleAndCheckbox", shouldHideAddressCheckBoxIfAddressIsNotPrefilledFromServer ? !shouldHideAddressGroup : false);
            }
        }

        internal static void AddOnValidationFailedProperty(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            foreach (PIDLResource pidlResource in inputResources)
            {
                Dictionary<string, object> addressDataDescriptions = GetAddressDataDescriptions(pidlResource);
                var checkboxDisplayHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.AddressCheckbox) as PropertyDisplayHint;
                if (addressDataDescriptions == null || checkboxDisplayHint == null)
                {
                    return;
                }

                foreach (KeyValuePair<string, object> property in addressDataDescriptions)
                {
                    var propertyDescription = (PropertyDescription)property.Value;

                    // If any of the address fields entered by user has a client side validation error, then set "hideAddressGroup" to false and show the address group
                    if (string.Equals(propertyDescription.PropertyType, "userData", StringComparison.OrdinalIgnoreCase))
                    {
                        propertyDescription.OnValidationFailed = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
                        {
                            { "hideAddressGroup", false }
                        };
                    }
                }
            }
        }

        private static bool AreAllRequiredAddressFieldsPrefilled(PIDLResource pidlResource)
        {
            Dictionary<string, object> addressDataDescriptions = GetAddressDataDescriptions(pidlResource);
            if (addressDataDescriptions == null)
            {
                return false;
            }

            bool shouldHideAddressGroup = true;
            foreach (KeyValuePair<string, object> property in addressDataDescriptions)
            {
                var propertyDescription = (PropertyDescription)property.Value;

                // If any of the required address fields are not prefilled, then the address group should not be hidden
                if (string.Equals(propertyDescription.PropertyType, "userData", StringComparison.OrdinalIgnoreCase) && !(propertyDescription.IsOptional ?? true) && string.IsNullOrEmpty(propertyDescription.DefaultValue?.ToString()))
                {
                    shouldHideAddressGroup = false;
                    break;
                }
            }

            return shouldHideAddressGroup;
        }

        private static Dictionary<string, object> GetAddressDataDescriptions(PIDLResource pidlResource)
        {
            Dictionary<string, object> detailsDataDescription = pidlResource.GetTargetDataDescription("details");
            if (detailsDataDescription == null || !detailsDataDescription.ContainsKey("address"))
            {
                return null;
            }

            List<PIDLResource> addressPidlResources = detailsDataDescription["address"] as List<PIDLResource>;
            if (addressPidlResources == null || addressPidlResources.Count == 0)
            {
                return null;
            }

            Dictionary<string, object> addressDataDescription = addressPidlResources[0].GetTargetDataDescription("address");

            return addressDataDescription;
        }

        private static void AddCheckBoxElementDataDescription(PIDLResource pidl, string propertyName, bool shouldHideAddressGroup)
        {
            Dictionary<string, object> dataDescription = pidl?.DataDescription;
            if (dataDescription != null && !dataDescription.ContainsKey(propertyName))
            {
                dataDescription[propertyName] = new PropertyDescription()
                {
                    PropertyType = "userData",
                    DataType = "bool",
                    PropertyDescriptionType = "bool",
                    IsUpdatable = true,
                    IsOptional = true,
                    IsKey = false,
                    DefaultValue = shouldHideAddressGroup,
                    IsConditionalFieldValue = true
                };
            }
        }
    }
}
