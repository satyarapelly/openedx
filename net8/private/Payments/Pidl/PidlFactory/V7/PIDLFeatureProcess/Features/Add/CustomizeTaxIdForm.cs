// <copyright file="CustomizeTaxIdForm.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the CustomizeTaxIdForm, which is used to Customize Tax Id form.
    /// </summary>
    internal class CustomizeTaxIdForm : IFeature
    {
        private static Dictionary<string, Dictionary<string, List<string>>> displayHintIdMappings = new Dictionary<string, Dictionary<string, List<string>>>()
        {
            { Constants.DataSource.ConsumerTaxId, Constants.ConsumerTaxIdMappings }
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                CustomizeTaxIdComponents,
            };
        }

        internal static void CustomizeTaxIdComponents(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (string.Equals(featureContext?.ResourceType, Constants.DescriptionTypes.TaxIdDescription, StringComparison.OrdinalIgnoreCase))
            {
                FeatureConfig featureConfig;
                featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.CustomizeTaxIdForm, out featureConfig);

                if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
                {
                    foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                    {
                        if (displayHintCustomizationDetail?.DisableCountryDropdown != null && bool.TryParse(displayHintCustomizationDetail.DisableCountryDropdown.ToString(), out bool disableCountryDropdown) && disableCountryDropdown)
                        {
                            foreach (PIDLResource taxPidl in inputResources)
                            {
                                taxPidl.SetPropertyState(Constants.DataDescriptionPropertyNames.Country, false);

                                foreach (var linkedPidl in taxPidl.LinkedPidls ?? Enumerable.Empty<PIDLResource>())
                                {
                                    linkedPidl.SetPropertyState(Constants.DataDescriptionPropertyNames.Country, false);
                                }
                            }
                        }

                        if (displayHintCustomizationDetail.FieldsToMakeRequired != null)
                        {
                            SetFieldsAsRequired(displayHintCustomizationDetail, inputResources, featureContext);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method is used to make the fields mandatory for Jarivs and Hapi Address User Type.
        /// </summary>
        /// <param name="displayHintCustomizationDetail">It is used to check the feature is enaled or not.</param>
        /// <param name="pidlResource">It contains the pidl.</param>
        /// <param name="featureContext">It contains the feature context.</param>
        private static void SetFieldsAsRequired(DisplayCustomizationDetail displayHintCustomizationDetail, List<PIDLResource> pidlResource, FeatureContext featureContext)
        {
            foreach (PIDLResource pidl in pidlResource)
            {
                foreach (string hintId in displayHintCustomizationDetail?.FieldsToMakeRequired)
                {
                    List<string> displayHintIds;

                    if (displayHintIdMappings.TryGetValue(displayHintCustomizationDetail.DataSource, out Dictionary<string, List<string>> hintIdMappings)
                        && (hintIdMappings != null && hintIdMappings.TryGetValue(hintId, out displayHintIds)))
                    {
                        foreach (string displayHintId in displayHintIds)
                        {
                            var propertyDescriptionId = pidl.GetDisplayHintById(displayHintId)?.PropertyName;
                            pidl.UpdateIsOptionalProperty(propertyDescriptionId, false);
                            RemoveOptionalFromDisplayName(pidl, displayHintId, featureContext);
                        }
                    }
                }
            }
        }

        private static void RemoveOptionalFromDisplayName(PIDLResource pidlResource, string hintId, FeatureContext featureContext)
        {
            // Get the display hint by feature flight
            var displayHintbyFeatureFlight = FeatureHelper.GetDisplayHintByFeatureFlight(hintId, featureContext, Constants.FeatureFlight.RemoveOptionalInLabel).FirstOrDefault();

            var textDisplayHintWithoutOptional = displayHintbyFeatureFlight != null && string.Equals(displayHintbyFeatureFlight.HintId, hintId, StringComparison.OrdinalIgnoreCase)
                 ? displayHintbyFeatureFlight as PropertyDisplayHint : null;

            PropertyDisplayHint textDisplayHint = pidlResource.GetDisplayHintById(hintId) as PropertyDisplayHint;

            if (textDisplayHint != null && textDisplayHintWithoutOptional != null)
            {
                textDisplayHint.DisplayName = LocalizationRepository.Instance.GetLocalizedString(textDisplayHintWithoutOptional.DisplayName, featureContext.Language);
                textDisplayHint.AddOrUpdateDisplayTag(Constants.DiplayHintProperties.AccessibilityName, textDisplayHint.DisplayName);
            }
        }
    }
}