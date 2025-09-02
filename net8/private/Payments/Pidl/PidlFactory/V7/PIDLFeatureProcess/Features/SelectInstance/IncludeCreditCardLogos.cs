// <copyright file="IncludeCreditCardLogos.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Class representing the IncludeCreditCardLogos, which includes defined credit logo urls in the data description.
    /// Mainly use for client side prefill scenario where we're populating list with local cards which don't have logo.
    /// </summary>
    internal class IncludeCreditCardLogos : IFeature
    {
        public List<Action<List<PIDLResource>, FeatureContext>> GetActions(FeatureContext featureContext)
        {
            return new List<Action<List<PIDLResource>, FeatureContext>>()
            {
                IncludeCreditCardLogosinDataDescription
            };
        }

        internal static void IncludeCreditCardLogosinDataDescription(List<PIDLResource> inputResources, FeatureContext featureContext)
        {
            if (string.Equals(featureContext.OperationType, V7.Constants.PidlOperationTypes.SelectInstance, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource pidlResource in inputResources)
                {
                    var cardLogoDescription = new PropertyDescription()
                    {
                        PropertyType = "userData",
                        PropertyDescriptionType = "object",
                        DataType = "object",
                        IsKey = false,
                        IsOptional = true,
                        IsUpdatable = false,
                        PossibleValues = new Dictionary<string, string>()
                        {
                            { Constants.PaymentMethodTypeNames.Visa, Constants.CreditCardLogoURLs.Visa },
                            { Constants.PaymentMethodTypeNames.MasterCard, Constants.CreditCardLogoURLs.Mastercard },
                            { Constants.PaymentMethodTypeNames.Amex, Constants.CreditCardLogoURLs.Amex },
                            { Constants.PaymentMethodTypeNames.Discover, Constants.CreditCardLogoURLs.Discover }
                        }
                    };

                    pidlResource.DataDescription.Add("cardLogos", cardLogoDescription);
                }
            }
        }
    }
}
