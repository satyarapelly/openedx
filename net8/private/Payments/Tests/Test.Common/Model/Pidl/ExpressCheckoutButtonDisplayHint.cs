// <copyright file="ExpressCheckoutButtonDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Tests.Common.Model.Pidl
{
    /// <summary>
    /// Model class for ExpressCheckoutButtonDisplayHint
    /// </summary>
    public sealed class ExpressCheckoutButtonDisplayHint : DisplayHint
    {
        [JsonProperty(PropertyName = "frameName")]
        public string FrameName { get; set; }

        [JsonProperty(PropertyName = "sourceUrl")]
        public string SourceUrl { get; set; }

        [JsonProperty(PropertyName = "messageTimeout")]
        public int MessageTimeout { get; set; }

        [JsonProperty(PropertyName = "payload")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, object> Payload { get; set; }
    }
}