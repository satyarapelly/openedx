// <copyright file="ContentDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// Abstract class for describing a Content Display Hint
    /// </summary>
    public abstract class ContentDisplayHint : DisplayHint
    {
        private string displayContent;
        private string displayContentDescription;
        private GroupDisplayHint displayContentGroup;

        public ContentDisplayHint()
        {
        }

        public ContentDisplayHint(ContentDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
            this.DisplayContent = template.DisplayContent;
            this.DisplayContentDescription = template.DisplayContentDescription;

            if (contextTable != null && contextTable.Keys.Count > 0)
            {
                foreach (KeyValuePair<string, string> contextKeyValue in contextTable)
                {
                    this.DisplayContent = this.DisplayContent == null ? null : this.DisplayContent.Replace(contextKeyValue.Key, contextKeyValue.Value);
                    this.DisplayContentDescription = this.DisplayContentDescription == null ? null : this.DisplayContentDescription.Replace(contextKeyValue.Key, contextKeyValue.Value);
                }
            }
        }

        [JsonProperty(PropertyName = "displayContent")]
        public string DisplayContent
        {
            get { return this.displayContent == null ? null : PidlModelHelper.GetLocalizedString(this.displayContent); }
            set { this.displayContent = value; }
        }

        [JsonProperty(PropertyName = "displayContentDescription")]
        public string DisplayContentDescription
        {
            get { return this.displayContentDescription == null ? null : PidlModelHelper.GetLocalizedString(this.displayContentDescription); }
            set { this.displayContentDescription = value; }
        }

        [JsonProperty(PropertyName = "displayContentDisplayDescription")]
        public GroupDisplayHint DisplayContentGroup
        {
            get { return this.displayContentGroup; }
            set { this.displayContentGroup = value; }
        }
    }
}