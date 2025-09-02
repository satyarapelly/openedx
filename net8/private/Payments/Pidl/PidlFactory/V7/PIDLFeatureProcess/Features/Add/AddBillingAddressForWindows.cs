// <copyright file="AddBillingAddressForWindows.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Used for reorganizing display content in Add Billing Address
    /// </summary>
    internal class AddBillingAddressForWindows : IFeature
    {
        // Key: hintId
        // Value: Dictionary where country is the key and the value is a list of style hints. Better to have country as key as there might be a case where a country has a different style hint than the default.
        private static Dictionary<string, Dictionary<string, IEnumerable<string>>> styleHints = new Dictionary<string, Dictionary<string, IEnumerable<string>>>()
        {
            {
                "profileAddressPageSubheading", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-bottom-x-small" } }
                }
            },
            {
                "addressState", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-triquarter" } }
                }
            },
            {
                "addressPostalCode", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-quarter" } }
                }
            },
            {
                "starRequiredTextGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-vertical-small" } }
                }
            },
            {
                "starLabel", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "text-alert" } }
                }
            },
            {
                "microsoftPrivacyTextGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "display-textgroup" } }
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
                "cancelNextGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "gap-small", "anchor-bottom" } }
                }
            },
            {
                "addressSummaryGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-vertical-small" } }
                }
            },
            {
                "paymentChangeButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-quarter" } }
                }
            },
            {
                "newProfileAddressText", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-top-small" } }
                }
            },
            {
                "paymentChangeSettingsStaticText", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-end-2x-small" } }
                }
            },
            {
                "saveButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-half" } }
                }
            },
            {
                "cancelSaveGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "gap-small", "anchor-bottom" } }
                }
            },
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ProcessAddBillingAddressForWindows,
            };
        }

        internal static void ProcessAddBillingAddressForWindows(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (featureContext == null || !Constants.PidlOperationTypes.Add.Equals(featureContext.OperationType, StringComparison.OrdinalIgnoreCase) || !Constants.ResourceTypes.Address.Equals(featureContext.ResourceType, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            foreach (PIDLResource pidl in inputResources)
            {
                if (pidl?.DisplayPages == null)
                {
                    continue;
                }

                List<DisplayHint> microsoftPrivacyTextGroup = pidl.GetAllDisplayHintsOfId(Constants.DisplayHintIds.MicrosoftPrivacyTextGroup);
                List<DisplayHint> paymentChangeSettingsTextGroup = pidl.GetAllDisplayHintsOfId(Constants.DisplayHintIds.PaymentChangeSettingsTextGroup);
                List<DisplayHint> starRequiredTextGroups = pidl.GetAllDisplayHintsOfId(Constants.DisplayHintIds.StarRequiredTextGroup);

                FeatureHelper.ConvertToGroupDisplayHint(microsoftPrivacyTextGroup, Constants.PartnerHintsValues.InlinePlacement);
                FeatureHelper.ConvertToGroupDisplayHint(paymentChangeSettingsTextGroup, Constants.PartnerHintsValues.InlinePlacement);
                FeatureHelper.ConvertToGroupDisplayHint(starRequiredTextGroups, Constants.PartnerHintsValues.InlinePlacement);

                FeatureHelper.PassStyleHints(pidl, styleHints, featureContext);
            }
        }
    }
}
