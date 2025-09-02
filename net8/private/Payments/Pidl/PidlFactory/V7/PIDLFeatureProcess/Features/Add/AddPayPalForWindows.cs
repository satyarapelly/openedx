// <copyright file="AddPayPalForWindows.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    internal class AddPayPalForWindows : IFeature
    {
        private static Dictionary<string, Dictionary<string, IEnumerable<string>>> styleHints = new Dictionary<string, Dictionary<string, IEnumerable<string>>>()
        {
            {
                "paypalLogo", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "image-small", "margin-vertical-small" } }
                }
            },
            {
                "paypalInstructionTextGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "flex-wrap", "display-textgroup" } }
                } 
            },
            {
                "microsoftPrivacyTextGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "display-textgroup" } }
                }
            },
            {
                "paypalText1", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-end-2x-small" } }
                }
            },
            {
                "paypalText2", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "text-bold" } }
                }
            },
            {
                "paypalRedirectText2", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-end-2x-small" } }
                }
            },
            {
                "paymentSummaryText", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-vertical-small" } }
                }
            },
            {
                "cancelBackSaveNextGroup", new Dictionary<string, IEnumerable<string>>()
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
                "saveNextButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
            {
                "paypalQrCodeImage", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "image-large" } }
                }
            },
            {
                "paypalQrCodeChallengePageButtonGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "gap-small", "height-fill", "alignment-end" } }
                }
            },
            {
                "paypalQrCodeChallengePageBackButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
            {
                "paypalYesButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
            {
                "paypalQrCodeChallengePage2ButtonGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "gap-small" } }
                }
            },
            {
                "paypalQrCodeChallengePage2NextButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
            {
                "paypalQrCodeChallengePage", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "height-fill", "gap-small" } }
                }
            },
            {
                "paypalPIShortUrlGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill", "alignment-horizontal-center" } }
                }
            },
            {
                "paypalQrCodeImageAndURLGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "alignment-vertical-center" } }
                }
            }
        };

        private static Dictionary<string, string> logoUrls = new Dictionary<string, string>()
        {
            { "paypalLogo", "/staticresourceservice/images/v4/logo_paypal.svg" },
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ProcessAddPayPalForWindows,
            };
        }

        internal static void ProcessAddPayPalForWindows(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (featureContext == null || featureContext.PaymentMethodfamily != Constants.PaymentMethodFamilyNames.Ewallet || featureContext.PaymentMethodType != Constants.PaymentMethodTypeNames.Paypal)
            {
                return;
            }

            foreach (PIDLResource pidl in inputResources)
            {
                List<DisplayHint> microsoftPrivacyTextGroups = pidl.GetAllDisplayHintsOfId("microsoftPrivacyTextGroup");
                List<DisplayHint> paypalInstructionTextGroups = pidl.GetAllDisplayHintsOfId("paypalInstructionTextGroup");

                FeatureHelper.ConvertToGroupDisplayHint(microsoftPrivacyTextGroups, "inline");
                FeatureHelper.ConvertToGroupDisplayHint(paypalInstructionTextGroups, "inline");
                FeatureHelper.UpdateLogoUrl(pidl, logoUrls);

                FeatureHelper.PassStyleHints(pidl, styleHints, featureContext);
            }
        }
    }
}
