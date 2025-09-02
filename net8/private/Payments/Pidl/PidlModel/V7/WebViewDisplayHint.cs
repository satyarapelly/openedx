// <copyright file="WebViewDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a Webview DisplayHint
    /// </summary>
    public sealed class WebViewDisplayHint : DisplayHint
    {
        public WebViewDisplayHint()
        {
        }

        public WebViewDisplayHint(WebViewDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
        }
        
        [JsonProperty(PropertyName = "sourceUrl")]
        public string SourceUrl
        {
            get;
            set;
        }

        protected override string GetDisplayType()
        {
            return HintType.WebView.ToString().ToLower();
        }
    }
}