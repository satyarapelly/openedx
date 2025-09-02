// <copyright file="PidlContainerDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a PidlContainer DisplayHint
    /// </summary>
    public class PidlContainerDisplayHint : DisplayHint
    {
        public enum SubmissionOrder
        {
            WithBase = 0, // default
            BeforeBase,
            AfterBase
        }

        [JsonProperty(PropertyName = "isMultiPage")]
        public bool? IsMultiPage { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(PropertyName = "linkedPidlId")]
        public Dictionary<string, string> LinkedPidlIdentity { get; set; }

        [JsonProperty(PropertyName = "submitOrder", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(CamelCaseStringEnumConverter))] // camelCase is used to fulfill the contract
        public SubmissionOrder SubmitOrder { get; set; }
    }
}