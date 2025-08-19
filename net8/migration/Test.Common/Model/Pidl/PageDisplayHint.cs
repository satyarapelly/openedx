// <copyright file="PageDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a Page Display Hint
    /// </summary>
    public class PageDisplayHint : ContainerDisplayHint
    { 
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonIgnore]
        public bool? Extend { get; set; }

        [JsonIgnore]
        public string FirstButtonGroup { get; set; }

        [JsonIgnore]
        public string ExtendButtonGroup { get; set; }
    }
}