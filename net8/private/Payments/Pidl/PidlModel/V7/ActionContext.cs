// <copyright file="ActionContext.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// ActionContext is a child of DisplayHintAction, client returns this object to the partner when an action is completed.
    /// </summary>
    public class ActionContext
    {
        private Dictionary<string, string> prefillData;

        public ActionContext()
        {
        }

        public ActionContext(string actionType, ResourceActionContext resourceActionContext)
        {
            this.Action = actionType;
            this.ResourceActionContext = resourceActionContext;
        }

        public ActionContext(ActionContext template)
        {
            this.Id = template.Id;
            this.Instance = template.Instance;
            this.PaymentInstrumentId = template.PaymentInstrumentId;
            this.BackupId = template.BackupId;
            this.BackupInstance = template.BackupInstance;
            this.Action = template.Action;
            this.PaymentMethodFamily = template.PaymentMethodFamily;
            this.PaymentMethodType = template.PaymentMethodType;
            this.ResourceActionContext = template.ResourceActionContext;
            this.PartnerHints = template.PartnerHints;
            this.TargetIdentity = template.TargetIdentity;

            this.SetPrefillData(template.prefillData);
        }

        public ActionContext(string propertyName, bool propertyValue)
        {
            this.PropertyName = propertyName;
            this.PropertyValue = propertyValue;
        }

        [JsonProperty(PropertyName = "propertyValue")]
        public bool? PropertyValue { get; set; }

        [JsonProperty(PropertyName = "propertyName")]
        public string PropertyName { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "instance")]
        public object Instance { get; set; }

        /// <summary>
        /// Gets or sets - Clone of Instance property to support List PI server side prefill with me/my-org PI's
        /// </summary>
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

        [JsonProperty(PropertyName = "isSelectPMSkipped")]
        public bool? IsSelectPMSkipped { get; set; }

        [JsonProperty(PropertyName = "resourceActionContext")]
        public ResourceActionContext ResourceActionContext { get; set; }

        [JsonProperty(PropertyName = "partnerHints")]
        public PartnerHints PartnerHints { get; set; }

        [JsonProperty(PropertyName = "prefillData")]
        public Dictionary<string, string> PrefillData
        {
            get
            {
                return this.prefillData;
            }
        }

        [JsonProperty(PropertyName = "targetIdentity")]
        public PidlIdentity TargetIdentity { get; set; }

        public void SetPrefillData(Dictionary<string, string> prefillDataIn)
        {
            if (prefillDataIn != null)
            {
                this.prefillData = new Dictionary<string, string>(prefillDataIn);
            }
        }
    }
}
