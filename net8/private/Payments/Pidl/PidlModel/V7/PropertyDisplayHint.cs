// <copyright file="PropertyDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// This class represents the display description of a property
    /// </summary>
    public class PropertyDisplayHint : DisplayHint
    {
        private string displayName;
        private string displayDescription;
        private List<string> displayExample;
        private List<string> displayFormat;
        private Dictionary<string, SelectOptionDescription> possibleOptions;
        private Dictionary<string, SelectOptionDescription> localizedPossibleOptions;
        private string displaySelectionText;

        public PropertyDisplayHint()
        {
        }

        public PropertyDisplayHint(PropertyDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
            this.DisplayName = template.DisplayName;
            this.DisplayDescription = template.DisplayDescription;
            this.ShowDisplayName = template.ShowDisplayName;
            this.DisplayImage = template.DisplayImage;
            this.DisplayLogo = template.DisplayLogo;
            this.MaskInput = template.MaskInput;
            this.MaskDisplay = template.MaskDisplay;
            this.InputScope = template.InputScope;
            this.SelectType = template.SelectType;

            if (template.DisplayExample != null)
            {
                this.displayExample = new List<string>(template.DisplayExample);
            }

            if (template.DisplayFormat != null)
            {
                this.displayFormat = new List<string>(template.DisplayFormat);
            }

            this.MinLength = template.MinLength;
            this.MaxLength = template.MaxLength;

            this.SetPossibleOptions(template.possibleOptions);

            this.displaySelectionText = template.displaySelectionText;

            if (contextTable != null && contextTable.Keys.Count > 0)
            {
                foreach (KeyValuePair<string, string> contextKeyValue in contextTable)
                {
                    this.DisplayName = this.DisplayName == null ? null : this.DisplayName.Replace(contextKeyValue.Key, contextKeyValue.Value);
                    this.DisplayDescription = this.DisplayDescription == null ? null : this.DisplayDescription.Replace(contextKeyValue.Key, contextKeyValue.Value);
                }
            }

            this.DataCollectionSource = template.DataCollectionSource;
            this.DataCollectionFilterDescription = template.DataCollectionFilterDescription;
            this.IsSelectFirstItem = template.IsSelectFirstItem;
        }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName
        {
            get { return this.displayName == null ? null : PidlModelHelper.GetLocalizedString(this.displayName); }
            set { this.displayName = value; }
        }

        [JsonProperty(PropertyName = "displayPropertyDescription")]
        public string DisplayDescription
        {
            get { return this.displayDescription == null ? null : PidlModelHelper.GetLocalizedString(this.displayDescription); }
            set { this.displayDescription = value; }
        }

        [JsonProperty(PropertyName = "maskInput")]
        public bool? MaskInput { get; set; }

        [JsonProperty(PropertyName = "maskDisplay")]
        public bool? MaskDisplay { get; set; }

        [JsonProperty(PropertyName = "inputScope")]
        public string InputScope { get; set; }

        // 'showDisplayName' is an mandatory field and it provides whether the Display Label needs to be displayed in the UI.
        [JsonProperty(PropertyName = "showDisplayName")]
        public string ShowDisplayName { get; set; }

        [JsonProperty(PropertyName = "displayExample")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<string> DisplayExample
        {
            get
            {
                return this.LocalizeDisplayExamples();
            }

            set
            {
                this.displayExample = value;
            }
        }

        [JsonProperty(PropertyName = "displayFormat")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<string> DisplayFormat
        {
            get { return this.displayFormat; }
            set { this.displayFormat = value; }
        }

        // 'minLength' represents the min size of the input box / drop-down box that should be rendered 
        // to cover 50th percentile of user inputs for this property.
        [JsonProperty(PropertyName = "minLength")]
        public int? MinLength { get; set; }

        // 'maxLength' represents the max size of the input box / drop-down box that should be rendered 
        // to cover 90th percentile of user inputs for this property.
        [JsonProperty(PropertyName = "maxLength")]
        public int? MaxLength { get; set; }

        //// TODO: Make it obsolete when all clients are upgraded to use PossibleOptions
        [JsonProperty(PropertyName = "possibleValues")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, string> PossibleValues
        {
            get
            {
                return this.PossibleOptions == null ? null : this.PossibleOptions.ToDictionary(
                    i => i.Key,
                    i => i.Value.DisplayText);
            }

            set
            {
                this.SetPossibleOptions(value);
            }
        }

        [JsonProperty(PropertyName = "possibleOptions")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, SelectOptionDescription> PossibleOptions
        {
            get
            {
                if (this.localizedPossibleOptions == null)
                {
                    this.LocalizePossibleOptions();
                }

                return this.localizedPossibleOptions;
            }

            set
            {
                this.localizedPossibleOptions = value;
            }
        }

        // 'displaySelectionText' exists only when 'possibleValues' exist.  In the case where 'possibleValues' are rendered as a drop-down box in the UI,
        //  the value provided by 'displaySelectionText' can be used as the default / disabled  option in the UI.
        [JsonProperty(PropertyName = "displaySelectionText")]
        public string DisplaySelectionText
        {
            get { return string.IsNullOrWhiteSpace(this.displaySelectionText) ? null : PidlModelHelper.GetLocalizedString(this.displaySelectionText); }
            set { this.displaySelectionText = value; }
        }

        [JsonProperty(PropertyName = "displayLogo")]
        public string DisplayLogo
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "selectType")]
        public string SelectType
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "displayErrorMessages")]
        public PropertyDisplayErrorMessageMap DisplayErrorMessages
        {
            get;
            set;
        }

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

        // 'alwaysUpdateModelValue' is an optional field on the dropdown element that is used to determine whether the model value on SDK should be updated on every input change.
        // If the field is set to true, the model value on SDK will be updated even if the newly selected value is the same as the previous value.
        [JsonProperty(PropertyName = "alwaysUpdateModelValue")]
        public bool? AlwaysUpdateModelValue { get; set; }

        [JsonProperty(PropertyName = "onfocusout")]
        public PropertyEvent Onfocusout { get; set; }

        public void SetPossibleOptions(Dictionary<string, string> values)
        {
            this.possibleOptions = values == null ? null : values.ToDictionary(
                i => i.Key,
                i => new SelectOptionDescription { DisplayText = i.Value });

            // Set localizedPossibleOptions to null to make sure to localize these values during next GET
            this.localizedPossibleOptions = null;
        }

        public void SetPossibleOptions(Dictionary<string, string[]> values)
        {
            this.possibleOptions = values == null ? null : values.ToDictionary(
                i => i.Key,
                i => new SelectOptionDescription { DisplayText = i.Value[0], DisplayContentHintId = i.Value[1] });

            // Set localizedPossibleOptions to null to make sure to localize these values during next GET
            this.localizedPossibleOptions = null;
        }

        public void SetPossibleOptions(Dictionary<string, SelectOptionDescription> values)
        {
            this.possibleOptions = values;

            // Set localizedPossibleOptions to null to make sure to localize these values during next GET
            this.localizedPossibleOptions = null;
        }

        public void AddPossibleOption(string key, SelectOptionDescription option)
        {
            if (this.possibleOptions == null)
            {
                this.possibleOptions = new Dictionary<string, SelectOptionDescription>();
            }

            this.possibleOptions[key] = option;
        }

        public void AddDisplayExample(string value)
        {
            if (this.displayExample == null)
            {
                this.displayExample = new List<string>();
            }

            this.displayExample.Add(value);
        }

        public void AddDisplayFormat(string value)
        {
            if (this.displayFormat == null)
            {
                this.displayFormat = new List<string>();
            }

            this.displayFormat.Add(value);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        protected override string GetDisplayType()
        {
            return HintType.Property.ToString().ToLower();
        }

        private void LocalizePossibleOptions()
        {
            this.localizedPossibleOptions = this.possibleOptions == null ? null : this.possibleOptions.ToDictionary(
                i => PidlModelHelper.GetNonParameterizedString(i.Key),
                i =>
                {
                    return new SelectOptionDescription(
                        i.Value == null ? null : i.Value.DisplayImageUrl,
                        (i.Value == null || i.Value.DisplayText == null) ? PidlModelHelper.GetNonParameterizedString(i.Key) : PidlModelHelper.GetLocalizedString(i.Value.DisplayText),
                        i.Value.PidlAction == null ? null : new DisplayHintAction(i.Value.PidlAction.ActionType, i.Value.PidlAction.IsDefault, i.Value.PidlAction.Context, i.Value.PidlAction.DestinationId),
                        i.Value.IsDisabled ? true : false,
                        i.Value.DisplayContent == null ? null : i.Value.DisplayContent,
                        i.Value.DisplayType == null ? null : i.Value.DisplayType,
                        i.Value.DisplayContentHintId == null ? null : i.Value.DisplayContentHintId,
                        i.Value.AccessibilityTag == null ? null : i.Value.AccessibilityTag,
                        i.Value.StyleHints == null ? null : i.Value.StyleHints);
                });
        }

        private List<string> LocalizeDisplayExamples()
        {
            if (this.displayExample != null)
            {
                List<string> localizedDisplayExamples = new List<string>();
                this.displayExample.ForEach(e => localizedDisplayExamples.Add(PidlModelHelper.GetLocalizedString(e)));
                return localizedDisplayExamples;
            }

            return null;
        }
    }
}