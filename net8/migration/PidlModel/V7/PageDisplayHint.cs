// <copyright file="PageDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Class for describing a Page Display Hint
    /// </summary>
    public class PageDisplayHint : ContainerDisplayHint
    {
        private string displayName;

        public PageDisplayHint() :
            base()
        {
        }

        public PageDisplayHint(PageDisplayHint template)
            : base(template)
        {
            this.DisplayName = template.DisplayName;
            this.Extend = template.Extend;
            this.FirstButtonGroup = template.FirstButtonGroup;
            this.ExtendButtonGroup = template.ExtendButtonGroup;
        }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName
        {
            get { return string.IsNullOrEmpty(this.displayName) ? null : PidlModelHelper.GetLocalizedString(this.displayName); }
            set { this.displayName = value; }
        }

        [JsonProperty(PropertyName = "keyPidlActions")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, DisplayHintAction> KeyPidlActions { get; set; }

        [JsonIgnore]
        public bool? Extend { get; set; }

        [JsonIgnore]
        public string FirstButtonGroup { get; set; }

        [JsonIgnore]
        public string ExtendButtonGroup { get; set; }

        public void AddKeyPidlAction(string key, DisplayHintAction action)
        {
            if (this.KeyPidlActions == null)
            {
                this.KeyPidlActions = new Dictionary<string, DisplayHintAction>();
            }

            this.KeyPidlActions.Add(key, action);
        }

        protected override string GetDisplayType()
        {
            return HintType.Page.ToString().ToLower();
        }
    }
}