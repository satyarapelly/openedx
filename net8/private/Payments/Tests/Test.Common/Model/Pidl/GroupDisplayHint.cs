// <copyright file="GroupDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    public enum ShowDisplayNameOption
    {
        TRUE,
        FALSE,
        OPTIONAL
    }

    /// <summary>
    /// Class for describing a Group Display Hint
    /// </summary>
    public class GroupDisplayHint : ContainerDisplayHint
    {
        // 'displayName' is an optional field and it provides the label for the group which should be displayed in the UI.
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        // 'showDisplayName' is an mandatory field and it provides whether the Display Label needs to be displayed in the UI.
        [JsonProperty(PropertyName = "showDisplayName")]
        public string ShowDisplayName { get; set; }

        // 'isSumbitGroup' is an optional field and it indicates whether it's a submit group.
        [JsonProperty(PropertyName = "isSubmitGroup")]
        public bool? IsSumbitGroup { get; set; }

        // 'isModalGroup' is an optional field and it indicates whether it's a modal group.
        [JsonProperty(PropertyName = "isModalGroup")]
        public bool? IsModalGroup { get; set; }

        // 'dataCollectionSource' is an optional field indicates the data collection source that this group is bound to.
        [JsonProperty(PropertyName = "dataCollectionSource")]
        public string DataCollectionSource { get; set; }

        // 'filterDescription' is an optional field indicates the filter function on data collection source that this group is bound to.
        [JsonProperty(PropertyName = "filterDescription")]
        public DataCollectionFilterDescription DataCollectionFilterDescription { get; set; }
    }
}