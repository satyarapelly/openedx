// <copyright file="GetCaptchaValues.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.HIPService
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class representing the Captcha solution inputed my the challenged user
    /// </summary>
    public class GetCaptchaValues
    {
        /// <summary>
        /// Gets or sets the value of the captcha id
        /// </summary>
        [JsonProperty(PropertyName = "captchaId")]
        public string CaptchaId { get; set; }

        /// <summary>
        /// Gets or sets the value of the captcha Region
        /// </summary>
        [JsonProperty(PropertyName = "captchaReg")]
        public string CaptchaReg { get; set; }

        /// <summary>
        /// Gets or sets the value of the captcha image
        /// </summary>
        [JsonProperty(PropertyName = "captchaImage")]
        public string CaptchaImage { get; set; }

        /// <summary>
        /// Gets or sets the value of the captcha image
        /// </summary>
        [JsonProperty(PropertyName = "captchaAudio")]
        public string CaptchaAudio { get; set; }
    }
}