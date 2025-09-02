// <copyright file="AddRedeemGiftCardButton.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the AddRedeemGiftCardButton, which is to add a "Redeem a gift card" button.
    /// </summary>
    internal class AddRedeemGiftCardButton : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                AddRedeemGiftCardButtonAction
            };
        }

        internal static void AddRedeemGiftCardButtonAction(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (string.Equals(featureContext.OperationType, V7.Constants.PidlOperationTypes.SelectInstance, StringComparison.OrdinalIgnoreCase))
            {
                DisplayHintAction pidlAction = new DisplayHintAction
                {
                    ActionType = DisplayHintActionType.success.ToString(),
                    IsDefault = false,
                    Context = new ActionContext
                    {
                        Instance = Constants.Instances.RedeemGiftCardLink,
                        Id = Constants.PaymentMethodFamilyNames.Ewallet + "." + Constants.PaymentMethodTypeNames.StoredValue,
                        PaymentMethodFamily = Constants.PaymentMethodFamilyNames.Ewallet,
                        PaymentMethodType = Constants.PaymentMethodTypeNames.StoredValue,
                    }
                };

                foreach (PIDLResource pidlResource in inputResources)
                {
                    foreach (var page in pidlResource.DisplayPages)
                    {
                        var pmLinkIndex = page.Members.FindIndex(m => m.HintId == Constants.DisplayHintIds.NewPaymentMethodLink);
                        if (pmLinkIndex >= 0)
                        {
                            ButtonDisplayHint buttonDisplayHint = new ButtonDisplayHint
                            {
                                Action = pidlAction,
                                DisplayContent = Constants.PaymentMethodOptionStrings.RedeemGiftCard,
                                HintId = Constants.DisplayHintIds.RedeemGiftCardLink
                            };

                            // Insert the new button after the "New Payment Method" link
                            page.Members.Insert(pmLinkIndex + 1, buttonDisplayHint);
                        }
                    }
                }
            }
        }
    }
}