// <copyright file="ImageDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a Image DisplayHint
    /// </summary>
    public class ImageDisplayHint : DisplayHint
    {
        [JsonProperty(PropertyName = "displayContent")]
        public string DisplayContent { get; set; }

        [JsonProperty(PropertyName = "sourceUrl")]
        public string SourceUrl
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "codepoint")]
        public string Codepoint
        {
            get;
            set;
        }
    }
}