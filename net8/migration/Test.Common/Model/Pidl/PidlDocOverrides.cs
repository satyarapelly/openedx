// <copyright file="PidlDocOverrides.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
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