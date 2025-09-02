// <copyright file="LogoDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a Logo DisplayHint
    /// </summary>
    public sealed class LogoDisplayHint : DisplayHint
    {
        [JsonProperty(PropertyName = "sourceUrl")]
        public string SourceUrl
        {
            get;
            set;
        }
    }
}