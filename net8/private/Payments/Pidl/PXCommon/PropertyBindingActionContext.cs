// <copyright file="PropertyBindingActionContext.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXCommon
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class PropertyBindingActionContext
    {
        private Dictionary<string, object> actionMap;

        [JsonProperty(PropertyName = "bindingPropertyName")]
        public string BindingPropertyName { get; set; }

        [JsonProperty(PropertyName = "actionMap")]
        public Dictionary<string, object> ActionMap
        {
            get
            {
                return this.actionMap;
            }
        }

        public void AddActionItem(string key, object action)
        {
            if (this.actionMap == null)
            {
                this.actionMap = new Dictionary<string, object>();
            }

            this.actionMap.Add(key, action);
        }
    }
}