// <copyright file="ActionContext.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl 
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// ActionContext is a child of DisplayHintAction, client returns this object to the partner when an action is completed.
    /// </summary>
    public class ActionContext
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "instance")]
        public object Instance { get; set; }

        [JsonProperty(PropertyName = "paymentInstrumentId")]
        public object PaymentInstrumentId { get; set; }

        [JsonProperty(PropertyName = "backupId")]
        public string BackupId { get; set; }

        [JsonProperty(PropertyName = "backupInstance")]
        public object BackupInstance { get; set; }

        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "paymentMethodFamily")]
        public string PaymentMethodFamily { get; set; }

        [JsonProperty(PropertyName = "paymentMethodType")]
        public string PaymentMethodType { get; set; }

        [JsonProperty(PropertyName = "resourceActionContext")]
        public ResourceActionContext ResourceActionContext { get; set; }

        [JsonProperty(PropertyName = "partnerHints")]
        public PartnerHints PartnerHints { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(PropertyName = "prefillData")]
        public Dictionary<string, string> PrefillData { get; set; }

        [JsonProperty(PropertyName = "targetIdentity")]
        public PidlIdentity TargetIdentity { get; set; }

        [JsonProperty(PropertyName = "isSelectPMSkipped")]
        public bool? IsSelectPMSkipped { get; set; }
    }
}
