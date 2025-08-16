// <copyright file="PrefillControlDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a PrefillControlDisplayHint
    /// </summary>
    public class PrefillControlDisplayHint : DisplayHint
    {
        private string displayName;

        public PrefillControlDisplayHint()
        {
        }

        public PrefillControlDisplayHint(PrefillControlDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
            this.displayName = template.displayName;
            this.ShowDisplayName = template.ShowDisplayName;
            this.SelectType = template.SelectType;
        }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName
        {
            get { return this.displayName == null ? null : PidlModelHelper.GetLocalizedString(this.displayName); }
            set { this.displayName = value; }
        }

        [JsonProperty(PropertyName = "showDisplayName")]
        public string ShowDisplayName { get; set; }

        [JsonProperty(PropertyName = "selectType")]
        public string SelectType
        {
            get;
            set;
        }

        protected override string GetDisplayType()
        {
            return HintType.PrefillControl.ToString().ToLower();
        }
    }
}
