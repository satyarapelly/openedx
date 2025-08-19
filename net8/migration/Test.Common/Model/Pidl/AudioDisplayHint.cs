// <copyright file="AudioDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a Audio DisplayHint
    /// </summary>
    public class AudioDisplayHint : DisplayHint
    {
        [JsonProperty(PropertyName = "audioUrl")]
        public string AudioUrl
        {
            get;
            set;
        }
    }
}