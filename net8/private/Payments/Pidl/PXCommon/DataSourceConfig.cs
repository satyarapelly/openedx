// <copyright file="DataSourceConfig.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXCommon
{
    using Newtonsoft.Json;

    public class DataSourceConfig
    {
        [JsonProperty(PropertyName = "useLocalDataSource")]
        public bool UseLocalDataSource { get; set; }

        [JsonProperty(PropertyName = "filter")]
        public DataSourceConfigFilters Filter { get; set; }
    }
}
