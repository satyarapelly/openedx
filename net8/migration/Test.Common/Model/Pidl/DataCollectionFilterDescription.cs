// <copyright file="DataCollectionFilterDescription.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    public class DataCollectionFilterDescription
    {
        [JsonProperty(PropertyName = "functionName")]
        public string FunctionName { get; set; }
    }
}