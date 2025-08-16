// <copyright file="GroupDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
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
        private string displayName;
        
        public GroupDisplayHint() 
            : base()
        {
        }

        public GroupDisplayHint(GroupDisplayHint template)
            : base(template)
        {
            this.DisplayName = template.DisplayName;
            this.ShowDisplayName = template.ShowDisplayName;
            this.IsSumbitGroup = template.IsSumbitGroup;
            this.DataCollectionSource = template.DataCollectionSource;
            this.DataCollectionFilterDescription = template.DataCollectionFilterDescription;
        }
        
        // 'displayName' is an optional field and it provides the label for the group which should be displayed in the UI.
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName
        {
            get { return string.IsNullOrEmpty(this.displayName) ? null : PidlModelHelper.GetLocalizedString(this.displayName); }
            set { this.displayName = value; }
        }

        // 'showDisplayName' is an mandatory field and it provides whether the Display Label needs to be displayed in the UI.
        [JsonProperty(PropertyName = "showDisplayName")]
        public string ShowDisplayName { get; set; }

        // 'isSumbitGroup' is an optional field and it indicates whether it's a submit group.
        [JsonProperty(PropertyName = "isSubmitGroup")]
        public bool? IsSumbitGroup { get; set; }

        // 'dataCollectionSource' is an optional field indicates the data collection source that this group is bound to.
        [JsonProperty(PropertyName = "dataCollectionSource")]
        public string DataCollectionSource { get; set; }

        // 'filterDescription' is an optional field indicates the filter function on data collection source that this group is bound to.
        [JsonProperty(PropertyName = "filterDescription")]
        public DataCollectionFilterDescription DataCollectionFilterDescription { get; set; }

        protected override string GetDisplayType()
        {
            return (string.IsNullOrWhiteSpace(this.DataCollectionSource) ? HintType.Group : HintType.DataCollectionBindingGroup).ToString().ToLower();
        }
    }
}