// <copyright file="RedirectionServiceLink.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;    
    
    public class RedirectionServiceLink
    {
        private Dictionary<string, string> ruparams = new Dictionary<string, string>();
        private Dictionary<string, string> rxparams = new Dictionary<string, string>();
        
        [JsonProperty(PropertyName = "baseUrl")]
        public string BaseUrl { get; set; }

        [JsonProperty(PropertyName = "successParams")]
        public Dictionary<string, string> RuParameters 
        { 
            get 
            {
                return this.ruparams; 
            } 
        }

        [JsonProperty(PropertyName = "failureParams")]
        public Dictionary<string, string> RxParameters 
        { 
            get 
            {
                return this.rxparams; 
            } 
        }

        [JsonProperty(PropertyName = "noCallbackParams")]
        public bool NoCallbackParams { get; set; } = false;
    }
}
