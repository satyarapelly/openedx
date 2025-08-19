// <copyright file="PropertyValidation.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class PropertyValidation
    {
        private string errorMessage;
        private string validationRegex;
        private string resolutionRegex;

        public PropertyValidation()
        {
        }

        public PropertyValidation(string validationRegex) :
            this(validationRegex, null, null)
        {
        }

        public PropertyValidation(string validationRegex, string errorCode, string errorMessage, string validationType = "regex")
        {
            this.validationRegex = validationRegex;
            this.ErrorCode = errorCode;
            this.errorMessage = errorMessage;
            this.ValidationType = validationType;
        }

        public PropertyValidation(PropertyValidation template, Dictionary<string, string> contextTable = null)
        {
            this.ValidationType = template.ValidationType;
            this.ValidationUrl = template.ValidationUrl;
            this.UrlValidationType = template.UrlValidationType;
            this.validationRegex = template.validationRegex;
            this.ValidationFunction = template.ValidationFunction;
            this.errorMessage = template.errorMessage;
            this.ErrorCode = template.ErrorCode;
            this.resolutionRegex = template.resolutionRegex;

            if (contextTable != null)
            {
                foreach (KeyValuePair<string, string> contextKeyValue in contextTable)
                {
                    this.validationRegex = this.validationRegex == null ? null : this.validationRegex.Replace(contextKeyValue.Key, contextKeyValue.Value);
                    this.errorMessage = this.errorMessage == null ? null : this.errorMessage.Replace(contextKeyValue.Key, contextKeyValue.Value);
                    this.ErrorCode = this.ErrorCode == null ? null : this.ErrorCode.Replace(contextKeyValue.Key, contextKeyValue.Value);
                    this.ValidationUrl = this.ValidationUrl == null ? null : this.ValidationUrl.Replace(contextKeyValue.Key, contextKeyValue.Value);
                    this.resolutionRegex = this.resolutionRegex == null ? null : this.resolutionRegex.Replace(contextKeyValue.Key, contextKeyValue.Value);
                    this.UrlValidationType = this.UrlValidationType?.Replace(contextKeyValue.Key, contextKeyValue.Value);
                }
            }
        }

        [JsonProperty(Order = 0, PropertyName = "validationType")]
        public string ValidationType { get; set; }

        [JsonProperty(Order = 1, PropertyName = "url")]
        public string ValidationUrl { get; set; }

        [JsonProperty(Order = 2, PropertyName = "urlValidationType")]
        public string UrlValidationType { get; set; }

        [JsonProperty(Order = 3, PropertyName = "regex")]
        public string Regex
        {
            get
            {
                return this.validationRegex;
            }

            set
            {
                this.validationRegex = value;
            }
        }

        [JsonProperty(Order = 4, PropertyName = "error_code")]
        public string ErrorCode { get; set; }

        [JsonProperty(Order = 5, PropertyName = "error_message")]
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

        [JsonProperty(Order = 5, PropertyName = "resolutionRegex")]
        public string ResolutionRegex
        {
            get
            {
                return this.resolutionRegex;
            }

            set
            {
                this.resolutionRegex = value;
            }
        }

        [JsonProperty(Order = 6, PropertyName = "function")]
        public string ValidationFunction { get; set; }
    }
}