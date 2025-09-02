// <copyright file="PropertyDisplayErrorMessage.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    public class PropertyDisplayErrorMessage
    {
        private string errorMessage = null;
        private string regex = null;

        public PropertyDisplayErrorMessage()
        {
        }

        public PropertyDisplayErrorMessage(PropertyDisplayErrorMessage template)
        {
            this.ErrorCode = template.ErrorCode;
            this.regex = template.Regex;
            this.errorMessage = template.ErrorMessage;
        }

        [JsonProperty(Order = 0, PropertyName = "regex")]
        public string Regex
        {
            get
            {
                return this.regex;
            }

            set
            {
                this.regex = value;
            }
        }

        [JsonProperty(Order = 1, PropertyName = "errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty(Order = 2, PropertyName = "errorMessage")]
        public string ErrorMessage
        {
            get
            {
                return this.errorMessage == null ? null : PidlModelHelper.GetLocalizedString(this.errorMessage);
            }

            set
            {
                this.errorMessage = value;
            }
        }
    }
}