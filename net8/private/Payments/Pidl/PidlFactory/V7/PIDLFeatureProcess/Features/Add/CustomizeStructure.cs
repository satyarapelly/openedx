// <copyright file="CustomizeStructure.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the CustomizeDisplayContent, which is to change display content of a PIDL element.
    /// </summary>
    internal class CustomizeStructure : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ChangeStructure
            };
        }

        internal static void ChangeStructure(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.CustomizeStructure, out featureConfig);

            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    foreach (PIDLResource pidlResource in inputResources)
                    {
                        if (displayHintCustomizationDetail?.RemoveGroupForExpiryMonthAndYear != null && bool.Parse(displayHintCustomizationDetail?.RemoveGroupForExpiryMonthAndYear.ToString()))
                        {
                            ContainerDisplayHint expiryGroup = pidlResource.GetPidlContainerDisplayHintbyDisplayId(Constants.ExpiryPrefixes.ExpiryGroup);
                            PropertyDisplayHint expiryMonth = pidlResource.GetDisplayHintById(Constants.ExpiryPrefixes.ExpiryMonth) as PropertyDisplayHint;
                            PropertyDisplayHint expiryYear = pidlResource.GetDisplayHintById(Constants.ExpiryPrefixes.ExpiryYear) as PropertyDisplayHint;

                            if (expiryGroup != null
                                && expiryMonth != null
                                && expiryYear != null)
                            {
                                expiryGroup.Members[0] = expiryMonth;
                                expiryGroup.Members[1] = expiryYear;
                            }
                        }
                    }
                }
            }
        }
    }
}