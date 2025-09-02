// <copyright file="SetButtonActionToSuccessType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;

    /// <summary>
    /// Class representing the RemoveNonSuccessActionForButton, which is to remove any action type that is not success for button and make the action type to always be success for button in buttonList PM select form
    /// </summary>
    internal class SetButtonActionToSuccessType : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                RemoveResourceActionContext
            };
        }

        private static void RemoveResourceActionContext(List<PIDLResource> inputResources, FeatureContext postProcessingParams)
        {
            foreach (PIDLResource resource in inputResources)
            {
                if (resource != null && resource.DisplayPages != null)
                {
                    PropertyDisplayHint selectHint = resource.GetDisplayHintById("paymentMethod") as PropertyDisplayHint;
                    if (selectHint != null && selectHint.SelectType.Equals(V7.Constants.PaymentMethodSelectType.ButtonList) && selectHint.PossibleOptions != null && selectHint.PossibleOptions.Count > 0)
                    {
                        Dictionary<string, SelectOptionDescription> newPossibleOptions = new Dictionary<string, SelectOptionDescription>();
                        foreach (KeyValuePair<string, SelectOptionDescription> option in selectHint.PossibleOptions)
                        {
                            SelectOptionDescription newOption = new SelectOptionDescription { DisplayText = option.Value.DisplayText };
                            ActionContext context = option.Value.PidlAction.Context as ActionContext;
                            newOption.PidlAction = new DisplayHintAction(
                                "success",
                                false,
                                new ActionContext
                                {
                                    Id = context.Id,
                                    PaymentMethodFamily = context.PaymentMethodFamily,
                                    PaymentMethodType = context.PaymentMethodType
                                },
                                null);
                            newOption.AccessibilityTag = option.Value.AccessibilityTag;

                            newPossibleOptions.Add(option.Key, newOption);
                        }

                        selectHint.SetPossibleOptions(newPossibleOptions);
                    }
                }
            }
        }
    }
}