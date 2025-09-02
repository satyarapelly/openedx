// <copyright file="CaptchaDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a Captcha Display Hint
    /// </summary>
    public class CaptchaDisplayHint : ContainerDisplayHint
    {
        // 'displayName' is an optional field and it provides the label for the group which should be displayed in the UI.
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        // 'showDisplayName' is an mandatory field and it provides whether the Display Label needs to be displayed in the UI.
        [JsonProperty(PropertyName = "showDisplayName")]
        public string ShowDisplayName { get; set; }

        [JsonProperty(PropertyName = "imageMembers")]
        public List<DisplayHint> ImageMembers { get; set; }

        [JsonProperty(PropertyName = "audioMembers")]
        public List<DisplayHint> AudioMembers { get; set; }
    }
}