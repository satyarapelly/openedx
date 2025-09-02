// <copyright file="DisplayCondition.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    
    public class DisplayCondition
    {
        public DisplayCondition(string displayConditionFunctionName)
        {
            this.FunctionName = displayConditionFunctionName;
        }

        [JsonProperty(PropertyName = "functionName")]
        public string FunctionName { get; set; }
    }
}