// <copyright file="ApplyRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService.Model.IssuerService
{
    using Newtonsoft.Json;

    public class ApplyRequest
    {
        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }
    }
}