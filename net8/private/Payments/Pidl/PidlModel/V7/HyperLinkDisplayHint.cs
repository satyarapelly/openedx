// <copyright file="HyperlinkDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a Hyperlink DisplayHint
    /// </summary>
    public sealed class HyperlinkDisplayHint : ContentDisplayHint
    {
        public HyperlinkDisplayHint()
        {
        }

        public HyperlinkDisplayHint(HyperlinkDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
            this.SourceUrl = template.SourceUrl;
        }

        [JsonProperty(PropertyName = "sourceUrl")]
        public string SourceUrl
        {
            get;
            set;
        }

        protected override string GetDisplayType()
        {
            return HintType.Hyperlink.ToString().ToLower();
        }
    }
}