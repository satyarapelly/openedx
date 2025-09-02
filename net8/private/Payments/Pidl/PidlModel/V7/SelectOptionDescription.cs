// <copyright file="SelectOptionDescription.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class SelectOptionDescription
    {
        private string accessibilityTag;
        private Dictionary<string, string> displayTags = null;

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
            string accessibilityTag,
            IEnumerable<string> styleHints)
        {
            this.DisplayImageUrl = displayImageUrl;
            this.DisplayText = displayText;
            this.PidlAction = pidlAction;
            this.IsDisabled = isDisabled;
            this.DisplayContent = displayContent;
            this.DisplayType = displayType;
            this.DisplayContentHintId = displayContentHintId;
            this.AccessibilityTag = accessibilityTag;
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
        public string AccessibilityTag
        {
            get
            {
                return this.accessibilityTag;
            }

            set
            {
                if (value != null)
                {
                    this.accessibilityTag = PidlModelHelper.GetLocalizedString(value);
                }
                else
                {
                    this.accessibilityTag = value;
                }
            }
        }

        [JsonProperty(Order = 7, PropertyName = "styleHints")]
        public IEnumerable<string> StyleHints { get; set; }

        [JsonProperty(Order = 8, PropertyName = "onresourceselected")]
        public PropertyEvent OnResourceSelected { get; set; }

        [JsonProperty(PropertyName = "tags")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, string> DisplayTags
        {
            get
            {
                return this.displayTags;
            }

            set
            {
                this.displayTags = value;
            }
        }

        [JsonIgnore]
        public string DisplayContentHintId { get; set; }

        public void AddOrUpdateDisplayTag(string key, string value)
        {
            if (this.displayTags == null)
            {
                this.displayTags = new Dictionary<string, string>();
            }

            if (this.displayTags.ContainsKey(key))
            {
                this.displayTags[key] = value;
            }
            else
            {
                this.displayTags.Add(key, value);
            }
        }
    }
}
