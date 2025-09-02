// <copyright file="PidlContainerDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a PidlContainer DisplayHint
    /// </summary>
    public class PidlContainerDisplayHint : DisplayHint
    {
        public const SubmissionOrder DefaultSubmitOrder = (SubmissionOrder)0;

        private Dictionary<string, string> linkedPidlIdentity;
        private SubmissionOrder submitOrder;

        public PidlContainerDisplayHint()
        {
        }

        public PidlContainerDisplayHint(PidlContainerDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
            this.IsMultiPage = template.IsMultiPage;
            this.SetLinkedPidlIdentity(template.LinkedPidlIdentity);
            this.submitOrder = DefaultSubmitOrder;
        }

        public enum SubmissionOrder
        {
            WithBase = 0, // default
            BeforeBase,
            AfterBase
        }

        [JsonProperty(PropertyName = "isMultiPage")]
        public bool? IsMultiPage { get; set; }

        [JsonProperty(PropertyName = "linkedPidlId")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, string> LinkedPidlIdentity
        {
            get
            {
                return this.linkedPidlIdentity;
            }

            set
            {
                this.linkedPidlIdentity = value;
            }
        }

        [JsonProperty(PropertyName = "submitOrder", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(CamelCaseStringEnumConverter))] // camelCase is used to fulfill the contract
        public SubmissionOrder SubmitOrder
        {
            get
            {
                return this.submitOrder;
            }

            set
            {
                this.submitOrder = value;
            }
        }

        public void SetLinkedPidlIdentity(Dictionary<string, string> identityIn)
        {
            if (identityIn != null)
            {
                this.linkedPidlIdentity = new Dictionary<string, string>(identityIn);
            }
        }

        protected override string GetDisplayType()
        {
            return HintType.PidlContainer.ToString().ToLower();
        }
    }
}