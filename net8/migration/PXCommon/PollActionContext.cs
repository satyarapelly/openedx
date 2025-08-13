// <copyright file="PollActionContext.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXCommon
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class PollActionContext : RestLink
    {
        private Dictionary<string, object> responseActions;

        [JsonProperty(PropertyName = "responseResultExpression")]
        public string ResponseResultExpression { get; set; }

        [JsonProperty(PropertyName = "interval")]
        public int Interval { get; set; }

        [JsonProperty(PropertyName = "maxPollingAttempts")]
        public int MaxPollingAttempts { get; set; }

        [JsonProperty(PropertyName = "checkPollingTimeOut")]
        public bool CheckPollingTimeOut { get; set; }

        [JsonProperty(PropertyName = "responseActions")]
        public Dictionary<string, object> ResponseActions
        {
            get
            {
                return this.responseActions;
            }
        }

        public void AddResponseActionsItem(string key, object value)
        {
            if (this.responseActions == null)
            {
                this.responseActions = new Dictionary<string, object>();
            }

            this.responseActions.Add(key, value);
        }
    }
}
