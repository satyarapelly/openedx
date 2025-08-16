// <copyright file="IFrameDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using PXCommon;

    public sealed class IFrameDisplayHint : ContentDisplayHint
    {
        private string sourceUrl;

        public IFrameDisplayHint()
        {
        }

        public IFrameDisplayHint(IFrameDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
        }

        [JsonProperty(PropertyName = "sourceUrl")]
        public string SourceUrl
        {
            get { return this.sourceUrl; }
            set { this.sourceUrl = value; }
        }

        [JsonProperty(PropertyName = "expectedClientActionId")]
        public string ExpectedClientActionId { get; set; }

        [JsonProperty(PropertyName = "messageTimeout")]
        public int? MessageTimeout { get; set; }

        [JsonProperty(PropertyName = "messageTimeoutClientAction")]
        public ClientAction MessageTimeoutClientAction { get; set; }

        [JsonProperty(PropertyName = "width")]
        public string Width { get; set; }

        [JsonProperty(PropertyName = "height")]
        public string Height { get; set; }

        [JsonProperty(PropertyName = "useAuth")]
        public bool? UseAuth { get; set; }

        [JsonProperty(PropertyName = "loadingMessage")]
        public string LoadingMessage { get; set; }

        protected override string GetDisplayType()
        {
            return HintType.IFrame.ToString().ToLower();
        }
    }
}
