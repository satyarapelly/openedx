// <copyright file="UsageType.cs" company="Microsoft Corporation">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// UsageType for payment instrument.
    /// Inline indicates pi can be used one time only.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UsageType
    {
        OnFile = 0,
        Inline = 1,
    }
}
