// <copyright file="PrefillControlDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a PrefillControlDisplayHint
    /// </summary>
    public class PrefillControlDisplayHint : DisplayHint
    {
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "showDisplayName")]
        public string ShowDisplayName { get; set; }

        [JsonProperty(PropertyName = "selectType")]
        public string SelectType
        {
            get;
            set;
        }
    }
}
