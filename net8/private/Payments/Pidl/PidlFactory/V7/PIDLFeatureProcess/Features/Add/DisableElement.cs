// <copyright file="DisableElement.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;

    /// <summary>
    /// Class representing the DisableElement feature, which is to show avs for add PI/address form.
    /// </summary>
    internal class DisableElement : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                DisableElementInPidl
            };
        }

        internal static void DisableElementInPidl(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            FeatureConfig featureConfig;
            featureContext.FeatureConfigs.TryGetValue(FeatureConfiguration.FeatureNames.DisableElement, out featureConfig);

            if (featureConfig != null && featureConfig.DisplayCustomizationDetail != null && featureConfig.DisplayCustomizationDetail.Count > 0)
            {
                foreach (DisplayCustomizationDetail displayHintCustomizationDetail in featureConfig.DisplayCustomizationDetail)
                {
                    foreach (PIDLResource pidlResource in inputResources)
                    {
                        if (displayHintCustomizationDetail?.DisableSelectPiRadioOption != null && bool.Parse(displayHintCustomizationDetail?.DisableSelectPiRadioOption.ToString()))
                        {
                            PropertyDisplayHint paymentInstrumentList = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.PaymentInstrumentSelect) as PropertyDisplayHint;

                            if (paymentInstrumentList != null)
                            {
                                foreach (var possibleOption in paymentInstrumentList.PossibleOptions)
                                {
                                    ActionContext context = possibleOption.Value.PidlAction.Context as ActionContext;
                                    if (context?.ResourceActionContext != null)
                                    {
                                        possibleOption.Value.IsDisabled = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}