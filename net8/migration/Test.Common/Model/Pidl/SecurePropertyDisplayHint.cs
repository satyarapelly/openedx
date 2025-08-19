// <copyright file="SecurePropertyDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public sealed class SecurePropertyDisplayHint : DisplayHint
    {
        private string displayName;
        private string displayDescription;
        private List<string> displayExample;
        private List<string> displayFormat;
        private string displaySelectionText;

        public SecurePropertyDisplayHint()
        {
        }

        [JsonProperty(PropertyName = "frameName")]
        public string FrameName { get; set; }

        [JsonProperty(PropertyName = "displayLogo")]
        public string DisplayLogo
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "sourceUrl")]
        public string SourceUrl { get; set; }

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
        public List<string> DisplayExample
        {
            get
            {
                return this.LocalizeDisplayExamples();
            }
        }

        [JsonProperty(PropertyName = "displayFormat")]
        public List<string> DisplayFormat
        {
            get { return this.displayFormat; }
        }

        // 'minLength' represents the min size of the input box / drop-down box that should be rendered 
        // to cover 50th percentile of user inputs for this property.
        [JsonProperty(PropertyName = "minLength")]
        public int? MinLength { get; set; }

        // 'maxLength' represents the max size of the input box / drop-down box that should be rendered 
        // to cover 90th percentile of user inputs for this property.
        [JsonProperty(PropertyName = "maxLength")]
        public int? MaxLength { get; set; }

        // 'displaySelectionText' exists only when 'possibleValues' exist.  In the case where 'possibleValues' are rendered as a drop-down box in the UI,
        //  the value provided by 'displaySelectionText' can be used as the default / disabled  option in the UI.
        [JsonProperty(PropertyName = "displaySelectionText")]
        public string DisplaySelectionText
        {
            get { return string.IsNullOrWhiteSpace(this.displaySelectionText) ? null : PidlModelHelper.GetLocalizedString(this.displaySelectionText); }
            set { this.displaySelectionText = value; }
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
