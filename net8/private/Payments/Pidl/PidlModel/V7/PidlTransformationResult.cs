// <copyright file="PidlTransformationResult.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    
    public class PidlTransformationResult<TDataType> : PidlExecutionResult 
        where TDataType : class 
    {
        [JsonProperty(PropertyName = "transformedValue")]
        public TDataType TransformedValue { get; set; }
    }
}