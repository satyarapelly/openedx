// <copyright file="CustomizeSEPAForm.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the CustomizeSEPAForm, which is used to Customize SEPA form.
    /// </summary>
    internal class CustomizeSEPAForm : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                CustomizeSEPAComponents,
            };
        }

        internal static void CustomizeSEPAComponents(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.CustomizeSEPAForm, out featureConfig);

            if (featureConfig != null)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    foreach (PIDLResource paymentPidlResource in inputResources)
                    {
                        if (displayHintCustomizationDetail?.CustomizeNCESEPA ?? false)
                        {
                            var allowPopupText = paymentPidlResource.GetDisplayHintById(V7.Constants.DisplayHintIds.DirectDebitSepaUpdateLine1) as TextDisplayHint;
                            if (allowPopupText != null)
                            {
                                allowPopupText.DisplayContent = PidlModelHelper.GetLocalizedString("Allow popups on your browser to be redirected to the Direct debit agreement.");
                            }

                            var privacyText = paymentPidlResource.GetDisplayHintById(V7.Constants.DisplayHintIds.MicrosoftPrivacyStaticText) as TextDisplayHint;
                            if (privacyText != null)
                            {
                                privacyText.DisplayContent = PidlModelHelper.GetLocalizedString("Microsoft respects your privacy.");
                            }

                            var privacyLinkText = paymentPidlResource.GetDisplayHintById(V7.Constants.DisplayHintIds.PrivacyStatementHyperLinkDisplayText) as HyperlinkDisplayHint;
                            if (privacyLinkText != null)
                            {
                                privacyLinkText.DisplayContent = PidlModelHelper.GetLocalizedString("View the Microsoft privacy statement");
                            }

                            paymentPidlResource.RemoveDisplayHintById(V7.Constants.DisplayHintIds.PaymentSummaryText);
                            paymentPidlResource.RemoveDisplayHintById(V7.Constants.ButtonDisplayHintIds.CancelBackButton);

                            var saveNextButton = paymentPidlResource.GetDisplayHintById(V7.Constants.ButtonDisplayHintIds.SaveNextButton) as ButtonDisplayHint;
                            if (saveNextButton != null)
                            {
                                saveNextButton.DisplayContent = PidlModelHelper.GetLocalizedString("Save");
                            }
                        }
                    }
                }
            }
        }
    }
}