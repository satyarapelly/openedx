// <copyright file="ContentDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    /// <summary>
    /// Abstract class for describing a Content Display Hint
    /// </summary>
    public abstract class ContentDisplayHint : DisplayHint
    {
        [JsonProperty(PropertyName = "displayContent")]
        public string DisplayContent { get; set; }

        [JsonProperty(PropertyName = "displayContentDescription")]
        public string DisplayContentDescription { get; set; }

        [JsonProperty(PropertyName = "displayContentDisplayDescription")]
        public GroupDisplayHint DisplayContentGroup { get; set; }

        public override string DisplayText()
        {
            return this.DisplayContent;
        }
    }
}