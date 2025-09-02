// <copyright file="PaymentMethod.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class PaymentMethod : PaymentMethodBase
    {
        [JsonProperty(PropertyName = "paymentMethodType")]
        public string PaymentMethodType { get; set; }

        [JsonProperty(PropertyName = "properties")]
        public PaymentMethodCapabilities Properties { get; set; }

        [JsonProperty(PropertyName = "paymentMethodGroup")]
        public string PaymentMethodGroup { get; set; }

        [JsonProperty(PropertyName = "groupDisplayName")]
        public string GroupDisplayName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed for serialization purpose.")]
        [JsonProperty(PropertyName = "exclusionTags")]
        public List<string> ExclusionTags { get; set; }

        [JsonIgnore]
        public string PaymentMethodId { get; set; }

        public bool EqualByFamilyAndType(PaymentMethod other)
        {
            return this.PaymentMethodFamily.Equals(other.PaymentMethodFamily)
                && this.PaymentMethodType.Equals(other.PaymentMethodType);
        }

        public bool EqualByFamilyAndType(string family, string type)
        {
            return this.PaymentMethodFamily.Equals(family, StringComparison.OrdinalIgnoreCase)
                && this.PaymentMethodType.Equals(type, StringComparison.OrdinalIgnoreCase);
        }
    }
}