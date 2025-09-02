// <copyright file="RedeemGiftCard.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Used for reorganizing display content in Redeem Gift Card
    /// </summary>
    internal class RedeemGiftCard : IFeature
    {
        // Key: hintId
        // Value: Dictionary where country is the key and the value is a list of style hints. Better to have country as key as there might be a case where a country has a different style hint than the default.
        private static Dictionary<string, Dictionary<string, IEnumerable<string>>> styleHints = new Dictionary<string, Dictionary<string, IEnumerable<string>>>()
        {
            {
                "cancelSaveNextGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "padding-top-small", "gap-medium", "dock-bottom" } }
                }
            },
            {
                "cancelButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill" } }
                }
            },
            {
                "cancelSaveConfirmGroup", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "padding-top-small", "gap-medium", "dock-bottom" } }
                }
            },
            {
                "saveNextButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill" } }
                }
            },
            {
                "csvRedeemLikeThisText", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "text-subtext" } }
                }
            },
            {
                "saveConfirmButton", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "width-fill" } }
                }
            },
            {
                "csvLogo", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "image-medium" } }
                }
            },
            {
                "csvAmountText", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-top-medium" } }
                }
            },
            {
                "giftCardToken", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "text-subtext" } }
                }
            },
            {
                "csvConfirmRedeemText1", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-end-2x-small" } }
                }
            },
            {
                "csvConfirmRedeemText2", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-end-2x-small" } }
                }
            },
            {
                "csvConfirmRedeemText3", new Dictionary<string, IEnumerable<string>>()
                {
                    { string.Empty, new List<string> { "margin-end-2x-small" } }
                }
            }
        };

        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ProcessRedeemGiftCard,
            };
        }

        internal static void ProcessRedeemGiftCard(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (featureContext == null || featureContext.PaymentMethodfamily != Constants.PaymentMethodFamilyNames.Ewallet || featureContext.PaymentMethodType != Constants.PaymentMethodTypeNames.StoredValue)
            {
                return;
            }

            foreach (PIDLResource pidl in inputResources)
            {
                FeatureHelper.PassStyleHints(pidl, styleHints, featureContext);
            }
        }
    }
}
