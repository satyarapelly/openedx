// <copyright file="MoveToPageIndexActionContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using Newtonsoft.Json;

    public class MoveToPageIndexActionContext
    {
        [JsonProperty(PropertyName = "pageIndex")]
        public int PageIndex { get; set; }
    }
}