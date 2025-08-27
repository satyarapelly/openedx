// <copyright file="EligiblePaymentMethodsRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System;
    using System.Collections.Generic;

    public class EligiblePaymentMethodsRequest
    {
        private Dictionary<PaymentMethodType, PimsModel.V4.PaymentMethod> paymentMethodsMap = new Dictionary<PaymentMethodType, PimsModel.V4.PaymentMethod>
        {
            { PaymentMethodType.Visa, new PimsModel.V4.PaymentMethod { PaymentMethodFamily = "credit_card", PaymentMethodType = "visa" } },
            { PaymentMethodType.Amex, new PimsModel.V4.PaymentMethod { PaymentMethodFamily = "credit_card", PaymentMethodType = "amex" } },
            { PaymentMethodType.Mc, new PimsModel.V4.PaymentMethod { PaymentMethodFamily = "credit_card", PaymentMethodType = "mc" } },
            { PaymentMethodType.Discover, new PimsModel.V4.PaymentMethod { PaymentMethodFamily = "credit_card", PaymentMethodType = "discover" } },
            { PaymentMethodType.ApplePay, new PimsModel.V4.PaymentMethod { PaymentMethodFamily = "ewallet", PaymentMethodType = "applepay" } },
            { PaymentMethodType.GooglePay, new PimsModel.V4.PaymentMethod { PaymentMethodFamily = "ewallet", PaymentMethodType = "googlepay" } }
        };

        public EligiblePaymentMethodsRequest()
        {
        }

        public EligiblePaymentMethodsRequest(string country, decimal amount, List<PaymentMethodType> allowedPaymentMethods)
        {
            this.Countries = new List<string> { country };
            this.Amount = Convert.ToString(amount);
            this.AllowedPaymentMethods = this.GetPaymentMethods(allowedPaymentMethods);
        }        

        public List<string> Countries { get; } = new List<string>();

        public string Amount { get; set; }

        public string Currency { get; set; }

        public List<PimsModel.V4.PaymentMethod> AllowedPaymentMethods { get; } = new List<PimsModel.V4.PaymentMethod>();

        // Comma separated values
        public string IncludeSections { get; set; }

        public List<PimsModel.V4.PaymentMethod> GetPaymentMethods(IList<PaymentMethodType> paymentMethodTypes)
        {
            var paymentMethods = new List<PimsModel.V4.PaymentMethod>();

            foreach (var paymentMethodType in paymentMethodTypes)
            {
                if (this.paymentMethodsMap.TryGetValue(paymentMethodType, out var paymentMethod))
                {
                    paymentMethods.Add(paymentMethod);
                }
                else
                {
                    // TO DO: Log if the key is not found
                }
            }

            return paymentMethods;
        }       
    }
}