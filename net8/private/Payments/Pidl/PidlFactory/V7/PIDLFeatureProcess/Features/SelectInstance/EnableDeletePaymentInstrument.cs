// <copyright file="EnableDeletePaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;

    /// <summary>
    /// Represents the EnableDeletePaymentInstrument class, designed to display delete button to the list payment instrument.
    /// </summary>
    internal class EnableDeletePaymentInstrument : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                EnableDeleteButtonInListPI
            };
        }

        private static void EnableDeleteButtonInListPI(List<PIDLResource> inputResources, FeatureContext postProcessingParams)
        {
            foreach (PIDLResource resource in inputResources)
            {
                if (resource != null && resource.DisplayPages != null && string.Equals(postProcessingParams.OperationType, V7.Constants.PidlOperationTypes.SelectInstance, StringComparison.OrdinalIgnoreCase))
                {
                    PropertyDisplayHint selectHint = resource.GetDisplayHintById(V7.Constants.DisplayHintIds.PaymentInstrumentSelect) as PropertyDisplayHint;
                    if (selectHint != null && string.Equals(selectHint.SelectType, V7.Constants.PaymentMethodSelectType.DropDown, StringComparison.OrdinalIgnoreCase) && selectHint.PossibleOptions != null && selectHint.PossibleOptions.Count > 0)
                    {
                        selectHint.StyleHints = new List<string>() { "padding-horizontal-medium", "padding-vertical-small" };

                        foreach (SelectOptionDescription option in selectHint.PossibleOptions.Values)
                        {
                            ActionContext context = option?.PidlAction?.Context as ActionContext;
                            PaymentInstrument selectionInstance = context?.Instance as PaymentInstrument;
                            if (selectionInstance != null)
                            {
                                option.DisplayContent = new GroupDisplayHint
                                {
                                    HintId = V7.Constants.DisplayHintIdPrefixes.PaymentOptionContainer + selectionInstance.PaymentInstrumentId,
                                    LayoutOrientation = V7.Constants.PartnerHintsValues.InlinePlacement,
                                    DisplayTags = new Dictionary<string, string>()
                                    {
                                        { Constants.DisplayTag.PiContainer, Constants.DisplayTag.PiContainer }
                                    },  
                                };

                                var logoGroup = PaymentSelectionHelper.BuildLogoElementForSelectionInstance(selectionInstance);
                                logoGroup.StyleHints = null;
                                logoGroup.DisplayTags = new Dictionary<string, string>()
                                {
                                    { Constants.DisplayTag.ImageIcon, Constants.DisplayTag.ImageIcon }
                                };

                                option.DisplayContent.Members.Add(logoGroup);
                                option.DisplayContent.Members.Add(PaymentSelectionHelper.BuildGroupElementForSelectInstanceDeletePI(selectionInstance, postProcessingParams, option.DisplayText));
                                option.DisplayText = null;
                            }
                        }
                    }
                }
            }
        }
    }
}