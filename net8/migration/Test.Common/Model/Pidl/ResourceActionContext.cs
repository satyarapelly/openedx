// <copyright file="ResourceActionContext.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// ResourceActionContext is a child of ActionContext, contains context the client needs to complete future actions.
    /// </summary>
    public class ResourceActionContext
    {
        private Dictionary<string, string> prefillData;

        public ResourceActionContext()
        {
        }

        public ResourceActionContext(ResourceActionContext template)
            : this(template.Action, template.PidlDocInfo, template.PidlIdentity, template.PrefillData, template.ResourceInfo)
        {
        }

        public ResourceActionContext(string actionIn, PidlDocInfo pidlDocInfoIn, PidlIdentity pidlIdentityIn, Dictionary<string, string> prefillDataIn, ResourceInfo resourceInfoIn)
        {
            this.Action = actionIn;
            this.PidlDocInfo = pidlDocInfoIn;
            this.PidlIdentity = pidlIdentityIn;
            this.ResourceInfo = resourceInfoIn;

            this.SetPrefillData(prefillDataIn);
        }

        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "pidlDocInfo")]
        public PidlDocInfo PidlDocInfo { get; set; }

        [JsonProperty(PropertyName = "pidlIdentity")]
        public PidlIdentity PidlIdentity { get; set; }

        [JsonProperty(PropertyName = "resourceInfo")]
        public ResourceInfo ResourceInfo { get; set; }

        [JsonProperty(PropertyName = "prefillData")]
        public Dictionary<string, string> PrefillData
        {
            get
            {
                return this.prefillData;
            }
        }

        public void SetPrefillData(Dictionary<string, string> prefillDataIn)
        {
            if (prefillDataIn != null)
            {
                this.prefillData = new Dictionary<string, string>(prefillDataIn);
            }
        }
    }
}
