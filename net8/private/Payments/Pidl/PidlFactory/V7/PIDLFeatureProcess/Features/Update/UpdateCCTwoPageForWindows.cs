// <copyright file="UpdateCCTwoPageForWindows.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Used for reorganizing display content in Update CC
    /// </summary>
    internal class UpdateCCTwoPageForWindows : IFeature
    {
        private static Dictionary<string, Dictionary<string, IEnumerable<string>>> styleHints = new Dictionary<string, Dictionary<string, IEnumerable<string>>>()
        {
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
                "cancelNextGroup", new Dictionary<string, IEnumerable<string>>()
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
            }
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ProcessUpdateCCTwoPageForWindows,
            };
        }

        internal static void ProcessUpdateCCTwoPageForWindows(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (featureContext == null)
            {
                return;
            }

            foreach (PIDLResource pidl in inputResources)
            {
                List<DisplayHint> starRequiredTextGroups = pidl.GetAllDisplayHintsOfId("starRequiredTextGroup");
                List<DisplayHint> microsoftPrivacyTextGroups = pidl.GetAllDisplayHintsOfId("microsoftPrivacyTextGroup");

                FeatureHelper.ConvertToGroupDisplayHint(starRequiredTextGroups, "inline");
                FeatureHelper.ConvertToGroupDisplayHint(microsoftPrivacyTextGroups, "inline");

                FeatureHelper.PassStyleHints(pidl, styleHints, featureContext);
            }
        }
    }
}
