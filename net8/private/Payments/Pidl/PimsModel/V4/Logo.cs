// <copyright file="Logo.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System;
    using Newtonsoft.Json;

    public class Logo
    {
        [JsonProperty(PropertyName = "mimeType")]
        public string MimeType { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}