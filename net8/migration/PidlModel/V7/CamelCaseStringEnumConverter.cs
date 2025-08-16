// <copyright file="CamelCaseStringEnumConverter.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    // Allows [JsonConverter(typeof(CamelCaseStringEnumConverter))] as syntactic sugar for [JsonConverter(typeof(StringEnumConverter), true)]
    // which is available for Newtonsoft.Json 9.0.1 so this class is deprecated if using that version.
    public class CamelCaseStringEnumConverter : StringEnumConverter
    {
        public CamelCaseStringEnumConverter() : base()
        {
            this.NamingStrategy = new CamelCaseNamingStrategy();
        }
    }
}
