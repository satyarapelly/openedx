// <copyright file="BrowserFlowContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model
{
    public class BrowserFlowContext
    {
        public bool IsFingerPrintRequired { get; set; }

        public bool IsAcsChallengeRequired { get; set; }

        public string FormActionURL { get; set; }

        public bool? FormPostAcsURL { get; set; }

        public bool? FormFullPageRedirectAcsURL { get; set; }

        public string FormInputThreeDSMethodData { get; set; }

        public string FormInputCReq { get; set; }

        public string FormInputThreeDSSessionData { get; set; }

        public PaymentSession PaymentSession { get; set; }

        public WindowSize ChallengeWindowSize { get; set; }

        public string CardHolderInfo { get; set; }

        public string TransactionSessionId { get; set; }
    }
}