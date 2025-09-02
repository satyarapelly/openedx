// <copyright file="LinkSession.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class LinkSession 
    {
        public LinkSession(string defaultSessionId)
        {
            this.DefaultSessionId = defaultSessionId;
        }

        [JsonProperty(PropertyName = "defaultSessionId")]
        public string DefaultSessionId { get; set; }
    }
}