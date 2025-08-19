// <copyright file="PaymentMethodCapabilities.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pims
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class PaymentMethodCapabilities
    {
        public PaymentMethodCapabilities()
        {
            this.SupportedOperations = new List<string>();
        }

        [JsonProperty(PropertyName = "offlineRecurring")]
        public bool OfflineRecurring { get; set; }

        [JsonProperty(PropertyName = "userManaged")]
        public bool UserManaged { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed for serialization purpose.")]
        [JsonProperty(PropertyName = "chargeThresholds")]
        public List<PayinCap> ChargeThreshold { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed for serialization purpose.")]
        [JsonProperty(PropertyName = "redirectRequired")]
        public List<string> RedirectRequired { get; set; }

        [JsonProperty(PropertyName = "soldToAddressRequired")]
        public bool SoldToAddressRequired { get; set; }

        [JsonProperty(PropertyName = "splitPaymentSupported")]
        public bool SplitPaymentSupported { get; set; }

        [JsonProperty(PropertyName = "supportedOperations")]
        public List<string> SupportedOperations { get; private set; }

        [JsonProperty(PropertyName = "taxable")]
        public bool Taxable { get; set; }

        [JsonProperty(PropertyName = "providerRemittable")]
        public bool ProviderRemittable { get; set; }

        [JsonProperty(PropertyName = "providerCountry")]
        public string ProviderCountry { get; set; }

        [JsonProperty(PropertyName = "nonStoredPaymentMethodId")]
        public string NonStoredPaymentMethodId { get; set; }

        [JsonProperty(PropertyName = "isNonStoredPaymentMethod")]
        public bool IsNonStoredPaymentMethod { get; set; }

        public void AddOperation(string operation)
        {
            this.SupportedOperations.Add(operation);
        }
    }
}