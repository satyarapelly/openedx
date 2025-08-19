// <copyright file="Logo.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pims
{
    using Newtonsoft.Json;

    public class Logo
    {
        [JsonProperty(PropertyName = "mimeType")]
        public string MimeType { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}