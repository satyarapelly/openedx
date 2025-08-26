// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class ResourceLifecycleStateManagerTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            var test = PIDLResourceFactory.Instance;

            ResourceLifecycleStateManager.Initialize(
               new SelfHostedPXServiceCore.Mocks.PXServiceSettings(),
               Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestData\PIServiceErrorConfig.csv"),
               Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestData\ClientActionConfig.csv"));
        }

        [TestMethod]
        public void PostModernPICUPValidationFailed()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "ValidationFailed",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.PostModernPI, state).Wait();

            Assert.AreEqual("[]", state.ResponseException.Error.Message);
            Assert.IsNotNull(state.ResponseException.Error.Details);
            Assert.AreNotEqual(0, state.ResponseException.Error.Details.Count);
            ServiceErrorDetail errorDetail = state.ResponseException.Error.Details[state.ResponseException.Error.Details.Count - 1];
            Assert.AreEqual(state.ResponseException.Error.ErrorCode, errorDetail.ErrorCode);
            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Check your card and phone numbers. They don’t go together.", state.Language), errorDetail.Message);
            Assert.AreEqual("accountToken,phone", errorDetail.Target);
        }

        [TestMethod]
        public void PostModernPICUPInvalidPhoneValue()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "InvalidPhoneValue",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.PostModernPI, state).Wait();

            Assert.AreEqual("[]", state.ResponseException.Error.Message);
            Assert.IsNotNull(state.ResponseException.Error.Details);
            Assert.AreNotEqual(0, state.ResponseException.Error.Details.Count);
            ServiceErrorDetail errorDetail = state.ResponseException.Error.Details[state.ResponseException.Error.Details.Count - 1];
            Assert.AreEqual(state.ResponseException.Error.ErrorCode, errorDetail.ErrorCode);
            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Check your card and phone numbers. They don’t go together.", state.Language), errorDetail.Message);
            Assert.AreEqual("accountToken,phone", errorDetail.Target);
        }

        [TestMethod]
        public void PostModernPICUPInvalidPaymentInstrumentInfo()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "InvalidPaymentInstrumentInfo",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.PostModernPI, state).Wait();

            Assert.AreEqual("[]", state.ResponseException.Error.Message);
            Assert.IsNotNull(state.ResponseException.Error.Details);
            Assert.AreNotEqual(0, state.ResponseException.Error.Details.Count);
            ServiceErrorDetail errorDetail = state.ResponseException.Error.Details[state.ResponseException.Error.Details.Count - 1];
            Assert.AreEqual(state.ResponseException.Error.ErrorCode, errorDetail.ErrorCode);
            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Check your card number. This one isn't valid.", state.Language), errorDetail.Message);
            Assert.AreEqual("accountToken", errorDetail.Target);
        }

        [TestMethod]
        [DataRow("InvalidIssuerResponseWithTRPAU0009")]
        [DataRow("InvalidIssuerResponseWithTRPAU0008")]
        public void PostModernPI_throwsInvalidIssuerResponse(string errorCode)
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: errorCode,
                paymentMethodFamily: "credit_card",
                paymentMethodType: "visa",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.PostModernPI, state).Wait();

            Assert.AreEqual("[]", state.ResponseException.Error.Message);
            Assert.IsNotNull(state.ResponseException.Error.Details);
            Assert.AreNotEqual(0, state.ResponseException.Error.Details.Count);
            ServiceErrorDetail errorDetail = state.ResponseException.Error.Details[state.ResponseException.Error.Details.Count - 1];
            Assert.AreEqual(state.ResponseException.Error.ErrorCode, errorDetail.ErrorCode);
            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("The card is not enabled for 3ds/otp authentication in India.", state.Language), errorDetail.Message);
            Assert.AreEqual("accountToken", errorDetail.Target);
        }

        [TestMethod]
        [DataRow("upi")]
        [DataRow("upi_commercial")]
        public void PostModernPI_Upi_throws_InvalidAccount_Error(string type)
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "AccountNotFound",
                paymentMethodFamily: "real_time_payments",
                paymentMethodType: type,
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.PostModernPI, state).Wait();

            Assert.AreEqual("[]", state.ResponseException.Error.Message);
            Assert.IsNotNull(state.ResponseException.Error.Details);
            Assert.AreNotEqual(0, state.ResponseException.Error.Details.Count);
            ServiceErrorDetail errorDetail = state.ResponseException.Error.Details[state.ResponseException.Error.Details.Count - 1];
            Assert.AreEqual(state.ResponseException.Error.ErrorCode, errorDetail.ErrorCode);
            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("UPI Id verification failed.", state.Language), errorDetail.Message);
            Assert.AreEqual("vpa", errorDetail.Target);
        }

        [TestMethod]
        public void PostModernPICUPTooManyOperations()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "TooManyOperations",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.PostModernPI, state).Wait();

            Assert.AreEqual("Wait a bit before you ask for a new code. Your requests exceeded the limit.", state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void PostModernPICUPGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.PostModernPI, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void PostModernPIPaypalGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "ewallet",
                paymentMethodType: "paypal",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.PostModernPI, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void PostModernPICCNotWalletGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "visa",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.PostModernPI, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void PostModernPICCWalletGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "visa",
                language: "en-US",
                partner: "wallet",
                country: "kr");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.PostModernPI, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void PostModernPIAchGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "direct_debit",
                paymentMethodType: "ach",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.PostModernPI, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void PostModernPISepaGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "direct_debit",
                paymentMethodType: "sepa",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.PostModernPI, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void PostModernPIAliPayGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "ewallet",
                paymentMethodType: "alipay_billing_agreement",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.PostModernPI, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void PostModernPINonSimGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "mobile_billing_non_sim",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.PostModernPI, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void PostModernPIKlarnaGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "invoice_credit",
                paymentMethodType: "klarna",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.PostModernPI, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void UpdateModernPICUPValidationFailed()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "ValidationFailed",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.UpdateModernPI, state).Wait();

            Assert.AreEqual("[]", state.ResponseException.Error.Message);
            Assert.IsNotNull(state.ResponseException.Error.Details);
            Assert.AreNotEqual(0, state.ResponseException.Error.Details.Count);
            ServiceErrorDetail errorDetail = state.ResponseException.Error.Details[state.ResponseException.Error.Details.Count - 1];
            Assert.AreEqual(state.ResponseException.Error.ErrorCode, errorDetail.ErrorCode);
            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Check your card security code and your phone number.", state.Language), errorDetail.Message);
            Assert.AreEqual("cvvToken,phone", errorDetail.Target);
        }

        [TestMethod]
        public void UpdateModernPICUPInvalidPhoneValue()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "InvalidPhoneValue",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.UpdateModernPI, state).Wait();

            Assert.AreEqual("[]", state.ResponseException.Error.Message);
            Assert.IsNotNull(state.ResponseException.Error.Details);
            Assert.AreNotEqual(0, state.ResponseException.Error.Details.Count);
            ServiceErrorDetail errorDetail = state.ResponseException.Error.Details[state.ResponseException.Error.Details.Count - 1];
            Assert.AreEqual(state.ResponseException.Error.ErrorCode, errorDetail.ErrorCode);
            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Check your card security code and your phone number.", state.Language), errorDetail.Message);
            Assert.AreEqual("cvvToken,phone", errorDetail.Target);
        }

        [TestMethod]
        public void UpdateModernPICUPInvalidCvv()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "InvalidCvv",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.UpdateModernPI, state).Wait();

            Assert.AreEqual("[]", state.ResponseException.Error.Message);
            Assert.IsNotNull(state.ResponseException.Error.Details);
            Assert.AreNotEqual(0, state.ResponseException.Error.Details.Count);
            ServiceErrorDetail errorDetail = state.ResponseException.Error.Details[state.ResponseException.Error.Details.Count - 1];
            Assert.AreEqual(state.ResponseException.Error.ErrorCode, errorDetail.ErrorCode);
            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Check your card security code and your phone number.", state.Language), errorDetail.Message);
            Assert.AreEqual("cvvToken,phone", errorDetail.Target);
        }

        [TestMethod]
        public void UpdateModernPICUPInvalidExpiryDate()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "InvalidExpiryDate",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.UpdateModernPI, state).Wait();

            Assert.AreEqual("[]", state.ResponseException.Error.Message);
            Assert.IsNotNull(state.ResponseException.Error.Details);
            Assert.AreNotEqual(0, state.ResponseException.Error.Details.Count);
            ServiceErrorDetail errorDetail = state.ResponseException.Error.Details[state.ResponseException.Error.Details.Count - 1];
            Assert.AreEqual(state.ResponseException.Error.ErrorCode, errorDetail.ErrorCode);
            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Check your expiration date.", state.Language), errorDetail.Message);
            Assert.AreEqual("expiryMonth,expiryYear", errorDetail.Target);
        }

        [TestMethod]
        public void UpdateModernPICUPExpiredCard()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "ExpiredCard",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.UpdateModernPI, state).Wait();

            Assert.AreEqual("[]", state.ResponseException.Error.Message);
            Assert.IsNotNull(state.ResponseException.Error.Details);
            Assert.AreNotEqual(0, state.ResponseException.Error.Details.Count);
            ServiceErrorDetail errorDetail = state.ResponseException.Error.Details[state.ResponseException.Error.Details.Count - 1];
            Assert.AreEqual(state.ResponseException.Error.ErrorCode, errorDetail.ErrorCode);
            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Check your expiration date.", state.Language), errorDetail.Message);
            Assert.AreEqual("expiryMonth,expiryYear", errorDetail.Target);
        }

        [TestMethod]
        public void UpdateModernPICUPTooManyOperations()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "TooManyOperations",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.UpdateModernPI, state).Wait();

            Assert.AreEqual("Wait a bit before you ask for a new code. Your requests exceeded the limit.", state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void UpdateModernPICUPGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.UpdateModernPI, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void UpdateModernPICUPDCGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_debitcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.UpdateModernPI, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void UpdateModernPICCGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "visa",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.UpdateModernPI, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void UpdateModernPIAchGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "direct_debit",
                paymentMethodType: "ach",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.UpdateModernPI, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void UpdateModernPISepaGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "direct_debit",
                paymentMethodType: "sepa",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.UpdateModernPI, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void ResumePendingOperationCUPValidationFailed()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "ValidationFailed",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.ResumePendingOperation, state).Wait();

            Assert.AreEqual("[]", state.ResponseException.Error.Message);
            Assert.IsNotNull(state.ResponseException.Error.Details);
            Assert.AreNotEqual(0, state.ResponseException.Error.Details.Count);
            ServiceErrorDetail errorDetail = state.ResponseException.Error.Details[state.ResponseException.Error.Details.Count - 1];
            Assert.AreEqual(state.ResponseException.Error.ErrorCode, errorDetail.ErrorCode);
            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Check your card security code and your phone number.", state.Language), errorDetail.Message);
            Assert.AreEqual("cvvToken,phone", errorDetail.Target);
        }

        [TestMethod]
        public void ResumePendingOperationCUPInvalidCvv()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "InvalidCvv",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.ResumePendingOperation, state).Wait();

            Assert.AreEqual("[]", state.ResponseException.Error.Message);
            Assert.IsNotNull(state.ResponseException.Error.Details);
            Assert.AreNotEqual(0, state.ResponseException.Error.Details.Count);
            ServiceErrorDetail errorDetail = state.ResponseException.Error.Details[state.ResponseException.Error.Details.Count - 1];
            Assert.AreEqual(state.ResponseException.Error.ErrorCode, errorDetail.ErrorCode);
            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Check your card security code and your phone number.", state.Language), errorDetail.Message);
            Assert.AreEqual("cvvToken,phone", errorDetail.Target);
        }

        [TestMethod]
        public void ResumePendingOperationCUPInvalidExpiryDate()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "InvalidExpiryDate",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.ResumePendingOperation, state).Wait();

            Assert.AreEqual("[]", state.ResponseException.Error.Message);
            Assert.IsNotNull(state.ResponseException.Error.Details);
            Assert.AreNotEqual(0, state.ResponseException.Error.Details.Count);
            ServiceErrorDetail errorDetail = state.ResponseException.Error.Details[state.ResponseException.Error.Details.Count - 1];
            Assert.AreEqual(state.ResponseException.Error.ErrorCode, errorDetail.ErrorCode);
            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Check your expiration date.", state.Language), errorDetail.Message);
            Assert.AreEqual("expiryMonth,expiryYear", errorDetail.Target);
        }

        [TestMethod]
        public void ResumePendingOperationCUPInvalidInvalidChallengeCode()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "InvalidChallengeCode",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.ResumePendingOperation, state).Wait();

            Assert.AreEqual("[]", state.ResponseException.Error.Message);
            Assert.IsNotNull(state.ResponseException.Error.Details);
            Assert.AreNotEqual(0, state.ResponseException.Error.Details.Count);
            ServiceErrorDetail errorDetail = state.ResponseException.Error.Details[state.ResponseException.Error.Details.Count - 1];
            Assert.AreEqual("InvalidChallengeCode", errorDetail.ErrorCode);
            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Check your code. The one entered isn't valid.", state.Language), errorDetail.Message);
            Assert.AreEqual("pin", errorDetail.Target);
        }

        [TestMethod]
        public void ResumePendingOperationCUPChallengeCodeExpired()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "ChallengeCodeExpired",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.ResumePendingOperation, state).Wait();

            Assert.AreEqual("[]", state.ResponseException.Error.Message);
            Assert.IsNotNull(state.ResponseException.Error.Details);
            Assert.AreNotEqual(0, state.ResponseException.Error.Details.Count);
            ServiceErrorDetail errorDetail = state.ResponseException.Error.Details[state.ResponseException.Error.Details.Count - 1];
            Assert.AreEqual("ChallengeCodeExpired", errorDetail.ErrorCode);
            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Request a new code. This one expired.", state.Language), errorDetail.Message);
            Assert.AreEqual("pin", errorDetail.Target);
        }

        [TestMethod]
        public void ResumePendingOperationCUPTooManyOperations()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "TooManyOperations",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.ResumePendingOperation, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Wait a bit before you ask for a new code. Your requests exceeded the limit.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void ResumePendingOperationCUPGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_creditcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.ResumePendingOperation, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void ResumePendingOperationCUPDCGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "credit_card",
                paymentMethodType: "unionpay_debitcard",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.ResumePendingOperation, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void ResumePendingOperationAchGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "direct_debit",
                paymentMethodType: "ach",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.ResumePendingOperation, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void ResumePendingOperationSepaGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "direct_debit",
                paymentMethodType: "sepa",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.ResumePendingOperation, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void ResumePendingOperationAliPayGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "ewallet",
                paymentMethodType: "alipay_billing_agreement",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.ResumePendingOperation, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        [TestMethod]
        public void ResumePendingOperationNonSimGeneric()
        {
            ResourceLifecycleStateManager.ErrorResourceState state = BuildInitialErrorState(
                errorCode: "unmatchedErrorCode",
                paymentMethodFamily: "mobile_billing_non_sim",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetErrorAsync(ResourceLifecycleStateManager.ErrorResourceAction.ResumePendingOperation, state).Wait();

            Assert.AreEqual(LocalizationRepository.Instance.GetLocalizedString("Try that again. Something happened on our end. Waiting a bit can help.", state.Language), state.ResponseException.Error.Message);
            Assert.IsNull(state.ResponseException.Error.Details);
        }

        /// <summary>
        /// PaymentType filter: PaymentMethodType.IdealBillingAgreement
        /// ClientAction filter:
        /// Changes: ActionType=Redirect, Context=testRedirectUrl
        /// </summary>
        [TestMethod]
        public void AddClientActionToPIIdealBillingAgreementInlinePartner()
        {
            string testRedirectUrl = "testRedirectUrl";

            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               pendingOn: string.Empty,
               redirectUrl: testRedirectUrl,
               paymentMethodType: "ideal_billing_agreement",
               country: "us",
               partner: "cart",
               language: "en-US");

            ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

            Assert.IsNotNull(state.PaymentInstrument.ClientAction);
            Assert.AreEqual(ClientActionType.Redirect, state.PaymentInstrument.ClientAction.ActionType);
            Assert.AreEqual(testRedirectUrl, state.PaymentInstrument.ClientAction.Context);
            Assert.IsNull(state.PaymentInstrument.ClientAction.RedirectPidl);
        }

        /// <summary>
        /// PaymentType filter: PaymentMethodType.IdealBillingAgreement
        /// ClientAction filter:
        /// Changes: ActionType=Redirect, Context=testRedirectUrl
        /// </summary>
        [TestMethod]
        public void AddClientActionToPIIdealBillingAgreementNotInlinePartner()
        {
            string testRedirectUrl = "testRedirectUrl";

            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               pendingOn: string.Empty,
               redirectUrl: testRedirectUrl,
               paymentMethodType: "ideal_billing_agreement",
               country: "us",
               partner: "wallet",
               language: "en-US");

            ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

            Assert.IsNotNull(state.PaymentInstrument.ClientAction);
            Assert.AreEqual(ClientActionType.Redirect, state.PaymentInstrument.ClientAction.ActionType);
            Assert.AreEqual(testRedirectUrl, state.PaymentInstrument.ClientAction.Context);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.RedirectPidl);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.RedirectPidl, typeof(List<PIDLResource>));

            List<PIDLResource> redirectPidl = (List<PIDLResource>)state.PaymentInstrument.ClientAction.RedirectPidl;

            Assert.AreEqual(1, redirectPidl.Count);
            Assert.IsTrue(redirectPidl[0].Identity.ContainsKey("type"));
            Assert.AreEqual("idealredirectpidl", redirectPidl[0].Identity["type"]);
        }

        /// <summary>
        /// PaymentType filter: PaymentMethodType.IdealBillingAgreement
        /// ClientAction filter:
        /// Changes: ActionType=Redirect, Context=testRedirectUrl, RedirectPidl=PidlResourceDescriptionType.IdealBillingAgreementRedirectStaticPidl, ClearMembers
        /// </summary>
        [TestMethod]
        public void AddClientActionToPIIdealBillingAgreementWebblendsInline()
        {
            string testRedirectUrl = "testRedirectUrl";

            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               pendingOn: string.Empty,
               redirectUrl: testRedirectUrl,
               paymentMethodType: "ideal_billing_agreement",
               country: "us",
               partner: "webblends_inline",
               language: "en-US");

            ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

            Assert.IsNotNull(state.PaymentInstrument.ClientAction);
            Assert.AreEqual(ClientActionType.Redirect, state.PaymentInstrument.ClientAction.ActionType);
            Assert.AreEqual(testRedirectUrl, state.PaymentInstrument.ClientAction.Context);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.RedirectPidl);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.RedirectPidl, typeof(List<PIDLResource>));
            List<PIDLResource> redirectPidl = (List<PIDLResource>)state.PaymentInstrument.ClientAction.RedirectPidl;
            Assert.AreEqual(1, redirectPidl.Count);
            Assert.IsTrue(redirectPidl[0].Identity.ContainsKey("type"));
            Assert.AreEqual("idealredirectpidl", redirectPidl[0].Identity["type"]);
            Assert.IsNotNull(redirectPidl[0].DisplayPages);
            Assert.AreNotEqual(0, redirectPidl[0].DisplayPages.Count);
            Assert.AreEqual(0, redirectPidl[0].DisplayPages[0].Members.Count);
        }

        /// <summary>
        /// PaymentType filter: PaymentMethodType.IdealBillingAgreement
        /// ClientAction filter:
        /// Changes: ActionType=Redirect, Context=testRedirectUrl, RedirectPidl=PidlResourceDescriptionType.IdealBillingAgreementRedirectStaticPidl, ClearMembers
        /// </summary>
        [TestMethod]
        public void AddClientActionToPIIdealBillingAgreementOxoWebDirect()
        {
            string testRedirectUrl = "testRedirectUrl";

            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               pendingOn: string.Empty,
               redirectUrl: testRedirectUrl,
               paymentMethodType: "ideal_billing_agreement",
               country: "us",
               partner: "oxowebdirect",
               language: "en-US");

            ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

            Assert.IsNotNull(state.PaymentInstrument.ClientAction);
            Assert.AreEqual(ClientActionType.Redirect, state.PaymentInstrument.ClientAction.ActionType);
            Assert.AreEqual(testRedirectUrl, state.PaymentInstrument.ClientAction.Context);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.RedirectPidl);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.RedirectPidl, typeof(List<PIDLResource>));
            List<PIDLResource> redirectPidl = (List<PIDLResource>)state.PaymentInstrument.ClientAction.RedirectPidl;
            Assert.AreEqual(1, redirectPidl.Count);
            Assert.IsTrue(redirectPidl[0].Identity.ContainsKey("type"));
            Assert.AreEqual("idealredirectpidl", redirectPidl[0].Identity["type"]);
            Assert.IsNotNull(redirectPidl[0].DisplayPages);
            Assert.AreNotEqual(0, redirectPidl[0].DisplayPages.Count);
            Assert.AreEqual(0, redirectPidl[0].DisplayPages[0].Members.Count);
        }

        /// <summary>
        /// PaymentType filter: family=mobile_billing_non_sim
        /// ClientAction filter: PendingOn=PaymentInstrumentPendingOnTypes.Sms
        /// ClientAction filter exceptions: NullPendingOn, UnmatchedPendingOn
        /// Changes: ActionType=Pidl, Context=GetSmsChallengeDescriptionForPI
        /// </summary>
        [TestMethod]
        public void AddClientActionToPINonSimMobile()
        {
            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               pendingOn: "sms",
               redirectUrl: string.Empty,
               paymentMethodFamily: "mobile_billing_non_sim",
               country: "us",
               partner: "cart",
               language: "en-US");

            ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

            Assert.IsNotNull(state.PaymentInstrument.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, state.PaymentInstrument.ClientAction.ActionType);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.Context);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.Context, typeof(List<PIDLResource>));
            List<PIDLResource> context = (List<PIDLResource>)state.PaymentInstrument.ClientAction.Context;
            Assert.AreEqual(1, context.Count);
            Assert.IsTrue(context[0].Identity.ContainsKey("type"));
            Assert.AreEqual("sms", context[0].Identity["type"]);
        }

        /// <summary>
        /// PaymentType filter: family=mobile_billing_non_sim
        /// ClientAction filter: PendingOn=PaymentInstrumentPendingOnTypes.Sms
        /// ClientAction filter exceptions: NullPendingOn, UnmatchedPendingOn
        /// Changes: ActionType=Pidl, Context=GetSmsChallengeDescriptionForPI
        /// </summary>
        [TestMethod]
        public void AddClientActionToPINonSimMobileNullPendingOn()
        {
            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               pendingOn: null,
               redirectUrl: string.Empty,
               paymentMethodFamily: "mobile_billing_non_sim",
               country: "us",
               partner: "cart",
               language: "en-US");

            IntegrationException exception = null;

            try
            {
                ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);
            }
            catch (IntegrationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual("The state of the PI is set to pending but the pendingOn is null", exception.Message);
            Assert.AreEqual("InvalidPendingOnType", exception.ErrorCode);
        }

        /// <summary>
        /// PaymentType filter: family=mobile_billing_non_sim
        /// ClientAction filter: PendingOn=PaymentInstrumentPendingOnTypes.Sms
        /// ClientAction filter exceptions: NullPendingOn, UnmatchedPendingOn
        /// Changes: ActionType=Pidl, Context=GetSmsChallengeDescriptionForPI
        /// </summary>
        [TestMethod]
        public void AddClientActionToPINonSimMobileUnmatchedPendingOn()
        {
            string unmatchedPendingOnValue = "unmatchedPendingOnValue";
            string expectedErrorMessage = string.Format("The state of PI was expected to be pending on SMS. Actual state {0}", unmatchedPendingOnValue);

            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               pendingOn: unmatchedPendingOnValue,
               redirectUrl: string.Empty,
               paymentMethodFamily: "mobile_billing_non_sim",
               country: "us",
               partner: "cart",
               language: "en-US");

            IntegrationException exception = null;

            try
            {
                ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);
            }
            catch (IntegrationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual(expectedErrorMessage, exception.Message);
            Assert.AreEqual("InvalidPendingOnType", exception.ErrorCode);
        }

        /// <summary>
        /// PaymentType filter: family=mobile_billing_non_sim
        /// ClientAction filter: PendingOn=PaymentInstrumentPendingOnTypes.Sms
        /// ClientAction filter exceptions: NullPendingOn, UnmatchedPendingOn
        /// Changes: ActionType=Pidl, Context=GetSmsChallengeDescriptionForPI
        /// </summary>
        [TestMethod]
        public void AddClientActionToPICUPCC()
        {
            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               pendingOn: "sms",
               redirectUrl: string.Empty,
               paymentMethodType: "unionpay_creditcard",
               country: "us",
               partner: "cart",
               language: "en-US");

            ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

            Assert.IsNotNull(state.PaymentInstrument.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, state.PaymentInstrument.ClientAction.ActionType);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.Context);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.Context, typeof(List<PIDLResource>));
            List<PIDLResource> context = (List<PIDLResource>)state.PaymentInstrument.ClientAction.Context;
            Assert.AreEqual(1, context.Count);
            Assert.IsTrue(context[0].Identity.ContainsKey("type"));
            Assert.AreEqual("sms", context[0].Identity["type"]);
        }

        /// <summary>
        /// PaymentType filter: type=PaymentMethodType.UnionPayCreditCard | PaymentMethodType.UnionPayDebitCard
        /// ClientAction filter: PendingOn=PaymentInstrumentPendingOnTypes.Sms
        /// ClientAction filter exceptions: NullPendingOn, UnmatchedPendingOn
        /// Changes: ActionType=Pidl, Context=GetSmsChallengeDescriptionForPI
        /// </summary>
        [TestMethod]
        public void AddClientActionToPICUPDC()
        {
            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               pendingOn: "sms",
               redirectUrl: string.Empty,
               paymentMethodType: "unionpay_debitcard",
               country: "us",
               partner: "cart",
               language: "en-US");

            ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

            Assert.IsNotNull(state.PaymentInstrument.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, state.PaymentInstrument.ClientAction.ActionType);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.Context);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.Context, typeof(List<PIDLResource>));
            List<PIDLResource> context = (List<PIDLResource>)state.PaymentInstrument.ClientAction.Context;
            Assert.AreEqual(1, context.Count);
            Assert.IsTrue(context[0].Identity.ContainsKey("type"));
            Assert.AreEqual("sms", context[0].Identity["type"]);
        }

        /// <summary>
        /// PaymentType filter: type=PaymentMethodType.UnionPayCreditCard | PaymentMethodType.UnionPayDebitCard
        /// ClientAction filter: PendingOn=PaymentInstrumentPendingOnTypes.Sms
        /// ClientAction filter exceptions: NullPendingOn, UnmatchedPendingOn
        /// Changes: ActionType=Pidl, Context=GetSmsChallengeDescriptionForPI
        /// </summary>
        [TestMethod]
        public void AddClientActionToPICUPNullPendingOn()
        {
            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               pendingOn: null,
               redirectUrl: string.Empty,
               paymentMethodType: "unionpay_debitcard",
               country: "us",
               partner: "cart",
               language: "en-US");

            IntegrationException exception = null;

            try
            {
                ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);
            }
            catch (IntegrationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual("The state of the PI is set to pending but the pendingOn is null", exception.Message);
            Assert.AreEqual("InvalidPendingOnType", exception.ErrorCode);
        }

        /// <summary>
        /// PaymentType filter: type=PaymentMethodType.UnionPayCreditCard | PaymentMethodType.UnionPayDebitCard
        /// ClientAction filter: PendingOn=PaymentInstrumentPendingOnTypes.Sms
        /// ClientAction filter exceptions: NullPendingOn, UnmatchedPendingOn
        /// Changes: ActionType=Pidl, Context=GetSmsChallengeDescriptionForPI
        /// </summary>
        [TestMethod]
        public void AddClientActionToPICUPUnmatchedPendingOn()
        {
            string unmatchedPendingOnValue = "unmatchedPendingOnValue";
            string expectedErrorMessage = string.Format("The state of PI was expected to be pending on SMS. Actual state {0}", unmatchedPendingOnValue);

            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               pendingOn: unmatchedPendingOnValue,
               redirectUrl: string.Empty,
               paymentMethodType: "unionpay_debitcard",
               country: "us",
               partner: "cart",
               language: "en-US");

            IntegrationException exception = null;

            try
            {
                ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);
            }
            catch (IntegrationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual(expectedErrorMessage, exception.Message);
            Assert.AreEqual("InvalidPendingOnType", exception.ErrorCode);
        }

        /// <summary>
        /// PaymentType filter: type=PaymentMethodType.AlipayBillingAgreement
        /// ClientAction filter: PendingOn=PaymentInstrumentPendingOnTypes.Sms
        /// ClientAction filter exceptions: NullPendingOn, UnmatchedPendingOn
        /// Changes: ActionType=Pidl, Context=GetSmsChallengeDescriptionForPI
        /// </summary>
        [TestMethod]
        public void AddClientActionToPIAliPay()
        {
            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               pendingOn: "sms",
               redirectUrl: string.Empty,
               paymentMethodType: "alipay_billing_agreement",
               country: "us",
               partner: "cart",
               language: "en-US");

            ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

            Assert.IsNotNull(state.PaymentInstrument.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, state.PaymentInstrument.ClientAction.ActionType);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.Context);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.Context, typeof(List<PIDLResource>));
            List<PIDLResource> context = (List<PIDLResource>)state.PaymentInstrument.ClientAction.Context;
            Assert.AreEqual(1, context.Count);
            Assert.IsTrue(context[0].Identity.ContainsKey("type"));
            Assert.AreEqual("sms", context[0].Identity["type"]);
        }

        /// <summary>
        /// PaymentType filter: type=PaymentMethodType.AlipayBillingAgreement
        /// ClientAction filter: PendingOn=PaymentInstrumentPendingOnTypes.Sms
        /// ClientAction filter exceptions: NullPendingOn, UnmatchedPendingOn
        /// Changes: ActionType=Pidl, Context=GetSmsChallengeDescriptionForPI
        /// </summary>
        [TestMethod]
        public void AddClientActionToPIAliPayNullPendingOn()
        {
            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               pendingOn: null,
               redirectUrl: string.Empty,
               paymentMethodType: "alipay_billing_agreement",
               country: "us",
               partner: "cart",
               language: "en-US");

            IntegrationException exception = null;

            try
            {
                ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);
            }
            catch (IntegrationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual("The state of the PI is set to pending but the pendingOn is null", exception.Message);
            Assert.AreEqual("InvalidPendingOnType", exception.ErrorCode);
        }

        /// <summary>
        /// PaymentType filter: type=PaymentMethodType.AlipayBillingAgreement
        /// ClientAction filter: PendingOn=PaymentInstrumentPendingOnTypes.Sms
        /// ClientAction filter exceptions: NullPendingOn, UnmatchedPendingOn
        /// Changes: ActionType=Pidl, Context=GetSmsChallengeDescriptionForPI
        /// </summary>
        [TestMethod]
        public void AddClientActionToPIAliPayUnmatchedPendingOn()
        {
            string unmatchedPendingOn = "unmatchedPendingOnValue";
            string expectedErrorMessage = string.Format("The state of PI was expected to be pending on SMS. Actual state {0}", unmatchedPendingOn);

            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               pendingOn: unmatchedPendingOn,
               redirectUrl: string.Empty,
               paymentMethodType: "alipay_billing_agreement",
               country: "us",
               partner: "cart",
               language: "en-US");

            IntegrationException exception = null;

            try
            {
                ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);
            }
            catch (IntegrationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual(expectedErrorMessage, exception.Message);
            Assert.AreEqual("InvalidPendingOnType", exception.ErrorCode);
        }

        /// <summary>
        /// PaymentType filter: family=ewallet, type=PaymentMethodType.PayPal
        /// ClientAction filter: PendingOn=PaymentInstrumentPendingOnTypes.AgreementUpdate, requestType=RequestType.GetPI
        /// ClientAction filter exceptions:
        /// Changes: ActionType=Pidl, Context=GetUpdateAgreementChallengeDescriptionForPI, PidlResourceDescriptionType.PaypalUpdateAgreementChallenge
        /// </summary>
        [TestMethod]
        public void AddClientActionToPIPaypalPendingOnAgreementUpdate()
        {
            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               pendingOn: "agreementUpdate",
               redirectUrl: string.Empty,
               paymentMethodFamily: "ewallet",
               paymentMethodType: "paypal",
               country: "us",
               partner: "cart",
               language: "en-US",
               requestType: "getPI");

            ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

            Assert.IsNotNull(state.PaymentInstrument.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, state.PaymentInstrument.ClientAction.ActionType);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.Context);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.Context, typeof(List<PIDLResource>));
            List<PIDLResource> context = (List<PIDLResource>)state.PaymentInstrument.ClientAction.Context;
            Assert.AreEqual(1, context.Count);
            Assert.IsTrue(context[0].Identity.ContainsKey("type"));
            Assert.AreEqual("paypalUpdateAgreementChallenge".ToLower(), context[0].Identity["type"]);
        }

        /// <summary>
        /// PaymentType filter: family=ewallet, type=PaymentMethodType.PayPal
        /// ClientAction filter: PendingOn=!PaymentInstrumentPendingOnTypes.AgreementUpdate, requestType=RequestType.GetPI
        /// ClientAction filter exceptions:
        /// Changes: ActionType=Pidl, Context=GetStaticPidlDescriptions, PidlResourceDescriptionType.PaypalRetryStaticPidl
        /// </summary>
        [TestMethod]
        public void AddClientActionToPIPaypalPendingOnNotAgreementUpdate()
        {
            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               pendingOn: "notAgreementUpdate",
               redirectUrl: string.Empty,
               paymentMethodFamily: "ewallet",
               paymentMethodType: "paypal",
               country: "us",
               partner: "cart",
               language: "en-US",
               requestType: "getPI");

            ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

            Assert.IsNotNull(state.PaymentInstrument.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, state.PaymentInstrument.ClientAction.ActionType);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.Context);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.Context, typeof(List<PIDLResource>));
            List<PIDLResource> context = (List<PIDLResource>)state.PaymentInstrument.ClientAction.Context;
            Assert.AreEqual(1, context.Count);
            Assert.IsTrue(context[0].Identity.ContainsKey("type"));
            Assert.AreEqual("paypalRetryStatic".ToLower(), context[0].Identity["type"]);
        }

        /// <summary>
        /// PaymentType filter: family=ewallet, type=PaymentMethodType.PayPal
        /// ClientAction filter: requestType=!RequestType.GetPI
        /// ClientAction filter exceptions:
        /// Changes: ActionType=Redirect, Context=RedirectServiceLink, PidlResourceDescriptionType.PaypalRedirectStaticPidl, ClearMembers
        /// </summary>
        [TestMethod]
        public void AddClientActionToPIPaypalRedirectInlinePartner()
        {
            string testRedirectUrl = "testRedirectUrl";

            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
               redirectUrl: testRedirectUrl,
               paymentMethodFamily: "ewallet",
               paymentMethodType: "paypal",
               country: "us",
               partner: "cart",
               language: "en-US",
               requestType: "notGetPI");

            ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

            Assert.IsNotNull(state.PaymentInstrument.ClientAction);
            Assert.AreEqual(ClientActionType.Redirect, state.PaymentInstrument.ClientAction.ActionType);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.Context);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.Context, typeof(RedirectionServiceLink));
            RedirectionServiceLink context = (RedirectionServiceLink)state.PaymentInstrument.ClientAction.Context;
            Assert.AreEqual(testRedirectUrl, context.BaseUrl);
            Assert.IsNull(state.PaymentInstrument.ClientAction.RedirectPidl);
        }

        /// <summary>
        /// PaymentType filter: type=PaymentMethodType.Ach
        /// ClientAction filter: PendingOn=PaymentInstrumentPendingOnTypes.Picv, requestType=RequestType.AddPI, partner=!cart
        /// ClientAction filter exceptions:
        /// Changes: ActionType=Pidl, Context=GetStaticPidlDescriptions, PidlResourceDescriptionType.AchPicVStatic
        /// </summary>
        [TestMethod]
        public void AddClientActionToPIAchPendingOnPicvAddPIRequest()
        {
            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
                pendingOn: "picv",
                redirectUrl: string.Empty,
                paymentMethodType: "ach",
                country: "us",
                partner: "webblends",
                language: "en-US",
                requestType: "addPI");

            ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

            Assert.IsNotNull(state.PaymentInstrument.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, state.PaymentInstrument.ClientAction.ActionType);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.Context);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.Context, typeof(List<PIDLResource>));
            List<PIDLResource> context = (List<PIDLResource>)state.PaymentInstrument.ClientAction.Context;
            Assert.AreEqual(1, context.Count);
            Assert.IsTrue(context[0].Identity.ContainsKey("type"));
            Assert.AreEqual("achPicvStatic".ToLower(), context[0].Identity["type"]);
        }

        /// <summary>
        /// PaymentType filter: type=PaymentMethodType.Ach
        /// ClientAction filter: PendingOn=PaymentInstrumentPendingOnTypes.Picv, requestType=RequestType.GetPI
        /// ClientAction filter exceptions:
        /// Changes: ActionType=Pidl, Context=GetPicvChallengeDescriptionForPI, PidlResourceDescriptionType.AchPicVChallenge
        /// </summary>
        [TestMethod]
        public void AddClientActionToPIAchPendingOnPicvGetPIRequest()
        {
            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
                pendingOn: "picv",
                redirectUrl: string.Empty,
                paymentMethodType: "ach",
                country: "us",
                partner: "webblends",
                language: "en-US",
                requestType: "getPI",
                picvRemainingRetries: "1");

            ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

            Assert.IsNotNull(state.PaymentInstrument.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, state.PaymentInstrument.ClientAction.ActionType);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.Context);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.Context, typeof(List<PIDLResource>));
            List<PIDLResource> context = (List<PIDLResource>)state.PaymentInstrument.ClientAction.Context;
            Assert.AreEqual(1, context.Count);
            Assert.IsTrue(context[0].Identity.ContainsKey("type"));
            Assert.AreEqual("ach_picv".ToLower(), context[0].Identity["type"]);
        }

        /// <summary>
        /// PaymentType filter: type=PaymentMethodType.Ach
        /// ClientAction filter: PendingOn=PaymentInstrumentPendingOnTypes.Picv, requestType=RequestType.GetPI | AddPI
        /// ClientAction filter exceptions: UnmatchedRequestType
        /// Changes:
        /// </summary>
        [TestMethod]
        public void AddClientActionToPIAchUnmatchedRequestType()
        {
            string unmatchedRequestType = "unmatchedRequestTypeValue";
            string expectedErrorMessage = string.Format("The operation type of PI was expected to be addPI or getPI. Actual operation type {0}", unmatchedRequestType);

            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
                pendingOn: "picv",
                redirectUrl: string.Empty,
                paymentMethodType: "ach",
                country: "us",
                partner: "cart",
                language: "en-US",
                requestType: unmatchedRequestType);

            IntegrationException exception = null;

            try
            {
                ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);
            }
            catch (IntegrationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual(expectedErrorMessage, exception.Message);
            Assert.AreEqual("InvalidPendingOnType", exception.ErrorCode);
        }

        /// <summary>
        /// PaymentType filter: type=PaymentMethodType.Sepa
        /// ClientAction filter: PendingOn=PaymentInstrumentPendingOnTypes.Picv
        /// ClientAction filter exceptions: UnmatchedPendingOn
        /// Changes: ActionType=Pidl, Context=GetPicvChallengeDescriptionForPI, PidlResourceDescriptionType.SepaPicVChallenge
        /// </summary>
        [TestMethod]
        public void AddClientActionToPISepa()
        {
            List<string> partners = new List<string> { "cart", "defaulttemplate" };

            foreach (string partner in partners)
            {
                ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
                pendingOn: "picv",
                redirectUrl: string.Empty,
                paymentMethodType: "sepa",
                country: "us",
                partner: partner,
                language: "en-US",
                picvRemainingRetries: "1");

                ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

                Assert.IsNotNull(state.PaymentInstrument.ClientAction);
                Assert.AreEqual(ClientActionType.Pidl, state.PaymentInstrument.ClientAction.ActionType);
                Assert.IsNotNull(state.PaymentInstrument.ClientAction.Context);
                Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.Context, typeof(List<PIDLResource>));
                List<PIDLResource> context = (List<PIDLResource>)state.PaymentInstrument.ClientAction.Context;
                Assert.AreEqual(1, context.Count);
                Assert.IsTrue(context[0].Identity.ContainsKey("type"));
                Assert.AreEqual("sepa_picv", context[0].Identity["type"]);
            }
        }

        /// <summary>
        /// PaymentType filter: PaymentMethodType.Sepa
        /// ClientAction filter: pendingOn=redirect
        /// ClientAction filter exceptions: UnmatchedPendingOn
        /// Changes: ActionType=Redirect, Context=RedirectServiceLink, PidlResourceDescriptionType.SepaPicVStatic, ClearMembers
        /// </summary>
        [TestMethod]
        public void AddClientActionToPISepaRedirectWebblendsInlinePartner()
        {
            string testRedirectUrl = "testRedirectUrl";

            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
                pendingOn: "redirect",
                redirectUrl: testRedirectUrl,
                paymentMethodType: "sepa",
                country: "us",
                partner: "webblends_inline",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

            Assert.IsNotNull(state.PaymentInstrument.ClientAction);
            Assert.AreEqual(ClientActionType.Redirect, state.PaymentInstrument.ClientAction.ActionType);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.Context);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.Context, typeof(RedirectionServiceLink));
            RedirectionServiceLink context = (RedirectionServiceLink)state.PaymentInstrument.ClientAction.Context;
            Assert.AreEqual(testRedirectUrl, context.BaseUrl);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.RedirectPidl);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.RedirectPidl, typeof(List<PIDLResource>));
            List<PIDLResource> redirectPidl = (List<PIDLResource>)state.PaymentInstrument.ClientAction.RedirectPidl;
            Assert.AreEqual(1, redirectPidl.Count);
            Assert.IsTrue(redirectPidl[0].Identity.ContainsKey("type"));
            Assert.AreEqual("sepaPicvStatic".ToLower(), redirectPidl[0].Identity["type"]);
            Assert.IsNotNull(redirectPidl[0].DisplayPages);
            Assert.AreNotEqual(0, redirectPidl[0].DisplayPages.Count);
            Assert.AreEqual(0, redirectPidl[0].DisplayPages[0].Members.Count);
        }

        /// <summary>
        /// PaymentType filter: PaymentMethodType.Sepa
        /// ClientAction filter: pendingOn=redirect
        /// ClientAction filter exceptions: UnmatchedPendingOn
        /// Changes: ActionType=Redirect, Context=RedirectServiceLink, PidlResourceDescriptionType.SepaPicVStatic, ClearMembers
        /// </summary>
        [TestMethod]
        public void AddClientActionToPISepaRedirectOxoWebDirectPartner()
        {
            string testRedirectUrl = "testRedirectUrl";

            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
                pendingOn: "redirect",
                redirectUrl: testRedirectUrl,
                paymentMethodType: "sepa",
                country: "us",
                partner: "oxowebdirect",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

            Assert.IsNotNull(state.PaymentInstrument.ClientAction);
            Assert.AreEqual(ClientActionType.Redirect, state.PaymentInstrument.ClientAction.ActionType);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.Context);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.Context, typeof(RedirectionServiceLink));
            RedirectionServiceLink context = (RedirectionServiceLink)state.PaymentInstrument.ClientAction.Context;
            Assert.AreEqual(testRedirectUrl, context.BaseUrl);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.RedirectPidl);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.RedirectPidl, typeof(List<PIDLResource>));
            List<PIDLResource> redirectPidl = (List<PIDLResource>)state.PaymentInstrument.ClientAction.RedirectPidl;
            Assert.AreEqual(1, redirectPidl.Count);
            Assert.IsTrue(redirectPidl[0].Identity.ContainsKey("type"));
            Assert.AreEqual("sepaPicvStatic".ToLower(), redirectPidl[0].Identity["type"]);
            Assert.IsNotNull(redirectPidl[0].DisplayPages);
            Assert.AreNotEqual(0, redirectPidl[0].DisplayPages.Count);
            Assert.AreEqual(0, redirectPidl[0].DisplayPages[0].Members.Count);
        }

        /// <summary>
        /// PaymentType filter: type=PaymentMethodType.Sepa
        /// ClientAction filter: PendingOn=PaymentInstrumentPendingOnTypes.Picv | Redirect
        /// ClientAction filter exceptions: UnmatchedPendingOn
        /// Changes:
        /// </summary>
        [TestMethod]
        public void AddClientActionToPISepaUnmatchedPendingOn()
        {
            string unmatchedPendingOnValue = "unmatchedPendingOnValue";
            string expectedErrorMessage = string.Format("The state of PI was expected to be pending on PICV or Redirect. Actual state {0}", unmatchedPendingOnValue);

            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
                pendingOn: unmatchedPendingOnValue,
                redirectUrl: string.Empty,
                paymentMethodType: "sepa",
                country: "us",
                partner: "cart",
                language: "en-US");

            IntegrationException exception = null;

            try
            {
                ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);
            }
            catch (IntegrationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual(expectedErrorMessage, exception.Message);
            Assert.AreEqual("InvalidPendingOnType", exception.ErrorCode);
        }

        /// <summary>
        /// PaymentType filter: family=credit_card, type=PaymentMethodType.CreditCardAmericanExpress, country=in
        /// ClientAction filter:
        /// ClientAction filter exceptions:
        /// Changes: ActionType=Redirect, Context=RedirectServiceLink, PidlResourceDescriptionType.PaypalRedirectStaticPidl, ClearMembers
        /// </summary>
        [TestMethod]
        public void AddClientActionToPICCRedirectInlinePartner()
        {
            string testRedirectUrl = "testRedirectUrl";

            ResourceLifecycleStateManager.ClientActionResourceState state = BuildInitialState(
                redirectUrl: testRedirectUrl,
                paymentMethodFamily: "credit_card",
                paymentMethodType: "amex",
                country: "in",
                partner: "cart",
                language: "en-US");

            ResourceLifecycleStateManager.Instance.SetClientAction(ResourceLifecycleStateManager.ClientActionResourceAction.AddClientActionToPI, state);

            Assert.IsNotNull(state.PaymentInstrument.ClientAction);
            Assert.AreEqual(ClientActionType.Redirect, state.PaymentInstrument.ClientAction.ActionType);
            Assert.IsNotNull(state.PaymentInstrument.ClientAction.Context);
            Assert.IsInstanceOfType(state.PaymentInstrument.ClientAction.Context, typeof(RedirectionServiceLink));
            RedirectionServiceLink context = (RedirectionServiceLink)state.PaymentInstrument.ClientAction.Context;
            Assert.AreEqual(testRedirectUrl, context.BaseUrl);
            Assert.IsNull(state.PaymentInstrument.ClientAction.RedirectPidl);
        }

        private ResourceLifecycleStateManager.ClientActionResourceState BuildInitialState(
            string pendingOn = "testPendingOn",
            string redirectUrl = "testRedirectUrl",
            JObject pendingDetails = null,
            string paymentMethodFamily = "testPaymentMethodFamily",
            string paymentMethodType = "testPaymentMethodType",
            string accountId = "testAccountId",
            string country = "testCountry",
            string partner = "default",
            string language = "testLanguages",
            string classicProduct = "testClassicProduct",
            bool completePrerequisites = false,
            string requestType = "testRequestType",
            string picvRemainingRetries = null)
        {
            return new ResourceLifecycleStateManager.ClientActionResourceState(
                new PaymentInstrument()
                {
                    PaymentInstrumentId = "testPaymentInstrumentId",
                    PaymentInstrumentDetails = new PaymentInstrumentDetails()
                    {
                        PendingOn = pendingOn,
                        RedirectUrl = redirectUrl,
                        PendingDetails = pendingDetails,
                        PicvRequired = false,
                        PicvDetails = new PaymentInstrumentDetails.PicvDetailsInfo()
                        {
                            RemainingAttempts = picvRemainingRetries
                        }
                    },
                    PaymentMethod = new PaymentMethod()
                    {
                        PaymentMethodFamily = paymentMethodFamily,
                        PaymentMethodType = paymentMethodType,
                    },
                },
                accountId,
                "testBillableAccountId",
                country,
                partner,
                language,
                classicProduct,
                completePrerequisites,
                "testEmail",
                "testPidlBaseUrl",
                requestType,
                new Microsoft.Commerce.Tracing.EventTraceActivity());
        }

        private ResourceLifecycleStateManager.ErrorResourceState BuildInitialErrorState(
            string errorCode,
            string errorMessage = "[]",
            string accountId = "testAccountId",
            string billableAccountId = "testBillableAccountId",
            string paymentMethodFamily = "testPaymentMethodFamily",
            string paymentMethodType = "testPaymentMethodType",
            string country = "testCountry",
            string partner = "default",
            string language = "testLanguages",
            string classicProduct = "testClassicProduct",
            bool completePrerequisites = false,
            string piid = "testPiid")
        {
            ServiceErrorResponseException ex = new ServiceErrorResponseException()
            {
                Error = new Microsoft.Commerce.Payments.PXService.ServiceErrorResponse(errorCode, errorMessage),
            };

            return new ResourceLifecycleStateManager.ErrorResourceState(
                ref ex,
                accountId,
                billableAccountId,
                paymentMethodFamily,
                paymentMethodType,
                country,
                partner,
                language,
                classicProduct,
                completePrerequisites,
                new PaymentInstrument()
                {
                    PaymentMethod = new PaymentMethod()
                    {
                        PaymentMethodFamily = paymentMethodFamily,
                        PaymentMethodType = paymentMethodType,
                    }
                },
                piid,
                new Microsoft.Commerce.Tracing.EventTraceActivity());
        }
    }
}
