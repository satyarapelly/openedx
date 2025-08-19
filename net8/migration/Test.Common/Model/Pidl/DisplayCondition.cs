// <copyright file="DisplayCondition.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    public class DisplayCondition
    { 
        [JsonProperty(PropertyName = "functionName")]
        public string FunctionName { get; set; }
    }
}