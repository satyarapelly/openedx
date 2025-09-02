// <copyright file="EnablePlaceholder.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the EnablePlaceholder feature, which is to enable placeholders in textboxes by hiding the associated labels.
    /// </summary>
    internal class EnablePlaceholder : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                EnablePlaceholders
            };
        }

        internal static void EnablePlaceholders(List<PIDLResource> inputResources, FeatureContext featureContext)
        {            
            foreach (PIDLResource pidlResource in inputResources)
            {
                EnablePlaceholdersInTextboxes(pidlResource);

                if (pidlResource.LinkedPidls != null)
                {
                    foreach (PIDLResource linkedPidl in pidlResource.LinkedPidls)
                    {
                        EnablePlaceholdersInTextboxes(linkedPidl);
                    }
                }
            }
        }

        private static void EnablePlaceholdersInTextboxes(PIDLResource pidlResource)
        {
            IEnumerable<DisplayHint> displayHints = pidlResource.GetAllDisplayHints();

            foreach (DisplayHint displayHint in displayHints)
            {
                PropertyDisplayHint displayProperty = displayHint as PropertyDisplayHint;

                if (displayProperty != null && string.Equals(displayProperty.ShowDisplayName, "true", StringComparison.OrdinalIgnoreCase)
                    && displayProperty.PossibleValues == null && displayProperty.SelectType == null && displayProperty.DisplayExample == null)
                {
                    displayProperty.DisplayExample = new List<string>() { displayProperty.DisplayName };
                }
            }
        }
    }
}
