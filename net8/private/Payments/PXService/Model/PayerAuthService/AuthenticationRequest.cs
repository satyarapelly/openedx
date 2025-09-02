// <copyright file="AuthenticationRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using PXModel = Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;

    /// <summary>
    /// This is a model per the PayerAuth.V3 API.  
    /// This object is sent as payload to PayerAuth.V3's POST /authenticate API
    /// This model is equivalent to the V2's AReq model.
    /// </summary>
    public class AuthenticationRequest
    {
        public AuthenticationRequest()
        {
        }

        public AuthenticationRequest(PaymentSession paymentSession, PXModel.AuthenticationRequest authRequest)
        {
            this.PaymentSession = paymentSession;
            this.SdkInfo = new SdkInfo
            {
                SdkAppID = authRequest.SdkAppId,
                SdkEncryptedData = authRequest.SdkEncData,
                SdkEphemeralPublicKey = new EphemPublicKey
                {
                    Crv = authRequest.SdkEphemPublicKey?.Crv,
                    X = authRequest.SdkEphemPublicKey?.X,
                    Y = authRequest.SdkEphemPublicKey?.Y,
                    Kty = authRequest.SdkEphemPublicKey?.Kty
                },
                SdkMaximumTimeout = authRequest.SdkMaxTimeout,
                SdkReferenceNumber = authRequest.SdkReferenceNumber,
                SdkTransID = authRequest.SdkTransID,
                DeviceRenderOption = new DeviceRenderOptions
                {
                    // SdkUiType = authRequest.SdkUiType,
                    // SdkInterface = authRequest.SdkInterface,

                    //// forcing the ACS to use the Native UI Types all the time
                    //// we may still get HTML type if the Bank can't do the native type for any reason
                    SdkInterface = "01",
                    SdkUiType = new List<string>() { "01", "02", "03", "04" }
                }
            };
            this.MessageVersion = authRequest.MessageVersion;
        }

        [JsonProperty(PropertyName = "payment_session")]
        public PaymentSession PaymentSession { get; set; }

        [JsonProperty(PropertyName = "sdk_info")]
        public SdkInfo SdkInfo { get; set; }

        [JsonProperty(PropertyName = "browser_info")]
        public BrowserInfo BrowserInfo { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Usage", 
            "CA2227:Collection properties should be read only", 
            Justification = "Needs to be writeable for the JSON deserializer")]
        [JsonProperty(PropertyName = "additional_challenge_data")]
        public Dictionary<ChallengeType, string> AdditionalChallengeData { get; set; }

        [JsonProperty(PropertyName = "three_dsecure_method_completion_indicator")]
        public ThreeDSMethodCompletionIndicator ThreeDSMethodCompletionIndicator { get; set; }

        [JsonProperty(PropertyName = "three_ds_server_trans_id")]
        public string ThreeDSServerTransId { get; set; }

        [JsonProperty(PropertyName = "acs_challenge_notification_url")]
        public string AcsChallengeNotificationUrl { get; set; }

        [JsonProperty(PropertyName = "risk_challenge_indicator")]
        public RiskChallengeIndicator RiskChallengIndicator { get; set; }
    
        // Latest supported version of PSD2 SDK
        [JsonProperty(PropertyName = "message_version")]
        public string MessageVersion { get; set; }
    }
}