// <copyright file="AuthenticateCommand.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using Newtonsoft.Json;

    public class AuthenticateCommand
    {
        [JsonProperty(PropertyName = "requestID")]
        public string RequestID { get; set; }

        [JsonProperty(PropertyName = "authenticationContext")]
        public object AuthenticationContext { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string AuthenticationType { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "contentType")]
        public string ContentType { get; set; }
    }
}
