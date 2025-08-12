// <copyright file="HIPCaptchaUserInput.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.Captcha
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class representing the Captcha solution inputed my the challenged user
    /// </summary>
    public class HIPCaptchaUserInput
    {
        /// <summary>
        /// Gets or sets the value of the validated captcha id
        /// </summary>
        [JsonProperty(PropertyName = "ChallengeId")]
        public string ChallengeId { get; set; }

        /// <summary>
        /// Gets or sets the value of captcha Solution the user submitted
        /// </summary>
        [JsonProperty(PropertyName = "InputSolution")]
        public string InputSolution { get; set; }
    }
}
