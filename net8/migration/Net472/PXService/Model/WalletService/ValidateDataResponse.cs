// <copyright file="ValidateDataResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.WalletService
{
    using Newtonsoft.Json;

    public class ValidateDataResponse
    {
        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }
    }
}