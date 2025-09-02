// <copyright file="UseTextForCVVHelpLink.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the UseTextForCVVHelpLink, which is to use text for the CVV help link.
    /// </summary>
    internal class UseTextForCVVHelpLink : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                SwapTextForCVVHelpLink
            };
        }

        internal static void SwapTextForCVVHelpLink(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig = null;
            featureContext?.FeatureConfigs?.TryGetValue(FeatureConfiguration.FeatureNames.UseTextForCVVHelpLink, out featureConfig);

            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    if (displayHintCustomizationDetail.UseTextForCVVHelpLink != null && displayHintCustomizationDetail.UseTextForCVVHelpLink.Value)
                    {
                        foreach (PIDLResource paymentMethodPidl in inputResources)
                        {
                            DisplayHint cvv = paymentMethodPidl.GetDisplayHintByPropertyName(Constants.DataDescriptionPropertyNames.CVV) as PropertyDisplayHint;
                            if (cvv != null)
                            {
                                cvv.DisplayImage = string.Empty;
                                cvv.DisplayHelpSequenceText = Constants.CVVHelpLinkText.WhatIsACVV; // will be localized in the DisplayHint
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(displayHintCustomizationDetail.CvvDisplayHelpPosition))
                    {
                        foreach (PIDLResource paymentMethodPidl in inputResources)
                        {
                            DisplayHint cvv = paymentMethodPidl.GetDisplayHintByPropertyName(Constants.DataDescriptionPropertyNames.CVV) as PropertyDisplayHint;
                            if (cvv != null)
                            {
                                cvv.DisplayHelpPosition = displayHintCustomizationDetail.CvvDisplayHelpPosition;
                            }
                        }
                    }
                }
            }
        }
    }
}