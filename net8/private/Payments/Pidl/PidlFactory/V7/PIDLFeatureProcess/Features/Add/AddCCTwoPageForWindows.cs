// <copyright file="AddCCTwoPageForWindows.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Used for reorganizing display content in Add CC
    /// </summary>
    internal class AddCCTwoPageForWindows : IFeature
    {
        // Key: hintId
        // Value: Dictionary where country is the key and the value is a list of style hints. Better to have country as key as there might be a case where a country has a different style hint than the default.
        private static Dictionary<string, Dictionary<string, IEnumerable<string>>> styleHints = new Dictionary<string, Dictionary<string, IEnumerable<string>>>()
        {
            {
                "accept_card_message", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-vertical-small" } }
                }
            },
            {
                "creditCardVisaLogo", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "image-small" } }
                }
            },
            {
                "creditCardMCLogo", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "image-small" } }
                }
            },
            {
                "creditCardDiscoverLogo", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "image-small" } }
                }
            },
            {
                "creditCardJCBLogo", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "image-small" } }
                }
            },
            {
                "creditCardAmexLogo", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "image-small" } }
                }
            },
            {
                "starRequiredTextGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-top-small", "margin-bottom-small-500" } }
                }
            },
            {
                "starLabel", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "text-alert", "margin-end-2x-small" } }
                }
            },
            {
                "expiryGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-triquarter", "gap-small" } }
                }
            },
            {
                "expiryMonthGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
            {
                "expiryYearGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
            {
                "cvvGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-third" } }
                }
            },
            {
                "microsoft_privacy_static_text", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-end-2x-small" } }
                }
            },
            {
                "microsoftPrivacyTextGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "display-textgroup" } }
                }
            },
            {
                "paymentOptionSaveText", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-bottom-small" } }
                }
            },
            {
                "cancelBackNextGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "gap-small", "height-fill", "alignment-end" } }
                }
            },
            {
                "cancelBackButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
            {
                "nextButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
            {
                "addressStateGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-column-large" } }
                }
            },
            {
                "addressPostalCodeGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-column-medium" } }
                }
            },
            {
                "cancelBackSaveGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "gap-small", "height-fill", "alignment-end" } }
                }
            },
            {
                "backButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
            {
                "saveButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
            {
                "cancelSubmitGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "gap-small", "height-fill", "alignment-end" } }
                }
            },
            {
                "cancelButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
            {
                "submitButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
        };

        private static Dictionary<string, string> logoUrls = new Dictionary<string, string>()
        {
            { "creditCardMCLogo", "/staticresourceservice/images/v4/logo_mc_left_aligned.svg" },
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ProcessAddCCTwoPageForWindows,
            };
        }

        internal static void ProcessAddCCTwoPageForWindows(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (featureContext == null || featureContext.PaymentMethodfamily != Constants.PaymentMethodFamilyNames.CreditCard)
            {
                return;
            }

            foreach (PIDLResource pidl in inputResources)
            {
                List<DisplayHint> starRequiredTextGroups = pidl.GetAllDisplayHintsOfId("starRequiredTextGroup");
                List<DisplayHint> microsoftPrivacyTextGroups = pidl.GetAllDisplayHintsOfId("microsoftPrivacyTextGroup");

                FeatureHelper.ConvertToGroupDisplayHint(starRequiredTextGroups, "inline");
                FeatureHelper.ConvertToGroupDisplayHint(microsoftPrivacyTextGroups, "inline");
                FeatureHelper.UpdateLogoUrl(pidl, logoUrls);

                FeatureHelper.PassStyleHints(pidl, styleHints, featureContext);
            }
        }
    }
}
