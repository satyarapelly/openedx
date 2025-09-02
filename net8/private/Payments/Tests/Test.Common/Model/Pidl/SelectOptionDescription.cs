// <copyright file="SelectOptionDescription.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class SelectOptionDescription
    {
        public SelectOptionDescription()
        {
        }

        public SelectOptionDescription(
            string displayImageUrl, 
            string displayText, 
            DisplayHintAction pidlAction,
            bool isDisabled,
            GroupDisplayHint displayContent,
            string displayType,
            string displayContentHintId,
            IEnumerable<string> styleHints)
        {
            this.DisplayImageUrl = displayImageUrl;
            this.DisplayText = displayText;
            this.PidlAction = pidlAction;
            this.IsDisabled = isDisabled;
            this.DisplayContent = displayContent;
            this.DisplayType = displayType;
            this.DisplayContentHintId = displayContentHintId;
            this.StyleHints = styleHints;
        }

        [JsonProperty(Order = 0, PropertyName = "displayImageUrl")]
        public string DisplayImageUrl { get; set; }

        [JsonProperty(Order = 1, PropertyName = "displayText")]
        public string DisplayText { get; set; }

        [JsonProperty(Order = 2, PropertyName = "pidlAction")]
        public DisplayHintAction PidlAction { get; set; }

        [JsonProperty(Order = 3, PropertyName = "isDisabled")]
        public bool IsDisabled { get; set; }

        [JsonProperty(Order = 4, PropertyName = "displayContent")]
        public GroupDisplayHint DisplayContent { get; set; }

        [JsonProperty(Order = 5, PropertyName = "displayType")]
        public string DisplayType { get; set; }

        [JsonProperty(Order = 6, PropertyName = "accessibilityName")]
        public string AccessibilityName { get; set; }

        [JsonProperty(Order = 7, PropertyName = "styleHints")]
        public IEnumerable<string> StyleHints { get; set; }

        [JsonProperty(Order = 8, PropertyName = "tags")]
        public Dictionary<string, string> DisplayTags { get; set; }

        [JsonProperty(Order = 9, PropertyName = "onresourceselected")]
        public PropertyEvent OnResourceSelected { get; set; }

        [JsonIgnore]
        public string DisplayContentHintId { get; set; }
    }
}
