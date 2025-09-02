// <copyright file="PidlResourceStrings.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class PidlResourceStrings
    {
        private Dictionary<string, string> errorCodes = null;
        private Dictionary<string, string> constantStrings = null;
        private Dictionary<string, ServerErrorCode> serverErrorCodes = null;
 
        public PidlResourceStrings()
        {
        }

        public PidlResourceStrings(PidlResourceStrings template)
        {
            if (template.ErrorCodes != null)
            {
                this.errorCodes = new Dictionary<string, string>(template.ErrorCodes);
            }

            if (template.ConstantStrings != null)
            {
                this.constantStrings = new Dictionary<string, string>(template.ConstantStrings);
            }
        }

        [JsonProperty(Order = 0, PropertyName = "errorCodes")]
        public Dictionary<string, string> ErrorCodes 
        {
            get
            {
                return this.errorCodes;
            }

            set
            {
                this.errorCodes = value;
            }
        }

        [JsonProperty(Order = 1, PropertyName = "constants")]
        public Dictionary<string, string> ConstantStrings 
        {
            get
            {
                return this.constantStrings;
            }

            set
            {
                this.constantStrings = value;
            }
        }

        [JsonProperty(Order = 2, PropertyName = "serverErrorCodes")]
        public Dictionary<string, ServerErrorCode> ServerErrorCodes
        {
            get
            {
                return this.serverErrorCodes;
            }

            set
            {
                this.serverErrorCodes = value;
            }
        }

        public void AddDisplayStringMapFromList(List<DisplayStringMap> displayStringMapList)
        {
            if (displayStringMapList == null)
            {
                return;
            }

            foreach (var displayStringMap in displayStringMapList)
            {
                this.AddDisplayStringMap(displayStringMap);
            }
        }

        public void AddOrUpdateServerErrorCode(string errorCode, ServerErrorCode serverErrorCode)
        {
            if (this.serverErrorCodes == null)
            {
                this.serverErrorCodes = new Dictionary<string, ServerErrorCode>(StringComparer.OrdinalIgnoreCase);
            }

            this.serverErrorCodes[errorCode] = serverErrorCode;
        }

        public void AddDisplayStringMap(DisplayStringMap displayStringMap)
        {
            if (displayStringMap.DisplayStringType == DisplayStringType.errorcode)
            {
                if (this.errorCodes == null)
                {
                    this.errorCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                this.errorCodes[displayStringMap.DisplayStringCode] = displayStringMap.DisplayStringValue;
            }
            else if (displayStringMap.DisplayStringType == DisplayStringType.constant)
            {
                if (this.constantStrings == null)
                {
                    this.constantStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                this.constantStrings[displayStringMap.DisplayStringCode] = displayStringMap.DisplayStringValue;
            }
            else if (displayStringMap.DisplayStringType == DisplayStringType.servererrorcode)
            {
                if (this.serverErrorCodes == null)
                {
                    this.serverErrorCodes = new Dictionary<string, ServerErrorCode>(StringComparer.OrdinalIgnoreCase);
                }

                this.serverErrorCodes[displayStringMap.DisplayStringCode] = new ServerErrorCode()
                {
                    ErrorMessage = displayStringMap.DisplayStringValue,
                    Target = displayStringMap.DisplayStringTarget
                };
            }
        }
    }
}