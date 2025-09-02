// <copyright file="AddVenmoForWindows.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    internal class AddVenmoForWindows : IFeature
    {
        private static Dictionary<string, Dictionary<string, IEnumerable<string>>> styleHints = new Dictionary<string, Dictionary<string, IEnumerable<string>>>()
        {
            {
                "venmoLogo", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "image-small", "margin-vertical-small" } }
                }
            },
            {
                "venmoQrCodeChallengeLoginIframeLink", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-vertical-small" } }
                }
            },
            {
                "venmoInstructionTextGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "flex-wrap", "display-textgroup" } }
                }
            },
            {
                "venmoText1", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-end-2x-small" } }
                }
            },
            {
                "venmoText2", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "text-bold" } }
                }
            },
            {
                "venmoText3", new Dictionary<string, IEnumerable<string>>()
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
                "venmoQrCodeChallengeGoToVenmoText", new Dictionary<string, IEnumerable<string>>()
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
                "venmoQrCodeChallengePage", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "height-fill", "gap-small" } }
                }
            },
            {
                "venmoQrCodeImage", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "image-large" } }
                }
            },
            {
                "venmoQrCodeChallengePageButtonGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "height-fill", "alignment-end" } }
                }
            },
            {
                "venmoQrCodeChallengePageBackButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
            {
                "backMoveFirstButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
            {
                "venmoQrCodeChallengePage2ButtonGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "gap-small", "height-fill", "alignment-end" } }
                }
            },
            {
                "venmoQrCodeChallengePage2NextButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
            {
                "venmoURLGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill", "alignment-horizontal-center" } }
                }
            },
            {
                "venmoQrCodeImageAndURLGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "alignment-vertical-center" } }
                }
            }
        };

        private static Dictionary<string, string> logoUrls = new Dictionary<string, string>()
        {
            { "venmoLogo", "/staticresourceservice/images/v4/v2_logo_venmo.svg" },
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ProcessAddVenmoForWindows,
            };
        }

        internal static void ProcessAddVenmoForWindows(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (featureContext == null || featureContext.PaymentMethodfamily != Constants.PaymentMethodFamilyNames.Ewallet || featureContext.PaymentMethodType != Constants.PaymentMethodTypeNames.Venmo)
            {
                return;
            }

            foreach (PIDLResource pidl in inputResources)
            {
                List<DisplayHint> venmoInstructionTextGroup = pidl.GetAllDisplayHintsOfId("venmoInstructionTextGroup");
                List<DisplayHint> microsoftPrivacyTextGroups = pidl.GetAllDisplayHintsOfId("microsoftPrivacyTextGroup");

                FeatureHelper.ConvertToGroupDisplayHint(venmoInstructionTextGroup, "inline");
                FeatureHelper.ConvertToGroupDisplayHint(microsoftPrivacyTextGroups, "inline");
                FeatureHelper.UpdateLogoUrl(pidl, logoUrls);

                FeatureHelper.PassStyleHints(pidl, styleHints, featureContext);
            }
        }
    }
}
