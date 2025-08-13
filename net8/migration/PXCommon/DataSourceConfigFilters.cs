// <copyright file="DataSourceConfigFilters.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXCommon
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class DataSourceConfigFilters
    {
        [JsonProperty(PropertyName = "functionName")]
        public string FunctionName { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        [JsonProperty(PropertyName = "functionContext")]
        public Dictionary<string, List<string>> FunctionContext { get; set; }
    }
}
