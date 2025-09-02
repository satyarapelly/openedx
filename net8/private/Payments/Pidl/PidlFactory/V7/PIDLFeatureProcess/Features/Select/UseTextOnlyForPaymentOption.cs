// <copyright file="UseTextOnlyForPaymentOption.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the UseTextOnlyForPaymentOption, which is to only use displayText in the select option (by removing displayContent) for PIDLSDK JS or React classic element factory which couldn't render displayContent properly.
    /// </summary>
    internal class UseTextOnlyForPaymentOption : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                UseTextOnlyForOption
            };
        }

        private static void UseTextOnlyForOption(List<PIDLResource> inputResources, FeatureContext postProcessingParams)
        {
            foreach (PIDLResource resource in inputResources)
            {
                if (resource != null && resource.DisplayPages != null)
                {
                    PropertyDisplayHint selectHint = resource.GetDisplayHintById(Constants.DisplayHintIds.PaymentMethodSelect) as PropertyDisplayHint;
                    if (selectHint != null && selectHint.PossibleOptions != null && selectHint.PossibleOptions.Count > 0)
                    {
                        foreach (KeyValuePair<string, SelectOptionDescription> option in selectHint.PossibleOptions)
                        {
                            option.Value.DisplayContent = null;
                        }
                    }
                }
            }
        }
    }
}