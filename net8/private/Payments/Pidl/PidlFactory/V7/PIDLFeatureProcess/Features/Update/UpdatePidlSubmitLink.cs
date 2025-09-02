// <copyright file="UpdatePidlSubmitLink.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the UpdatePidlSubmitLink feature, which Customize the Submit Link.
    /// </summary>
    internal class UpdatePidlSubmitLink : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                CustomizeSubmitLink
            };
        }

        internal static void CustomizeSubmitLink(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig = null;
            featureContext?.FeatureConfigs?.TryGetValue(FeatureConfiguration.FeatureNames.UpdatePidlSubmitLink, out featureConfig);

            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    // Updates Consumer Profile SubmitLink To Jarvis Patch
                    if (displayHintCustomizationDetail.UpdateConsumerProfileSubmitLinkToJarvisPatch != null && displayHintCustomizationDetail.UpdateConsumerProfileSubmitLinkToJarvisPatch.Value)
                    {
                        foreach (PIDLResource resource in inputResources)
                        {
                            if (string.Equals(resource.Identity[Constants.DescriptionIdentityFields.DescriptionType], Constants.DescriptionTypes.ProfileDescription, StringComparison.OrdinalIgnoreCase)
                                && string.Equals(resource.Identity[Constants.DescriptionIdentityFields.Type], Constants.ProfileTypes.Consumer, StringComparison.OrdinalIgnoreCase)
                                && string.Equals(featureContext.OperationType, Constants.PidlOperationTypes.Update, StringComparison.OrdinalIgnoreCase))
                            {
                                // Profile update flow sets the overrideJarvisVersionToV3 to true which uses the Jarvis V3 version.
                                UpdateSubmitLinkByHintId(FeatureHelper.GetButtonDisplayHintInPIDLResource(resource), Constants.SubmitUrls.JarvisFdProfileUpdateClientPrefillingUrlTemplate, GlobalConstants.HttpMethods.Patch);
                            }
                        }
                    }

                    // Sets submitURL to be empty to allow both primary and linked pidl payload return to partner
                    if (displayHintCustomizationDetail.SetSubmitURLToEmptyForTaxId != null && displayHintCustomizationDetail.SetSubmitURLToEmptyForTaxId.Value)
                    {
                        foreach (PIDLResource resource in inputResources)
                        {
                            if (string.Equals(featureContext.ResourceType, Constants.DescriptionTypes.TaxIdDescription, StringComparison.OrdinalIgnoreCase))
                            {
                                UpdateSubmitLinkByHintId(FeatureHelper.GetButtonDisplayHintInPIDLResource(resource), string.Empty);
                            }
                        }
                    }
                }
            }
        }

        private static void UpdateSubmitLinkByHintId(DisplayHint hintId, string href, string method = null)
        {
            var submitLink = hintId.Action.Context as PXCommon.RestLink;
            if (submitLink != null)
            {
                submitLink.Href = href;
                if (method != null)
                {
                    submitLink.Method = method;
                }
            }
        }
    }
}