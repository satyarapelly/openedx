// <copyright file="AddPMButtonWithPlusIcon.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the AddPMButtonWithPlusIcon, which is to add plus icon to the "Add a new payment method" button.
    /// </summary>
    internal class AddPMButtonWithPlusIcon : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                AddPlusIconToAddPMButton
            };
        }

        internal static void AddPlusIconToAddPMButton(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (string.Equals(featureContext.OperationType, V7.Constants.PidlOperationTypes.SelectInstance, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource pidlResource in inputResources)
                {                    
                    PropertyDisplayHint paymentInstrument = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentInstrumentSelect) as PropertyDisplayHint;

                    if (paymentInstrument != null && paymentInstrument.PossibleOptions.ContainsKey(Constants.DisplayHintIds.NewPaymentMethodLink))
                    {                        
                        DisplayHint newPaymentMethodLinkDisplayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHints(Constants.TemplateName.ListPiDropDown, Constants.DisplayHintIds.NewPaymentMethodLink, featureContext.Country, featureContext.OperationType, null).FirstOrDefault();

                        if (newPaymentMethodLinkDisplayHint != null)
                        {
                            string displayText = paymentInstrument.PossibleOptions[Constants.DisplayHintIds.NewPaymentMethodLink].DisplayText;
                            DisplayHintAction pidlAction = paymentInstrument.PossibleOptions[Constants.DisplayHintIds.NewPaymentMethodLink].PidlAction;

                            SelectOptionDescription newPaymentMethodLink = paymentInstrument.PossibleOptions[Constants.DisplayHintIds.NewPaymentMethodLink];
                            newPaymentMethodLink.DisplayText = Constants.DisplayHintIds.NewPaymentMethodLink;
                            newPaymentMethodLink.DisplayContent = new GroupDisplayHint { HintId = V7.Constants.DisplayHintIdPrefixes.PaymentOptionContainer + Constants.DisplayHintIds.NewPaymentMethodLink };

                            ButtonDisplayHint buttonDisplayHint = new ButtonDisplayHint
                            {
                                Action = pidlAction,
                                DisplayContent = displayText,
                                HintId = Constants.DisplayHintIds.NewPaymentMethodLink,
                                StyleHints = newPaymentMethodLinkDisplayHint.StyleHints,
                                DisplayHintType = newPaymentMethodLinkDisplayHint.DisplayHintType
                            };

                            buttonDisplayHint.AddDisplayTags(newPaymentMethodLinkDisplayHint.DisplayTags);
                            newPaymentMethodLink.DisplayContent.Members.Add(buttonDisplayHint);
                        }
                    }
                }
            }
        }
    }
}