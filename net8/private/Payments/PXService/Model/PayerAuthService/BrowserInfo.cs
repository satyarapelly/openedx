// <copyright file="BrowserInfo.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using Microsoft.Commerce.Payments.PXService.V7;
    using Newtonsoft.Json;
    using ThreeDSExternalService;

    public class BrowserInfo
    {
        private string browserLanguage;

        [JsonProperty(PropertyName = "browser_accept_header")]
        public string BrowserAcceptHeader { get; set; }

        [JsonProperty(PropertyName = "browser_ip")]
        public string BrowserIP { get; set; }

        [JsonProperty(PropertyName = "browser_java_enabled")]
        public bool? BrowserJavaEnabled { get; set; }

        [JsonProperty(PropertyName = "browser_language")]
        public string BrowserLanguage
        {
            get
            {
                return this.browserLanguage;
            }

            set
            {
                this.browserLanguage = TruncateLanguage(value);
            }
        }

        [JsonProperty(PropertyName = "browser_color_depth")]
        public string BrowserColorDepth { get; set; }

        [JsonProperty(PropertyName = "browser_screen_height")]
        public string BrowserScreenHeight { get; set; }

        [JsonProperty(PropertyName = "browser_screen_width")]
        public string BrowserScreenWidth { get; set; }

        [JsonProperty(PropertyName = "browser_tz")]
        public string BrowserTZ { get; set; }

        [JsonProperty(PropertyName = "browser_user_agent")]
        public string BrowserUserAgent { get; set; }

        [JsonProperty(PropertyName = "challenge_window_size")]
        public ChallengeWindowSize ChallengeWindowSize { get; set; }

        public static string TruncateLanguage(string value)
        {
            if (value.Length <= Constants.CharacterCount.MaxBrowserLanguageInput && !value.Contains(","))
            {
                return value;
            }

            string languageResult = null, language = null;
            for (int i = 0; i < value.Length; i++)
            {
                string curLetter = value[i].ToString();
                if (curLetter == "-" || curLetter == ",")
                {
                    language = languageResult;
                }

                if (curLetter == "," || i >= Constants.CharacterCount.MaxBrowserLanguageInput)
                {
                    return language;
                }

                languageResult += curLetter;
            }

            return languageResult;
        }
    }
}
