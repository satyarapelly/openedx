// <copyright file="PaymentMethodPayoutCapabilities.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class PaymentMethodPayoutCapabilities
    {
        [JsonProperty(PropertyName = "payout_thresholds")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed for serialization purpose.")]
        public List<PayoutThreshold> PayoutThresholds { get; set; }

        [JsonProperty(PropertyName = "payout_threshold_file")]
        public string PayoutThresholdFile { get; set; }

        [JsonProperty(PropertyName = "supported_operations")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed for serialization purpose.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Needed for serialization purpose.")]
        public List<string> SupportedOperations { get; set; }
    }
}