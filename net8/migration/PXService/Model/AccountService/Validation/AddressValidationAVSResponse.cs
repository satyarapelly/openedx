// <copyright file="AddressValidationAVSResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.AccountService.AddressValidation
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class AddressValidationAVSResponse
    {
        [JsonProperty(PropertyName = "original_address")]
        public object OriginalAddress { get; set; }

        [JsonProperty(PropertyName = "suggested_address")]
        public object SuggestedAddress { get; set; }

        [JsonProperty(PropertyName = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public AddressAVSValidationStatus Status { get; set; }

        [JsonProperty(PropertyName = "validation_message")]
        public string ValidationMessage { get; set; }
    }
}