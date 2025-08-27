// <copyright file="DeviceChannel.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService
{
    using Common.Web;
    using Newtonsoft.Json;

    [JsonConverter(typeof(EnumJsonConverter))]
    public enum DeviceChannel
    {
        AppBased,
        Browser,
        ThreeDSRequestorInit
    }
}