// <copyright file="ExpressCheckoutButtonDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// Model class for Express Checkout ButtonDisplayHint
    /// </summary>
    public sealed class ExpressCheckoutButtonDisplayHint : DisplayHint
    {
        public ExpressCheckoutButtonDisplayHint()
        {
        }

        public ExpressCheckoutButtonDisplayHint(ExpressCheckoutButtonDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
            if (template != null)
            {
                this.SourceUrl = template.SourceUrl;
                this.FrameName = template.FrameName;
                this.MessageTimeout = template.MessageTimeout;
                this.Payload = template.Payload;
            }
        }

        [JsonProperty(PropertyName = "frameName")]
        public string FrameName { get; set; }

        [JsonProperty(PropertyName = "sourceUrl")]
        public string SourceUrl { get; set; }

        [JsonProperty(PropertyName = "messageTimeout")]
        public int MessageTimeout { get; set; }

        [JsonProperty(PropertyName = "payload")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, object> Payload { get; set; }

        protected override string GetDisplayType()
        {
            return HintType.ExpressCheckoutButton.ToString().ToLower();
        }
    }
}