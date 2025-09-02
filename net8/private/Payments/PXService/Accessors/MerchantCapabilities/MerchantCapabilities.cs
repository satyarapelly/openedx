// <copyright file="MerchantCapabilities.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.MerchantCapabilitiesService.V7
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class MerchantCapabilities
    {
        [JsonProperty(PropertyName = "merchantId")]
        public string MerchantId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:Collection properties should be read only", Justification = "Needs to be writeable so the JSON serializer can run")]
        [JsonProperty(PropertyName = "paymentMethods")]
        public IList<MerchantCapabilitiesPaymentMethod> PaymentMethods { get; set; }

        public IDictionary<string, IList<string>> PaymentMethodsPerFamily
        {
            get
            {
                IDictionary<string, IList<string>> paymentMethodsPerFamily = new Dictionary<string, IList<string>>();

                foreach (MerchantCapabilitiesPaymentMethod paymentMethod in this.PaymentMethods)
                {
                    if (!paymentMethodsPerFamily.ContainsKey(paymentMethod.PaymentMethodFamily))
                    {
                        paymentMethodsPerFamily.Add(paymentMethod.PaymentMethodFamily, new List<string>());
                    }

                    paymentMethodsPerFamily[paymentMethod.PaymentMethodFamily].Add(paymentMethod.PaymentMethodType);
                }

                return paymentMethodsPerFamily;
            }
        }
    }
}