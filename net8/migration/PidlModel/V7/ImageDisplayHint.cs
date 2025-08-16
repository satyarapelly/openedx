// <copyright file="ImageDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a Image DisplayHint
    /// </summary>
    public class ImageDisplayHint : DisplayHint
    {
        private string displayContent;

        public ImageDisplayHint()
        {
        }

        public ImageDisplayHint(ImageDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
            this.DisplayContent = template.DisplayContent;
            this.SourceUrl = template.SourceUrl;
            this.Codepoint = template.Codepoint;
            if (contextTable != null && contextTable.Keys.Count > 0)
            {
                foreach (KeyValuePair<string, string> contextKeyValue in contextTable)
                {
                    this.DisplayContent = this.DisplayContent == null ? null : this.DisplayContent.Replace(contextKeyValue.Key, contextKeyValue.Value);
                }
            }
        }

        [JsonProperty(PropertyName = "displayContent")]
        public string DisplayContent
        {
            get { return this.displayContent == null ? null : PidlModelHelper.GetLocalizedString(this.displayContent); }
            set { this.displayContent = value; }
        }

        [JsonProperty(PropertyName = "sourceUrl")]
        public string SourceUrl
        {
            get;
            set;
        }

        // In pidlsdk, this is used to load the icon from the ttf font file
        [JsonProperty(PropertyName = "codepoint")]
        public string Codepoint
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "accessibilityName")]
        public string AccessibilityName
        {
            get;
            set;
        }

        protected override string GetDisplayType()
        {
            return HintType.Image.ToString().ToLower();
        }
    }
}