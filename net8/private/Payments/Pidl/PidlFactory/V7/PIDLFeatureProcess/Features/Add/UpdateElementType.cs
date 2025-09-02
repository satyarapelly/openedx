// <copyright file="UpdateElementType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    internal class UpdateElementType : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                UpdateElement
            };
        }

        internal static void UpdateElement(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.ChangeDisplayHintToText, out featureConfig);

            if (featureConfig?.DisplayCustomizationDetail?.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    foreach (PIDLResource pidlResource in inputResources)
                    {
                        if (displayHintCustomizationDetail?.UpdatePaymentMethodHeadingTypeToText != null && bool.Parse(displayHintCustomizationDetail?.UpdatePaymentMethodHeadingTypeToText.ToString()))
                        {
                            ChangeDisplayHintToText(pidlResource);
                        }

                        if (displayHintCustomizationDetail?.SetPrivacyStatementHyperLinkDisplayToButton != null && bool.Parse(displayHintCustomizationDetail?.SetPrivacyStatementHyperLinkDisplayToButton.ToString()))
                        {
                            var displayHint = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PrivacyStatementHyperLinkDisplayText) as HyperlinkDisplayHint;
                            if (displayHint != null)
                            {
                                // Set the DisplayType of PrivacyStatementHyperLinkDisplayText from HyperLink to Button
                                displayHint.DisplayHintType = Constants.DisplayHintTypes.Button;
                                displayHint.StyleHints = new List<string> { Constants.DisplayHintStyle.DisplayHyperLink };
                            }
                        }
                    }
                }
            }
        }

        private static void ChangeDisplayHintToText(PIDLResource resource)
        {
            IEnumerable<DisplayHint> displayHints = resource.GetAllDisplayHints();
            foreach (DisplayHint displayHint in displayHints)
            {
                if (displayHint.DisplayHintType.Equals(Constants.DisplayHintTypes.Heading, StringComparison.OrdinalIgnoreCase)
                    || displayHint.DisplayHintType.Equals(Constants.DisplayHintTypes.SubHeading, StringComparison.OrdinalIgnoreCase))
                {
                    displayHint.DisplayHintType = Constants.DisplayHintTypes.Text;
                }
            }
        }
    }
}
