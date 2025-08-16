// <copyright file="LogoDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a Logo DisplayHint
    /// </summary>
    public sealed class LogoDisplayHint : DisplayHint
    {
        public LogoDisplayHint()
        {
        }

        public LogoDisplayHint(LogoDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
            this.SourceUrl = template.SourceUrl;

            if (contextTable != null && contextTable.Keys.Count > 0)
            {
                foreach (KeyValuePair<string, string> contextKeyValue in contextTable)
                {
                    this.SourceUrl = this.SourceUrl == null ? null : this.SourceUrl.Replace(contextKeyValue.Key, contextKeyValue.Value);
                }
            }
        }

        [JsonProperty(PropertyName = "sourceUrl")]
        public string SourceUrl
        {
            get;
            set;
        }

        protected override string GetDisplayType()
        {
            return HintType.Logo.ToString().ToLower();
        }
    }
}