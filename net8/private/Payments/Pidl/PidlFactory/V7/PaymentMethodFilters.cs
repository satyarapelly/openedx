// <copyright file="PaymentMethodFilters.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    public class PaymentMethodFilters
    {
        public PaymentMethodFilters()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed for serialization purpose.")]
        [JsonProperty(PropertyName = "exclusionTags")]
        public List<string> ExclusionTags { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed for serialization purpose.")]
        [JsonProperty(PropertyName = "chargeThresholds")]
        public List<decimal> ChargeThresholds { get; set; }

        [JsonProperty(PropertyName = "chargeThreshold")]
        public decimal? ChargeThreshold { get; set; }

        [JsonProperty(PropertyName = "splitPaymentSupported")]
        public bool? SplitPaymentSupported { get; set; }

        [JsonProperty(PropertyName = "filterExpiredPayment")]
        public bool? FilterExpiredPayment { get; set; }

        [JsonProperty(PropertyName = "filterPrepaidCards")]
        public bool? FilterPrepaidCards { get; set; }

        [JsonProperty(PropertyName = "filterPurchaseRedirectPayment")]
        public bool? FilterPurchaseRedirectPayment { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "backupId")]
        public string BackupId { get; set; }

        [JsonProperty(PropertyName = "isBackupPiOptional")]
        public bool? IsBackupPiOptional { get; set; }
    }
}