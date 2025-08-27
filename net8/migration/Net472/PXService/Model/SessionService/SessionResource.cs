// <copyright file="SessionResource.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.SessionService
{
    using Common.Transaction;
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

        [JsonProperty(PropertyName = "TestContext")]
        public TestContext TestContext { get; set; }
    }
}