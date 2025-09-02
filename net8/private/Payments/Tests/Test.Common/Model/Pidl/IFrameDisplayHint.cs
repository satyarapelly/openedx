// <copyright file="IFrameDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    public sealed class IFrameDisplayHint : ContentDisplayHint
    {
        private string sourceUrl;

        public IFrameDisplayHint()
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

        [JsonProperty(PropertyName = "updateDisplayContent")]
        public bool? UpdateDisplayContent { get; set; }
    }
}
