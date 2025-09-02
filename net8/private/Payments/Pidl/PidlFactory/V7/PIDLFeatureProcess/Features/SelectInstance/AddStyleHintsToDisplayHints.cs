// <copyright file="AddStyleHintsToDisplayHints.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the AddStyleHintsToDisplayHints, which is to add styleHints to display hints in PIDL forms.
    /// </summary>
    internal class AddStyleHintsToDisplayHints : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                AddStyleHintsToDisplayHintsAction
            };
        }

        internal static void AddStyleHintsToDisplayHintsAction(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.AddStyleHintsToDisplayHints, out featureConfig);

            if (featureConfig?.DisplayCustomizationDetail?.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    if (displayHintCustomizationDetail?.StyleHintsToBeAdded != null)
                    {
                        foreach (var hintId in displayHintCustomizationDetail.StyleHintsToBeAdded.Keys)
                        {
                            var styleHints = displayHintCustomizationDetail.StyleHintsToBeAdded[hintId];
                            if (styleHints != null)
                            {
                                AddStyleHintsToDisplayHint(inputResources, hintId, styleHints);
                            }
                        }
                    }
                }
            }
        }

        internal static void AddStyleHintsToDisplayHint(List<PIDLResource> inputResources, string hintId, List<string> styleHints)
        {
            if (inputResources == null || string.IsNullOrEmpty(hintId) || styleHints?.Count == 0)
            {
                return;
            }

            foreach (PIDLResource resource in inputResources)
            {
                foreach (DisplayHint displayHint in PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(resource, hintId))
                {
                    displayHint?.AddStyleHints(styleHints);
                }
            }
        }
    }
}