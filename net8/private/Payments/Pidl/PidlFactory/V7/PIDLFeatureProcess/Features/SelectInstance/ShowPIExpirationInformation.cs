// <copyright file="ShowPIExpirationInformation.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;

    /// <summary>
    /// Class representing the ShowPIExpirationInformation, which is to show expiration alert for expired card in list pi dropdown.
    /// </summary>
    internal class ShowPIExpirationInformation : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ShowPIExpirationInformationInSelectOption
            };
        }

        private static void ShowPIExpirationInformationInSelectOption(List<PIDLResource> inputResources, FeatureContext postProcessingParams)
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
                            ActionContext context = option.PidlAction.Context as ActionContext;
                            PaymentInstrument selectionInstance = context.Instance as PaymentInstrument;
                            if (selectionInstance != null)
                            {
                                option.DisplayContent = new GroupDisplayHint
                                {
                                    HintId = V7.Constants.DisplayHintIdPrefixes.PaymentOptionContainer + selectionInstance.PaymentInstrumentId,
                                    LayoutOrientation = V7.Constants.PartnerHintsValues.InlinePlacement,
                                    StyleHints = new List<string>() { "gap-small", "align-vertical-center" },
                                };
                                
                                option.DisplayContent.Members.Add(PaymentSelectionHelper.BuildLogoElementForSelectionInstance(selectionInstance));
                                option.DisplayContent.Members.Add(PaymentSelectionHelper.BuildTextGroupElementForSelectionInstance(selectionInstance, postProcessingParams));
                                RemoveUnneededSelectionOptionFields(option);
                            }
                        }
                    }
                }
            }
        }

        private static void RemoveUnneededSelectionOptionFields(SelectOptionDescription option)
        {
            option.DisplayText = null;
            option.DisplayImageUrl = null;
        }
    }
}