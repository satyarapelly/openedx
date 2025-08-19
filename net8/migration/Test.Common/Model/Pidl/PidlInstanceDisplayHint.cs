// <copyright file="PidlInstanceDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

using Newtonsoft.Json;

namespace Tests.Common.Model.Pidl
{
    /// <summary>
    /// Class for describing a Text DisplayHint
    /// </summary>
    public sealed class PidlInstanceDisplayHint : ContentDisplayHint
    {
        [JsonProperty(PropertyName = "pidlInstance")]
        public string PidlInstance { get; set; }

        [JsonProperty(PropertyName = "triggerSubmitOrder")]
        public string TriggerSubmitOrder { get; set; }
    }
}