// <copyright file="ThreeDSMethodCompletionIndicator.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ThreeDSMethodCompletionIndicator 
    {
        [EnumMember(Value = "Y")]
        Y,

        [EnumMember(Value = "N")]
        N,

        [EnumMember(Value = "U")]
        U,
    }
}