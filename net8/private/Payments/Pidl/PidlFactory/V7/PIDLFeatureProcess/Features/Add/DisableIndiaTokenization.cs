// <copyright file="DisableIndiaTokenization.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Disables India Tokenization 
    /// </summary>
    internal class DisableIndiaTokenization : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                RemoveTokenizationElements
            };
        }

        internal static void RemoveTokenizationElements(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            string[] tokenizationDisplayDescriptions = { Constants.TextDisplayHintIds.IndiaTokenConsentMessage, Constants.ButtonDisplayHintIds.IndiaTokenConsentMessageHyperlink };

            if (string.Equals(featureContext.PaymentMethodfamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource paymentMethodPidl in inputResources)
                {
                    foreach (string displayHintId in tokenizationDisplayDescriptions)
                    {
                        if (paymentMethodPidl.GetDisplayHintById(displayHintId) != null)
                        {
                            paymentMethodPidl.RemoveDisplayHintById(displayHintId);
                        }
                    }

                    if (paymentMethodPidl.DataDescription.ContainsKey(Constants.DataDescriptionVariableNames.Details))
                    {
                        List<PIDLResource> dataDescriptionDetails = (List<PIDLResource>)paymentMethodPidl.DataDescription[Constants.DataDescriptionVariableNames.Details];
                        if (dataDescriptionDetails[0].DataDescription.TryGetValue(Constants.DataDescriptionPropertyNames.TokenizationConsent, out object tokenizationConsent)
                            && tokenizationConsent != null)
                        {
                            dataDescriptionDetails[0].DataDescription.Remove(Constants.DataDescriptionPropertyNames.TokenizationConsent);
                        }
                    }
                }
            }
        }
    }
}