// <copyright file="DisplayStringMap.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    public enum DisplayStringType
    {
        constant,
        errorcode,
        servererrorcode
    }

    public class DisplayStringMap
    {
        private string displayStringValue;

        public DisplayStringMap()
        {
        }

        public DisplayStringMap(DisplayStringMap template)
        {
            this.DisplayStringId = template.DisplayStringId;
            this.DisplayStringType = template.DisplayStringType;
            this.DisplayStringCode = template.DisplayStringCode;
            this.DisplayStringValue = template.DisplayStringValue;
            this.DisplayStringTarget = template.DisplayStringTarget;
        }

        [JsonIgnore]
        public string DisplayStringId { get; set; }

        [JsonIgnore]
        public DisplayStringType DisplayStringType { get; set; }

        [JsonProperty(Order = 0, PropertyName = "code")]
        public string DisplayStringCode { get; set; }

        [JsonProperty(Order = 1, PropertyName = "value")]
        public string DisplayStringValue
        {
            get
            {
                return this.displayStringValue == null ? null : PidlModelHelper.GetLocalizedString(this.displayStringValue);
            }

            set
            {
                this.displayStringValue = value;
            }
        }

        [JsonProperty(Order = 2, PropertyName = "target")]
        public string DisplayStringTarget { get; set; }
    }
}