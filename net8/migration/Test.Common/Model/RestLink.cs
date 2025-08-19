// <copyright file="RestLink.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class RestLink
    {
        private Dictionary<string, string> headers;
        private List<string> errorCodeExpressions;

        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "propertyName")]
        public string PropertyName { get; set; }

        [JsonProperty(PropertyName = "payload")]
        public object Payload { get; set; }

        [JsonProperty(PropertyName = "headers")]
        public Dictionary<string, string> Headers
        {
            get
            {
                return this.headers;
            }

            set
            {
                this.headers = Headers;
            }
        }

        [JsonProperty(PropertyName = "errorCodeExpressions")]
        public List<string> ErrorCodeExpressions
        {
            get
            {
                return this.errorCodeExpressions;
            }
        }

        public void AddHeader(string key, string value)
        {
            if (this.Headers == null)
            {
                this.headers = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            }

            this.headers.Add(key, value);
        }

        public void SetErrorCodeExpressions(string[] expressions)
        {
            this.errorCodeExpressions = new List<string>(expressions);
        }
    }
}
