// <copyright file="DisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    /// <summary>
    /// Abstract class for describing a Display Hint
    /// </summary>
    public class DisplayHint
    {
        private string displayHelpSequenceText;
        private IEnumerable<DisplayHint> helpDisplayDescriptions = null;
        private Dictionary<string, string> displayTags = null;
        private Dictionary<string, string> displayConditionalFields = null;
        private Dictionary<string, DisplayTransformation> displayTransformations = null;
        private string displayHintValue;

        // List of Display Tags that need to be Localized
        private List<string> displayTagsToBeLocalized = new List<string>
        {
            "accessibilityName",
            "accessibilityHint",
            "help.closeButtonAccessibilityName",
            "help.accessibilityName",
            "help.linkAccessibilityName"
        };

        public DisplayHint()
        {
        }

        public DisplayHint(DisplayHint template)
        {
            this.HintId = template.HintId;
            this.PropertyName = template.PropertyName;
            this.DependentPropertyName = template.DependentPropertyName;
            if (template.Action != null)
            {
                this.Action = new DisplayHintAction(template.Action.ActionType, template.Action.IsDefault, template.Action.Context, template.Action.DestinationId);
            }

            if (template.DisplayTags != null)
            {
                this.displayTags = new Dictionary<string, string>(template.DisplayTags, StringComparer.OrdinalIgnoreCase);
            }

            this.IsHidden = template.IsHidden;
            this.IsDisabled = template.IsDisabled;
            this.IsHighlighted = template.IsHighlighted;
            this.IsBack = template.IsBack;
            this.DependentPropertyValueRegex = template.DependentPropertyValueRegex;
            this.DisplayHelpSequenceId = template.DisplayHelpSequenceId;
            this.DisplayHelpSequenceText = template.DisplayHelpSequenceText;
            this.HelpDisplayDescriptions = template.HelpDisplayDescriptions;
            this.DisplayImage = template.DisplayImage;
            this.StyleHints = template.StyleHints;

            if (template.DisplayTransformations != null)
            {
                this.displayTransformations = new Dictionary<string, DisplayTransformation>(template.DisplayTransformations, StringComparer.CurrentCultureIgnoreCase);
            }

            this.DisplayCondition = template.DisplayCondition;
            this.DisplayHelpPosition = template.DisplayHelpPosition;
        }

        public DisplayHint(DisplayHint template, Dictionary<string, string> contextTable)
            : this(template)
        {
            if (contextTable != null && contextTable.Keys.Count > 0)
            {
                foreach (KeyValuePair<string, string> contextKeyValue in contextTable)
                {
                    this.DependentPropertyValueRegex = this.DependentPropertyValueRegex == null ? null : this.DependentPropertyValueRegex.Replace(contextKeyValue.Key, contextKeyValue.Value);
                }
            }
        }

        [JsonProperty(PropertyName = "displayId")]
        public string HintId { get; set; }

        [JsonProperty(PropertyName = "displayType")]
        public string DisplayHintType
        {
            get
            {
                return string.IsNullOrEmpty(this.displayHintValue) ? this.GetDisplayType() : this.displayHintValue;
            }

            set
            {
                this.displayHintValue = value;
            }
        }

        [JsonProperty(PropertyName = "isHidden")]
        public bool? IsHidden { get; set; }

        [JsonProperty(PropertyName = "isDisabled")]
        public bool? IsDisabled { get; set; }

        [JsonProperty(PropertyName = "isHighlighted")]
        public bool? IsHighlighted { get; set; }

        [JsonProperty(PropertyName = "isBack")]
        public bool? IsBack { get; set; }

        [JsonProperty(PropertyName = "pidlAction")]
        public DisplayHintAction Action
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "propertyName")]
        public string PropertyName { get; set; }

        [JsonProperty(PropertyName = "dependentPropertyName")]
        public string DependentPropertyName { get; set; }

        [JsonProperty(PropertyName = "dependentPropertyValueRegex")]
        public string DependentPropertyValueRegex { get; set; }

        [JsonIgnore]
        public string DisplayHelpSequenceId { get; set; }

        [JsonProperty(PropertyName = "displayHelpName")]
        public string DisplayHelpSequenceText
        {
            get { return this.displayHelpSequenceText == null ? null : PidlModelHelper.GetLocalizedString(this.displayHelpSequenceText); }
            set { this.displayHelpSequenceText = value; }
        }

        // 'displayHelp' provides the display description of the information to help the user to input the right value in the UI.
        [JsonProperty(PropertyName = "displayHelp")]
        public IEnumerable<DisplayHint> HelpDisplayDescriptions
        {
            get
            {
                return this.helpDisplayDescriptions;
            }

            set
            {
                this.helpDisplayDescriptions = value;
            }
        }

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

        [JsonProperty(PropertyName = "conditionalFields")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, string> ConditionalFields
        {
            get
            {
                return this.displayConditionalFields;
            }

            set
            {
                this.displayConditionalFields = value;
            }
        }

        [JsonProperty(PropertyName = "displayTransformations")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, DisplayTransformation> DisplayTransformations
        {
            get
            {
                return this.displayTransformations;
            }

            set
            {
                this.displayTransformations = value;
            }
        }

        [JsonProperty(PropertyName = "displayCondition")]
        public DisplayCondition DisplayCondition
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "displayImage")]
        public string DisplayImage
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "displayHelpPosition")]
        public string DisplayHelpPosition
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "styleHints")]
        public IEnumerable<string> StyleHints
        {
            get;
            set;
        }

        public void AddDisplayTags(Dictionary<string, string> tags)
        {
            if (tags == null)
            {
                return;
            }

            this.displayTags = new Dictionary<string, string>(tags);

            foreach (var tagKey in this.displayTagsToBeLocalized)
            {
                if (this.displayTags.ContainsKey(tagKey))
                {
                    this.displayTags[tagKey] = PidlModelHelper.GetLocalizedString(this.displayTags[tagKey]);
                }
            }
        }

        public void AddConditionalFields(Dictionary<string, string> conditionalFields)
        {
            if (conditionalFields == null)
            {
                return;
            }

            this.displayConditionalFields = new Dictionary<string, string>(conditionalFields);
        }

        public void AddDisplayTag(string key, string value)
        {
            if (this.displayTags == null)
            {
                this.displayTags = new Dictionary<string, string>();
            }

            this.displayTags.Add(key, value);
        }

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

        public void AddDisplayTransformations(Dictionary<string, DisplayTransformation> transformations)
        {
            if (transformations == null)
            {
                return;
            }

            this.displayTransformations = new Dictionary<string, DisplayTransformation>(transformations);
        }

        public bool RemoveDisplayTransformations()
        {
            if (this.displayTransformations != null)
            {
                this.displayTransformations = null;
                return true;
            }

            return false;
        }

        public void AddStyleHint(string styleHint)
        {
            if (this.StyleHints == null)
            {
                this.StyleHints = new List<string>();
            }

            ((List<string>)this.StyleHints).Add(styleHint);
        }

        public void AddStyleHints(List<string> styleHints)
        {
            if (this.StyleHints == null)
            {
                this.StyleHints = new List<string>();
            }

            ((List<string>)this.StyleHints).AddRange(styleHints);
        }

        protected virtual string GetDisplayType()
        {
            return null;
        }
    }
}
