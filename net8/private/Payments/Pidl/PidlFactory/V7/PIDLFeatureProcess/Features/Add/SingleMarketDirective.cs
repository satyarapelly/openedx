// <copyright file="SingleMarketDirective.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing SingleMarketDirective, which is a legal requirement from the European Union that is applicable to all online merchants who sell products and services to EU customers.
    /// </summary>
    internal class SingleMarketDirective : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                ApplySingleMarketDirective
            };
        }

        internal static void ApplySingleMarketDirective(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (featureContext.XMSFlightlHeader != null && featureContext.XMSFlightlHeader.Contains(Constants.PartnerFlightValues.SMDDisabled, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            foreach (PIDLResource paymentMethodPidl in inputResources)
            {
                List<string> singleMarkets = featureContext.SmdMarkets;
                if (singleMarkets != null
                     && singleMarkets.Count > 0
                     && singleMarkets.Contains(featureContext.Country?.ToLower())
                     && string.Equals(featureContext.PaymentMethodfamily, Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase)
                     && string.Equals(featureContext.OperationType, Constants.PidlOperationTypes.Add, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (PIDLResource resource in inputResources)
                    {
                        resource.SetPropertyState(Constants.DataDescriptionPropertyNames.Country, true);
                        resource.UpdateDisplayHintPossibleOptions(Constants.DataDescriptionPropertyNames.Country, singleMarkets);
                    }
                }
            }
        }
    }
}
