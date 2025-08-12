// <copyright file="ChallengeMethodType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Supported challenge method types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChallengeMethodType
    {
        /// <summary>
        /// Indicates an unknown challenge method type.
        /// </summary>
        Unknown,

        /// <summary>
        /// Send a 'one time pin' via email.
        /// </summary>
        OtpEmail,

        /// <summary>
        /// Send a 'one time pin' via online banking.
        /// </summary>
        OtpOnlineBanking,

        /// <summary>
        /// Send a 'one time pin' via text messaging.
        /// </summary>
        OtpSms,

        /// <summary>
        /// Perform the challenge using customer service call.
        /// </summary>
        CustomerService,

        /// <summary>
        /// Application to application challenge.
        /// </summary>
        AppToApp,

        /// <summary>
        /// Complete challenge via a voice call.
        /// </summary>
        OutboundCall,
    }
}