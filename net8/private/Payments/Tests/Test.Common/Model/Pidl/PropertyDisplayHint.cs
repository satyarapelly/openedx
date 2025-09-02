// <copyright file="PropertyDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the display description of a property
    /// </summary>
    public sealed class PropertyDisplayHint : DisplayHint
    {
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "displayPropertyDescription")]
        public string DisplayDescription { get; set; }

        [JsonProperty(PropertyName = "maskInput")]
        public bool? MaskInput { get; set; }

        [JsonProperty(PropertyName = "maskDisplay")]
        public bool? MaskDisplay { get; set; }

        [JsonProperty(PropertyName = "inputScope")]
        public string InputScope { get; set; }

        // 'showDisplayName' is an mandatory field and it provides whether the Display Label needs to be displayed in the UI.
        [JsonProperty(PropertyName = "showDisplayName")]
        public string ShowDisplayName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(PropertyName = "displayExample")]
        public List<string> DisplayExample { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(PropertyName = "displayFormat")]
        public List<string> DisplayFormat { get; set; }
        [JsonProperty(PropertyName = "minLength")]
        public int? MinLength { get; set; }

        // 'maxLength' represents the max size of the input box / drop-down box that should be rendered 
        // to cover 90th percentile of user inputs for this property.
        [JsonProperty(PropertyName = "maxLength")]
        public int? MaxLength { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        //// TODO: Make it obsolete when all clients are upgraded to use PossibleOptions
        [JsonProperty(PropertyName = "possibleValues")]
        public Dictionary<string, string> PossibleValues { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(PropertyName = "possibleOptions")]
        public Dictionary<string, SelectOptionDescription> PossibleOptions { get; set; }
        
        // 'displaySelectionText' exists only when 'possibleValues' exist.  In the case where 'possibleValues' are rendered as a drop-down box in the UI,
        //  the value provided by 'displaySelectionText' can be used as the default / disabled  option in the UI.
        [JsonProperty(PropertyName = "displaySelectionText")]
        public string DisplaySelectionText { get; set; }

        [JsonProperty(PropertyName = "displayImage")]
        public string DisplayImage { get; set; }

        [JsonProperty(PropertyName = "displayLogo")]
        public string DisplayLogo { get; set; }

        [JsonProperty(PropertyName = "selectType")]
        public string SelectType { get; set; }

        [JsonProperty(PropertyName = "displayErrorMessages")]
        public PropertyDisplayErrorMessageMap DisplayErrorMessages { get; set; }

        // 'resolutionPolicy' is an optional field that is used to determine how pidl resolutions are to be done on the SDK.
        // If the field doesn't exist the default behavior is to attempt to resolve pidl on every input ('always' policy).
        [JsonProperty(PropertyName = "resolutionPolicy")]
        public string ResolutionPolicy { get; set; }

        [JsonProperty(PropertyName = "dataCollectionSource")]
        public string DataCollectionSource { get; set; }

        [JsonProperty(PropertyName = "filterDescription")]
        public DataCollectionFilterDescription DataCollectionFilterDescription { get; set; }

        [JsonProperty(PropertyName = "isSelectFirstItem")]
        public bool? IsSelectFirstItem { get; set; }

        [JsonProperty(PropertyName = "alwaysUpdateModelValue")]
        public bool? AlwaysUpdateModelValue { get; set; }

        [JsonProperty(PropertyName = "onfocusout")]
        public PropertyEvent Onfocusout { get; set; }

        public override string DisplayText()
        {
            return this.DisplayName; 
        }
    }
}