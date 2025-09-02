// <copyright file="PimsSessionStatus.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PimsSessionStatus
    {
        Created,
        InProgress,        
        Success,
        Failed,
        Expired
    }
}
