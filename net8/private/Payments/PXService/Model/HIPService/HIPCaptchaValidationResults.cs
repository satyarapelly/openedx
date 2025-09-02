// <copyright file="HIPCaptchaValidationResults.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.HIPService
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class representing the HIP CaptchaValidation Results
    /// </summary>
    public class HIPCaptchaValidationResults
    {
        /// <summary>
        /// Gets or sets the value of the validated captcha id
        /// </summary>
        [JsonProperty(PropertyName = "ChallengeId")]
        public string ChallengeId { get; set; }

        /// <summary>
        /// Gets or sets the value of the validated captcha status
        /// </summary>
        [JsonProperty(PropertyName = "Solved")]
        public string Solved { get; set; }

        /// <summary>
        /// Gets or sets the value of the string that explains why the captcha failed or passed
        /// </summary>
        [JsonProperty(PropertyName = "Reason")]
        public string Reason { get; set; }
    }
}