// <copyright file="ThreeDSServerAuthenticationResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using Newtonsoft.Json;

    public class ThreeDSServerAuthenticationResponse
    {
        [JsonProperty(PropertyName = "threeDSServerTransID")]
        public string ThreeDSServerTransactionID { get; set; }

        [JsonProperty(PropertyName = "acsTransID")]
        public string AcsTransactionID { get; set; }

        [JsonProperty(PropertyName = "acsSignedContent")]
        public string AcsSignedContent { get; set; }

        [JsonProperty(PropertyName = "messageVersion")]
        public string MessageVersion { get; set; }
    }
}