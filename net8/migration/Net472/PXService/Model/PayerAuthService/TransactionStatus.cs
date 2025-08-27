// <copyright file="TransactionStatus.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TransactionStatus
    {
        ////Y = Authentication / Account Verification Successful
        ////N = Not Authenticated /Account Not Verified; Transaction denied
        ////U = Authentication / Account Verification Could Not Be Performed; Technical or other problem, as indicated in ARes or RReq
        ////A = Attempts Processing Performed; Not Authenticated/Verified, but a proof of attempted authentication/verification is provided
        ////C = Challenge Required; Additional authentication is required using the CReq/CRes
        ////R = Authentication / Account Verification Rejected; Issuer is rejecting authentication/verification and request that authorisation not be attempted.
        ////FR = Payer Auth Rejected for fraud, unlike the other values here, this is not 3DS supported and is unique to our ecosystem

        [EnumMember(Value = "Y")]
        Y,

        [EnumMember(Value = "N")]
        N,

        [EnumMember(Value = "U")]
        U,

        [EnumMember(Value = "A")]
        A,

        [EnumMember(Value = "C")]
        C,

        [EnumMember(Value = "R")]
        R,

        [EnumMember(Value = "FR")]
        FR,
    }
}