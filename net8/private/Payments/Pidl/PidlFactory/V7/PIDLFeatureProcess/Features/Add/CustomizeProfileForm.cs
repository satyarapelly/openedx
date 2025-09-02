// <copyright file="CustomizeProfileForm.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the CustomizeProfileForm, which is used to Customize  the Profile form.
    /// </summary>
    internal class CustomizeProfileForm : IFeature
    {
        private static Dictionary<string, Dictionary<string, string>> profileFieldMappings = new Dictionary<string, Dictionary<string, string>>()
        {
            { Constants.ProfileTypes.Employee, Constants.ProfileEmployeeDisplayHints },
            { Constants.ProfileTypes.Organization, Constants.ProfileOrganizationDisplayHints },
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                CustomizeProfileComponents,
            };
        }

        internal static void CustomizeProfileComponents(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.CustomizeProfileForm, out featureConfig);

            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    if (string.Equals(featureContext.OriginalTypeName, displayHintCustomizationDetail.ProfileType, StringComparison.OrdinalIgnoreCase))
                    {
                        var fieldsToRemove = displayHintCustomizationDetail?.DataFieldsToRemoveFromPayload;
                        if (fieldsToRemove != null && fieldsToRemove.Count() > 0)
                        {
                            foreach (PIDLResource profilePidl in inputResources)
                            {
                                PIDLResourceFactory.RemoveDataDescriptionWithFullPath(profilePidl, displayHintCustomizationDetail.DataFieldsToRemoveFullPath ?? null, fieldsToRemove.ToArray<string>());
                            }
                        }

                        if (!string.IsNullOrEmpty(displayHintCustomizationDetail.ConvertProfileTypeTo))
                        {
                            foreach (PIDLResource profilePidl in inputResources)
                            {
                                if (profilePidl.DataDescription != null)
                                {
                                    object profileType;
                                    profilePidl.DataDescription.TryGetValue(Constants.PidlIdentityFields.Type, out profileType);

                                    if (profileType != null)
                                    {
                                        (profileType as PropertyDescription).DefaultValue = displayHintCustomizationDetail.ConvertProfileTypeTo;
                                        profilePidl.DataDescription[Constants.PidlIdentityFields.Type] = profileType;
                                    }
                                }
                            }
                        }

                        var fieldsToBeDisabled = displayHintCustomizationDetail?.FieldsToBeDisabled;
                        if (fieldsToBeDisabled != null && fieldsToBeDisabled.Count() > 0)
                        {
                            foreach (PIDLResource pidl in inputResources)
                            {
                                foreach (string fieldToBeDisabled in fieldsToBeDisabled)
                                {
                                    if (profileFieldMappings.TryGetValue(featureContext.OriginalTypeName, out var typeMapping))
                                    {
                                        if (typeMapping.TryGetValue(fieldToBeDisabled, out var displayHintId))
                                        {
                                            var displayHint = pidl.GetDisplayHintById(displayHintId);
                                            if (displayHint != null)
                                            {
                                                displayHint.IsDisabled = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}