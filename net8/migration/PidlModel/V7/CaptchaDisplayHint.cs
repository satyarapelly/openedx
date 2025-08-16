// <copyright file="CaptchaDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a Captcha Display Hint
    /// </summary>
    public class CaptchaDisplayHint : ContainerDisplayHint
    {
        private string displayName;
        private List<DisplayHint> imageMembers;
        private List<DisplayHint> audioMembers;

        public CaptchaDisplayHint()
        {
            this.imageMembers = new List<DisplayHint>();
            this.audioMembers = new List<DisplayHint>();
        }

        // 'displayName' is an optional field and it provides the label for the captcha which should be displayed in the UI.
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName
        {
            get { return string.IsNullOrEmpty(this.displayName) ? null : PidlModelHelper.GetLocalizedString(this.displayName); }
            set { this.displayName = value; }
        }

        // 'showDisplayName' is an mandatory field and it provides whether the Display Label needs to be displayed in the UI.
        [JsonProperty(PropertyName = "showDisplayName")]
        public string ShowDisplayName { get; set; }

        [JsonIgnore]
        public string CaptchaDisplayType { get; set; }

        [JsonProperty(PropertyName = "imageMembers")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<DisplayHint> ImageMembers
        {
            get 
            { 
                return this.imageMembers; 
            }

            set
            {
                if (value != null)
                {
                    this.imageMembers.AddRange(value);
                }
            }
        }

        [JsonProperty(PropertyName = "audioMembers")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<DisplayHint> AudioMembers
        {
            get 
            { 
                return this.audioMembers; 
            }

            set
            {
                if (value != null)
                {
                    this.audioMembers.AddRange(value);
                }
            }
        }

        public void AddImageDisplayHint(DisplayHint displayHint)
        {
            this.imageMembers.Add(displayHint);
        }

        public void AddImageDisplayHints(IEnumerable<DisplayHint> displayHints)
        {
            this.imageMembers.AddRange(displayHints);
        }

        public void AddAudioDisplayHint(DisplayHint displayHint)
        {
            this.imageMembers.Add(displayHint);
        }

        public void AddAudioDisplayHints(IEnumerable<DisplayHint> displayHints)
        {
            this.audioMembers.AddRange(displayHints);
        }

        public void RemoveImageDisplayHint(DisplayHint displayHint)
        {
            this.imageMembers.RemoveAll(member => member.HintId == displayHint.HintId);
        }

        public void RemoveAudioDisplayHint(DisplayHint displayHint)
        {
            this.audioMembers.RemoveAll(member => member.HintId == displayHint.HintId);
        }

        protected override string GetDisplayType()
        {
            return HintType.Captcha.ToString().ToLower();
        }
    }
}