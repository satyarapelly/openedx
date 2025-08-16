// <copyright file="PidlDocOverrides.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    public class PidlDocOverrides
    {
        public PidlDocOverrides()
        {
        }

        [JsonProperty(PropertyName = "template")]
        public string Template { get; set; }
    }
}