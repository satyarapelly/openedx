// <copyright file="PropertyDisplayErrorMessageMap.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public enum MessageSourceType
    {
        fromerrorcode,
        fromregex
    }

    public class PropertyDisplayErrorMessageMap
    {
        private string defaultErrorMessage = null;
        private List<PropertyDisplayErrorMessage> errorCodeMessages = null;
        private List<PropertyDisplayErrorMessage> regexMessages = null;

        public PropertyDisplayErrorMessageMap()
        {
        }

        public PropertyDisplayErrorMessageMap(PropertyDisplayErrorMessageMap template)
        {
            this.defaultErrorMessage = template.DefaultErrorMessage;

            if (template.ErrorCodeMessages != null)
            {
                foreach (var errorMessage in template.ErrorCodeMessages)
                {
                    if (this.errorCodeMessages == null)
                    {
                        this.errorCodeMessages = new List<PropertyDisplayErrorMessage>();
                    }

                    this.errorCodeMessages.Add(new PropertyDisplayErrorMessage(errorMessage));
                }
            }

            if (template.RegexMessages != null)
            {
                foreach (var regexMessage in template.RegexMessages)
                {
                    if (this.regexMessages == null)
                    {
                        this.regexMessages = new List<PropertyDisplayErrorMessage>();
                    }

                    this.regexMessages.Add(new PropertyDisplayErrorMessage(regexMessage));
                }
            }
        }

        [JsonProperty(Order = 0, PropertyName = "defaultErrorMessage")]
        public string DefaultErrorMessage
        {
            get
            {
                return this.defaultErrorMessage == null ? null : PidlModelHelper.GetLocalizedString(this.defaultErrorMessage);
            }

            set
            {
                this.defaultErrorMessage = value;
            }
        }

        [JsonProperty(Order = 1, PropertyName = "fromErrorCode")]
        public List<PropertyDisplayErrorMessage> ErrorCodeMessages
        {
            get
            {
                return this.errorCodeMessages;
            }
        }

        [JsonProperty(Order = 2, PropertyName = "fromRegex")]
        public List<PropertyDisplayErrorMessage> RegexMessages
        {
            get
            {
                return this.regexMessages;
            }
        }

        public void AddDisplayMessage(MessageSourceType? target, PropertyDisplayErrorMessage displayMessage)
        {
            if (target == null)
            {
                return;
            }

            if (target == MessageSourceType.fromerrorcode)
            {
                if (this.errorCodeMessages == null)
                {
                    this.errorCodeMessages = new List<PropertyDisplayErrorMessage>();
                }

                this.errorCodeMessages.Add(displayMessage);
            }
            else
            {
                if (target == MessageSourceType.fromregex)
                {
                    if (this.regexMessages == null)
                    {
                        this.regexMessages = new List<PropertyDisplayErrorMessage>();
                    }

                    this.regexMessages.Add(displayMessage);
                }
            }
        }
    }
}