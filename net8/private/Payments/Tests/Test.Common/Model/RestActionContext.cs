// <copyright file="RestActionContext.cs" company="Microsoft Corporation">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model
{
    using Newtonsoft.Json;

    public class RestActionContext : RestLink
    {
        /// <summary>
        /// Gets or sets a value indicating whether PIDLSDK should invoke success event for the response from restAction.
        /// </summary>
        [JsonProperty(PropertyName = "shouldHandleSuccess")]
        public bool ShouldHandleSuccess { get; set; }
    }
}
