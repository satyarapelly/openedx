// <copyright file="SelectPMButtonListStyleForWindows.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using static Microsoft.Commerce.Payments.PidlFactory.V7.Constants;

    /// <summary>
    /// Class representing the SkipSelectPM, which skips select PM if CreditCard is the only option and goes straight to CreditCard AddResource
    /// </summary>
    internal class SelectPMButtonListStyleForWindows : IFeature
    {
        private static Dictionary<string, Dictionary<string, IEnumerable<string>>> styleHints = new Dictionary<string, Dictionary<string, IEnumerable<string>>>()
        {
            {
                "backButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
            {
                "optionContainer_", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill", "justify-content-space-between", "margin-start-small" } }
                }
            },
            {
                "paymentOptionTextGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-start-small" } }
                }
            },
            {
                "paymentMethodOption_", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill", "justify-content-space-between" } }
                }
            },
            {
                "multiplePaymentMethodLogosRowOneGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "gap-x-small" } }
                }
            }
        };

        private static Dictionary<string, string> logoUrls = new Dictionary<string, string>()
        {
           { "credit_card_mc_logo", "/staticresourceservice/images/v4/logo_mc_left_aligned.svg" },
           { "ewallet_venmo_logo", "/staticresourceservice/images/v4/v2_logo_venmo.svg" },
           { "single-ewallet_venmo_logo_sub_page", "/staticresourceservice/images/v4/v2_logo_venmo.svg" },
           { "online_bank_transfer_paysafecard_logo", "/staticresourceservice/images/v4/logo_paysafecard.png" },
           { "single-online_bank_transfer_paysafecard_logo_sub_page", "/staticresourceservice/images/v4/logo_paysafecard.png" }
        };

        private static Dictionary<string, IEnumerable<string>> possibleOptionStyleHints = new Dictionary<string, IEnumerable<string>>()
        {
            {
                "paymentMethodSelectPMGroupingPage", new List<string> { "height-x-small", "border-round-small", "border-gray", "selection-highlight-blue", "selection-highlight-border-accent", "margin-end-x-small", "width-fill" }
            },
            {
                "paymentMethodSubGroupPage_ewallet_ewallet", new List<string> { "height-x-small", "border-round-small", "border-gray", "selection-highlight-blue", "selection-highlight-border-accent", "width-fill" }
            }
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ApplyWindowsSelectPMButtonListStyles
            };
        }

        private static void ApplyWindowsSelectPMButtonListStyles(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            // This feature modifies the style hints of selectMSrewards if we allow it to execute this block. 
            // All these features are enabled for Select operation from PSS. 
            // We need to have an early exit condition to match the flow of execution of each select operation.
            if (featureContext == null || featureContext.ResourceType == ResourceTypes.Rewards)
            {
                return;
            }

            foreach (PIDLResource pidl in inputResources)
            {
                pidl.RemoveDisplayHintById("backButton");
                List<DisplayHint> logoContainer_ = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(pidl, "logoContainer_");
                List<DisplayHint> paymentMethodOption_ = PIDLResourceDisplayHintFactory.GetAllDisplayHintsOfId(pidl, "paymentMethodOption_");

                FeatureHelper.ConvertToGroupDisplayHint(logoContainer_, "inline");
                FeatureHelper.ConvertToGroupDisplayHint(paymentMethodOption_, "inline");
                
                FeatureHelper.UpdateLogoUrl(pidl, logoUrls);
                FeatureHelper.PassStyleHints(pidl, styleHints, featureContext);

                foreach (PageDisplayHint page in pidl.DisplayPages)
                {
                    if (possibleOptionStyleHints.ContainsKey(page.HintId))
                    {
                        foreach (DisplayHint displayHint in page.Members)
                        {
                            PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;
                            if (propertyDisplayHint != null && propertyDisplayHint.PossibleOptions != null)
                            {
                                foreach (KeyValuePair<string, SelectOptionDescription> option in propertyDisplayHint.PossibleOptions)
                                {
                                    option.Value.StyleHints = possibleOptionStyleHints[page.HintId];
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
