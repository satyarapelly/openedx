// <copyright file="UpdatePropertyValueActionContext.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using Newtonsoft.Json;

    public class UpdatePropertyValueActionContext
    {
        [JsonProperty(PropertyName = "propertyValue")]
        public string PropertyValue { get; set; }

        [JsonProperty(PropertyName = "propertyName")]
        public string PropertyName { get; set; }
    }
}
