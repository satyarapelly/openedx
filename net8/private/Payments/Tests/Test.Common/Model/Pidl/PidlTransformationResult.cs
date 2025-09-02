// <copyright file="PidlTransformationResult.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    public class PidlTransformationResult<TDataType> : PidlExecutionResult 
        where TDataType : class 
    {
        [JsonProperty(PropertyName = "transformedValue")]
        public TDataType TransformedValue { get; set; }
    }
}