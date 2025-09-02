// <copyright file="DisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Abstract class for describing a Display Hint
    /// </summary>
    public abstract class DisplayHint
    {
        [JsonProperty(PropertyName = "displayId")]
        public string HintId { get; set; }

        [JsonProperty(PropertyName = "displayType")]
        public string DisplayHintType { get; set; }

        [JsonProperty(PropertyName = "isHidden")]
        public bool? IsHidden { get; set; }

        [JsonProperty(PropertyName = "isDisabled")]
        public bool? IsDisabled { get; set; }

        [JsonProperty(PropertyName = "isHighlighted")]
        public bool? IsHighlighted { get; set; }

        [JsonProperty(PropertyName = "isBack")]
        public bool? IsBack { get; set; }

        [JsonProperty(PropertyName = "pidlAction")]
        public DisplayHintAction Action { get; set; }

        [JsonProperty(PropertyName = "propertyName")]
        public string PropertyName { get; set; }

        [JsonProperty(PropertyName = "dependentPropertyName")]
        public string DependentPropertyName { get; set; }

        [JsonProperty(PropertyName = "dependentPropertyValueRegex")]
        public string DependentPropertyValueRegex { get; set; }

        [JsonProperty(Order = 5, PropertyName = "styleHints")]
        public List<string> StyleHints { get; set; }

        [JsonIgnore]
        public string DisplayHelpSequenceId { get; set; }

        [JsonProperty(PropertyName = "displayHelpName")]
        public string DisplayHelpSequenceText { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(PropertyName = "displayHelp")]
        public List<DisplayHint> HelpDisplayDescriptions { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(PropertyName = "tags")]
        public Dictionary<string, string> DisplayTags { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(PropertyName = "conditionalFields")]
        public Dictionary<string, string> ConditionalFields { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(PropertyName = "displayTransformations")]
        public Dictionary<string, DisplayTransformation> DisplayTransformations { get; set; }

        [JsonProperty(PropertyName = "displayCondition")]
        public DisplayCondition DisplayCondition { get; set; }

        [JsonProperty(PropertyName = "displayHelpPosition")]
        public string DisplayHelpPosition { get; set; }

        public virtual string DisplayText()
        {
            return null;
        }
    }
}
