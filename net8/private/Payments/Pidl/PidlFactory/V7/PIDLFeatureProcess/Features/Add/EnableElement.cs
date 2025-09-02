// <copyright file="EnableElement.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the Enable element, which is to enabled country dropdown in credit card form.
    /// </summary>
    internal class EnableElement : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                EnableElements
            };
        }

        internal static void EnableElements(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.EnableElement, out featureConfig);

            if (featureConfig?.DisplayCustomizationDetail?.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    if (displayHintCustomizationDetail.EnableCountryAddorUpdateCC)
                    {
                        foreach (PIDLResource paymentMethodPidl in inputResources)
                        {
                            EnabledDisplayHint(paymentMethodPidl, Constants.DisplayHintIds.AddressCountry);
                        }
                    }
                }
            }
        }

        private static void EnabledDisplayHint(PIDLResource resource, string hintId)
        {
            DisplayHint displayHint = resource.GetDisplayHintById(hintId);
            if (displayHint != null)
            {
                displayHint.IsDisabled = false;
            }
        }
    }
}
