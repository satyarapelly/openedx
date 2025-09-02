// <copyright file="AudioDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a Audio DisplayHint
    /// </summary>
    public class AudioDisplayHint : DisplayHint
    {
        public AudioDisplayHint()
        {
        }

        [JsonProperty(PropertyName = "audioUrl")]
        public string AudioUrl
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "accessibilityName")]
        public string AccessibilityName
        {
            get;
            set;
        }

        protected override string GetDisplayType()
        {
            return HintType.Audio.ToString().ToLower();
        }
    }
}