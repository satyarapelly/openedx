// <copyright file="DataCollectionFilterDescription.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    
    public class DataCollectionFilterDescription
    {
        public DataCollectionFilterDescription(string filterFunctionName)
        {
            this.FunctionName = filterFunctionName;
        }

        [JsonProperty(PropertyName = "functionName")]
        public string FunctionName { get; set; }
    }
}