// <copyright file="SessionResource.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.SessionService
{
    using Newtonsoft.Json;

    public class SessionResource
    {
        public SessionResource()
        {
            this.State = "INCOMPLETE";
        }

        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "SessionType")]
        public SessionType SessionType { get; set; }

        [JsonProperty(PropertyName = "Data")]
        public string Data { get; set; }

        [JsonProperty(PropertyName = "EncryptData")]
        public bool EncryptData { get; set; }

        [JsonProperty(PropertyName = "Result")]
        public string Result { get; set; }

        [JsonProperty(PropertyName = "State")]
        public string State { get; set; }
    }
}
