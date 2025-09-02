// <copyright file="HyperlinkDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a Hyperlink DisplayHint
    /// </summary>
    public sealed class HyperlinkDisplayHint : ContentDisplayHint
    {
        [JsonProperty(PropertyName = "sourceUrl")]
        public string SourceUrl
        {
            get;
            set;
        }
    }
}