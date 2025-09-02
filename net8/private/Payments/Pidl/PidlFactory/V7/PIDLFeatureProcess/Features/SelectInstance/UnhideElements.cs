// <copyright file="UnhideElements.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the UnhideElements, which is to unhide a cancelBackButton.
    /// </summary>
    internal class UnhideElements : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                UnhideElementsAction
            };
        }

        internal static void UnhideElementsAction(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.UnhideElements, out featureConfig);

            if (featureConfig?.DisplayCustomizationDetail?.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    if (displayHintCustomizationDetail.ElementsToBeUnhidden != null)
                    {
                        Unhide(inputResources, displayHintCustomizationDetail);
                    }
                }
            }
        }

        private static void Unhide(List<PIDLResource> inputResources, DisplayCustomizationDetail displayHintCustomizationDetail)
        {
            foreach (PIDLResource pidlResource in inputResources)
            {
                foreach (string hintId in displayHintCustomizationDetail?.ElementsToBeUnhidden)
                {
                    List<DisplayHint> displayHints = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(pidlResource, hintId);
                    foreach (var displayHint in displayHints)
                    {
                        displayHint.IsHidden = false;
                    }
                }
            }
        }
    }
}