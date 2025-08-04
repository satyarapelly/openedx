// <copyright file="PaymentSessionsHandler.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.WebUtilities;
    using Common.Environments;
    using Common.Transaction;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Accessors.TransactionDataService;
    using Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService;
    using Microsoft.Commerce.Payments.PXService.Model.TransactionService;
    using Model;
    using Newtonsoft.Json;
    using PayerAuth = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService;
    using Purchase = PXService.Model.PurchaseService;
    using PXInternal = Microsoft.Commerce.Payments.PXService.Model.PXInternal;
    using System.Diagnostics.Tracing;

    public class PaymentSessionsHandler
    {
        public static readonly string HandlerVersion = "V1";

        protected const string DefaultPSD2SettingsVersion = "V11";
        protected const string PSD2SettingVersionFlightRegex = @"^\s*PXPSD2SettingVersionV(?'versionNumber'\d{1,4})\s*$";

        private static readonly Dictionary<ChallengeWindowSize, WindowSize> ChallengeWindowSizes = new Dictionary<ChallengeWindowSize, WindowSize>()
        {
            { ChallengeWindowSize.One, new WindowSize() { Width = "250px", Height = "400px" } },
            { ChallengeWindowSize.Two, new WindowSize() { Width = "390px", Height = "400px" } },
            { ChallengeWindowSize.Three, new WindowSize() { Width = "500px", Height = "600px" } },
            { ChallengeWindowSize.Four, new WindowSize() { Width = "600px", Height = "400px" } },
            { ChallengeWindowSize.Five, new WindowSize() { Width = "100%", Height = "100%" } },
        };

        private PXInternal.PaymentSession storedSession = null;

        public PaymentSessionsHandler(
            IPayerAuthServiceAccessor payerAuthServiceAccessor,
            IPIMSAccessor pimsAccessor,
            ISessionServiceAccessor sessionServiceAccessor,
            IAccountServiceAccessor accountServiceAccessor,
            IPurchaseServiceAccessor purchaseServiceAccessor,
            ITransactionServiceAccessor transactionServiceAccessor,
            ITransactionDataServiceAccessor transactionDataServiceAccessor,
            string pifdBaseUrl)
        {
            this.PayerAuthServiceAccessor = payerAuthServiceAccessor;
            this.PimsAccessor = pimsAccessor;
            this.SessionServiceAccessor = sessionServiceAccessor;
            this.AccountServiceAccessor = accountServiceAccessor;
            this.PurchaseServiceAccessor = purchaseServiceAccessor;
            this.TransactionServiceAccessor = transactionServiceAccessor;
            this.TransactionDataServiceAccessor = transactionDataServiceAccessor;
            this.PifdBaseUrl = pifdBaseUrl;
        }

        protected string PifdBaseUrl { get; set; }

        protected IPIMSAccessor PimsAccessor { get; set; }

        protected ISessionServiceAccessor SessionServiceAccessor { get; set; }

        protected IPayerAuthServiceAccessor PayerAuthServiceAccessor { get; set; }

        protected IAccountServiceAccessor AccountServiceAccessor { get; set; }

        protected IPurchaseServiceAccessor PurchaseServiceAccessor { get; set; }

        protected ITransactionServiceAccessor TransactionServiceAccessor { get; set; }

        protected ITransactionDataServiceAccessor TransactionDataServiceAccessor { get; set; }

        public static void ValidateSettingsVersion(AuthenticationRequest authRequest, List<string> exposedFlightFeatures)
        {
            string targetSettingsVersion = GetLatestAvailablePSD2SettingVersion(exposedFlightFeatures);

            // On 2nd or subsequent TryCount, dont throw this exception even if there is a mismatch. This is to allow
            // the client to attempt Authentication if it fails to download the target settings version for some reason.
            if (string.Equals(targetSettingsVersion, authRequest.SettingsVersion, StringComparison.OrdinalIgnoreCase) == false
                && authRequest.SettingsVersionTryCount == 1)
            {
                throw new ValidationException(ErrorCode.SettingsVersionMismatch, targetSettingsVersion, string.Empty);
            }
        }

        public static Uri GetChallengeRedirectUriFromPaymentSession(PaymentSession paymentSession)
        {
            string url = (paymentSession.ChallengeStatus == PaymentChallengeStatus.Succeeded) ? paymentSession.SuccessUrl : paymentSession.FailureUrl;

            var uri = new Uri(url);
            var existing = QueryHelpers.ParseQuery(uri.Query);
            var builder = new QueryBuilder();
            foreach (var kvp in existing)
            {
                foreach (var value in kvp.Value)
                {
                    builder.Add(kvp.Key, value);
                }
            }

            builder.Add("challengeStatus", paymentSession.ChallengeStatus.ToString());

            if (paymentSession.ChallengeStatus == PaymentChallengeStatus.Succeeded)
            {
                builder.Add("sessionId", paymentSession.Id);
                builder.Add("piid", paymentSession.PaymentInstrumentId);
            }
            else
            {
                var errorCode = V7.Constants.PSD2ErrorCodes.RejectedByProvider;
                if (paymentSession.ChallengeStatus == PaymentChallengeStatus.InternalServerError)
                {
                    errorCode = V7.Constants.ThreeDSErrorCodes.InternalServerError;
                }

                builder.Add("errorCode", errorCode);
                builder.Add("errorMessage", paymentSession.ChallengeStatus.ToString());

                if (!string.IsNullOrEmpty(paymentSession.UserDisplayMessage))
                {
                    builder.Add("userDisplayMessage", paymentSession.UserDisplayMessage);
                }
            }

            var uriBuilder = new UriBuilder(uri)
            {
                Query = builder.ToQueryString().Value.TrimStart('?')
            };
            return uriBuilder.Uri;
        }

        public static string GetTransactionServiceStore(string partner, PaymentExperienceSetting setting)
        {
            string store = null;
            if (setting != null)
            {
                if (PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseOmsTransactionServiceStore, null, setting))
                {
                    store = V7.Constants.TransationServiceStore.OMS;
                }
                else if (PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseAzureTransactionServiceStore, null, setting))
                {
                    store = V7.Constants.TransationServiceStore.Azure;
                }
            }

            if (string.IsNullOrEmpty(store))
            {
                if (PartnerHelper.IsAzurePartner(partner))
                {
                    store = V7.Constants.TransationServiceStore.Azure;
                }
                else if (string.Equals(V7.Constants.PartnerName.CommercialStores, partner, StringComparison.OrdinalIgnoreCase))
                {
                    store = V7.Constants.TransationServiceStore.OMS;
                }
            }

            return store ?? V7.Constants.TransationServiceStore.Azure;
        }

        /// <summary>
        /// Used by both Browser flow and App flow
        /// Create Payment Session Object
        /// </summary>
        /// <param name="accountId">Pi Account Id</param>
        /// <param name="paymentSessionData">The context to create PaymentSession object</param>
        /// <param name="deviceChannel">Sepcifies whether this call is from the App flow or the browser flow</param>
        /// <param name="emailAddress">Used for risk assessment during ValidatePI call</param>
        /// <param name="exposedFlightFeatures">Exposed flight features</param>'
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <param name="testContext">Test Context</param>
        /// <param name="isMotoAuthorized">Whether the request is authorized for moto scenario</param>
        /// <param name="tid">Org tid</param>
        /// <param name="setting">payment experience partner setting</param>
        /// <param name="userId">Incoming user PUID</param>
        /// <param name="isGuestUser">Guest user</param>
        /// <param name="requestContext">Request context for payment or checkout request</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns the data ThreeDSMethodPreparationResult for PIDL to consume </returns>
        public virtual async Task<PaymentSession> CreatePaymentSession(
            string accountId,
            PaymentSessionData paymentSessionData,
            DeviceChannel deviceChannel,
            string emailAddress,
            List<string> exposedFlightFeatures,
            EventTraceActivity traceActivityId,
            TestContext testContext,
            string isMotoAuthorized = null,
            string tid = null,
            PaymentExperienceSetting setting = null,
            string userId = null,
            bool isGuestUser = false,
            V7.Contexts.RequestContext requestContext = null)
        {
            PaymentSession session = new PaymentSession(paymentSessionData);
            PaymentInstrument paymentInstrument = null;

            // Remove the following check once ThreeDS Provider MC Prod is ready.
            if (!IsPSD2EnabledInPartnerSetting(setting) &&
                (exposedFlightFeatures == null || !exposedFlightFeatures.Contains(Flighting.Features.PXPSD2ProdIntegration, StringComparer.OrdinalIgnoreCase)) &&
                Common.Environments.Environment.Current.EnvironmentType != EnvironmentType.Integration &&
                Common.Environments.Environment.Current.EnvironmentType != EnvironmentType.OneBox &&
                !HttpRequestHelper.HasAnyPSD2TestScenarios(testContext))
            {
                session.IsChallengeRequired = false;
                session.Id = Guid.NewGuid().ToString();
                session.ChallengeStatus = PaymentChallengeStatus.NotApplicable;
                session.Signature = session.GenerateSignature();
                return session;
            }

            // Verify Moto and Pi ownership before creating a session
            VerifyMOTOAuthorization(
                sessionData: paymentSessionData,
                isMotoAuthorized: isMotoAuthorized);

            // 0. GetExtendedPI from PIMS to see if PI is creditcard or not. We are using this API instead of GetPI to bypass Authorization checks on the PI
            if ((exposedFlightFeatures != null && V7.Constants.PSD2IgnorePIAuthorizationPartners.Contains(paymentSessionData.Partner, StringComparer.OrdinalIgnoreCase))
                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.PSD2, session.Country, setting, Constants.DisplayCustomizationDetail.Psd2IgnorePIAuthorization))
            {
                // The check for commercialstores should be done before accountId, piid ownership check. It is because commercial stores has my-org scenario
                // to do purchase against SEPA or ACH. To avoid failure for SEPA and ACH, return "NotApplicable" before we do ownership check.
                if (await CallSafetyNetOperation(
                async () =>
                {
                    paymentInstrument = await this.PimsAccessor.GetExtendedPaymentInstrument(
                        piid: session.PaymentInstrumentId,
                        traceActivityId: traceActivityId,
                        paymentSessionData.Partner,
                        exposedFlightFeatures: exposedFlightFeatures);
                },
                traceActivityId,
                exposedFeatures: exposedFlightFeatures,
                excludeErrorFeatureFormat: "PSD2SafetyNet-GetPIExt-{0}-{1}"))
                {
                    return GetSafetyNetPaymentSession(paymentSessionData);
                }

                if (PaymentMethodRequiresPaymentChallenge(paymentInstrument, exposedFlightFeatures))
                {
                    session.IsChallengeRequired = false;
                    session.Id = Guid.NewGuid().ToString();
                    session.ChallengeStatus = PaymentChallengeStatus.NotApplicable;
                    session.Signature = session.GenerateSignature();
                    return session;
                }
            }

            // 1. GetPI to verify paymentInstrumentAccountId owns paymentInstrumentId (GetExtendedPI does not verify ownership)
            try
            {
                PaymentInstrument piDetails = await this.PimsAccessor.GetPaymentInstrument(
                    accountId: accountId,
                    piid: session.PaymentInstrumentId,
                    traceActivityId: traceActivityId,
                    setting: setting);

                if (PaymentMethodRequiresPaymentChallenge(piDetails, exposedFlightFeatures))
                {
                    session.IsChallengeRequired = false;
                    session.Id = Guid.NewGuid().ToString();
                    session.ChallengeStatus = PaymentChallengeStatus.NotApplicable;
                    session.Signature = session.GenerateSignature();
                    return session;
                }
            }
            catch (ServiceErrorResponseException ex)
            {
                if (ex.Error?.ErrorCode == "AccountPINotFound")
                {
                    throw new ValidationException(
                        ErrorCode.PaymentInstrumentNotFound,
                        "Caller is not authorized to access specified PaymentInstrumentId");
                }
                else if (ex.Error?.InnerError?.ErrorCode == "InvalidQueryStringParameter"
                    && ex.Error?.InnerError?.Target == "AccountService")
                {
                    throw new ValidationException(ErrorCode.InvalidAccountId, "Specified PaymentInstrumentAccountId was not found");
                }

                throw;
            }

            // 2. GetExtendedPI from PIMS to see if challenge may be required
            bool piRequires3ds1Authentication = false;
            bool piRequires3ds2Authentication = false;
            bool piIsIssuedIn3ds1RequiredCountry = false;

            if (await CallSafetyNetOperation(
                async () =>
                {
                    paymentInstrument = await this.PimsAccessor.GetExtendedPaymentInstrument(
                        piid: session.PaymentInstrumentId,
                        traceActivityId: traceActivityId,
                        paymentSessionData.Partner,
                        exposedFlightFeatures: exposedFlightFeatures);

                    List<string> challengeList = paymentInstrument?.PaymentInstrumentDetails?.RequiredChallenge;

                    // For India flows, check another flight. This will be removed later.
                    piIsIssuedIn3ds1RequiredCountry = challengeList != null && challengeList.Contains("3ds")
                        && ((exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.India3dsEnableForBilldesk, StringComparer.OrdinalIgnoreCase)) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ThreeDSOne, session.Country, setting, Constants.DisplayCustomizationDetail.India3dsEnableForBilldesk));

                    piRequires3ds1Authentication = piIsIssuedIn3ds1RequiredCountry
                                                    && string.Equals(session.Country, "IN", StringComparison.OrdinalIgnoreCase)
                                                    && string.Equals(session.Currency, "INR", StringComparison.OrdinalIgnoreCase);
                    piRequires3ds2Authentication = challengeList != null && challengeList.Contains("3ds2");
                },
                traceActivityId,
                exposedFeatures: exposedFlightFeatures,
                excludeErrorFeatureFormat: "PSD2SafetyNet-GetPIExt-{0}-{1}"))
            {
                return GetSafetyNetPaymentSession(paymentSessionData);
            }

            // Guest checkout user
            if (isGuestUser
                && paymentInstrument?.PaymentInstrumentDetails?.UsageType == UsageType.Inline
                && !string.IsNullOrWhiteSpace(paymentInstrument?.PaymentInstrumentDetails?.TransactionLink?.LinkedPaymentSessionId))
            {
                // For 1PP Guest checkout
                session.IsChallengeRequired = false;
                session.ChallengeStatus = PaymentChallengeStatus.NotApplicable;
                session.Id = paymentInstrument.PaymentInstrumentDetails.TransactionLink.LinkedPaymentSessionId;
                session.Signature = session.GenerateSignature();
                return session;
            }

            // Since Amex is not enabled through billdesk in India market yet, we need to continue processing amex with adyen, so we need to disable 2FA for Amex until Amex is enabled through billdesk
            // Once Amex is enabled through billdesk, we need to enable 2FA for Amex. Created task 38121719 to track this.
            if (piRequires3ds1Authentication
                && paymentInstrument.IsCreditCard()
                && ((!((exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnableIndia3DS1Challenge, StringComparer.OrdinalIgnoreCase)) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ThreeDSOne, session.Country, setting, Constants.DisplayCustomizationDetail.PXEnableIndia3DS1Challenge)))
                    || (paymentInstrument.PaymentMethod.IsCreditCardAmex()
                    && string.Equals(session.Country, "IN", StringComparison.OrdinalIgnoreCase))))
            {
                piRequires3ds1Authentication = false;
            }

            if (piRequires3ds1Authentication == false
                && paymentInstrument.IsCreditCard()
                && HttpRequestHelper.HasAnyThreeDSOneTestScenarios(testContext))
            {
                piRequires3ds1Authentication = true;
                piRequires3ds2Authentication = false;
            }

            if (piRequires3ds2Authentication == false
                && paymentInstrument.IsCreditCard()
                && ((exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXPSD2PretendPIMSReturned3DS2, StringComparer.OrdinalIgnoreCase))
                    || HttpRequestHelper.HasE2EPSD2TestScenarios(testContext)))
            {
                piRequires3ds2Authentication = true;

                // Because 3ds1 and PSD2 share the same PSD2 test header, when the PSD2 test header passed together with "EnableThreeDSOne" partner flight, both 3ds1 and PSD2 are enabled, which is not expected
                // Therefore, set piRequires3ds2Authentication to be false when both PSD2 test header and "EnableThreeDSOne" partner flight are sent from partner
                if ((exposedFlightFeatures.Contains(Flighting.Features.PXEnableIndia3DS1Challenge, StringComparer.OrdinalIgnoreCase) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ThreeDSOne, session.Country, setting, Constants.DisplayCustomizationDetail.PXEnableIndia3DS1Challenge)) && string.Equals(session.Country, "IN", StringComparison.OrdinalIgnoreCase))
                {
                    piRequires3ds2Authentication = false;
                }
            }

            if (piRequires3ds1Authentication == false
                && paymentInstrument.IsCreditCard()
                && HttpRequestHelper.HasE2EPSD2TestScenarios(testContext)
                && string.Equals(session.Country, "IN", StringComparison.OrdinalIgnoreCase))
            {
                piRequires3ds1Authentication = true;
                piRequires3ds2Authentication = false;

                if ((PartnerHelper.IsIndiaThreeDSCommercialPartner(paymentSessionData.Partner)
                    || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ThreeDSOne, session.Country, setting, Constants.DisplayCustomizationDetail.EnableIndia3dsForNonZeroPaymentTransaction))
                    && ((session.ChallengeScenario == ChallengeScenario.PaymentTransaction
                    && session.Amount == 0)
                    || session.ChallengeScenario == ChallengeScenario.RecurringTransaction)
                    && string.Equals(session.Country, "IN", StringComparison.OrdinalIgnoreCase))
                {
                    piRequires3ds1Authentication = false;
                }
            }

            // 3. Create a PayerAuth's PaymentSession object
            var payerAuthPaymentSession = new PayerAuth.PaymentSession(
            accountId: accountId,
            id: null,
            data: paymentSessionData,
            paymentInstrument: paymentInstrument,
            deviceChannel: deviceChannel,
            piRequiresAuthentication: PiRequiresAuthentication(piRequires3ds2Authentication, piRequires3ds1Authentication, paymentInstrument.PaymentMethod, exposedFlightFeatures));

            if (paymentSessionData.RedeemRewards && !string.IsNullOrEmpty(userId))
            {
                payerAuthPaymentSession.UserId = userId;
            }

            // 4. Call CreatePaymentSessionId
            PayerAuth.PaymentSessionResponse resp = null;

            if (await CallSafetyNetOperation(
                async () =>
                {
                    resp = await this.PayerAuthServiceAccessor.CreatePaymentSessionId(
                        paymentSessionData: payerAuthPaymentSession,
                        traceActivityId: traceActivityId);
                },
                traceActivityId,
                exposedFeatures: exposedFlightFeatures,
                excludeErrorFeatureFormat: "PSD2SafetyNet-CreatePS-{0}-{1}"))
            {
                return GetSafetyNetPaymentSession(paymentSessionData);
            }

            payerAuthPaymentSession.Id = resp.PaymentSessionId;
            session.Id = resp.PaymentSessionId;
            this.storedSession = new PXInternal.PaymentSession(
                exposedFlightFeatures: exposedFlightFeatures,
                payerAuthApiVersion: GlobalConstants.PayerAuthApiVersions.V3,
                accountId: accountId,
                payerAuthPaymentSession: payerAuthPaymentSession,
                billableAccountId: paymentSessionData.BillableAccountId,
                classicProduct: paymentSessionData.ClassicProduct,
                emailAddress: emailAddress,
                language: paymentSessionData.Language);

            // 5. If MOTO, call PayerAuth.Authenticate to notify that this is MOTO and return with no challenge
            if (session.IsMOTO || session.RedeemRewards)
            {
                session.IsChallengeRequired = false;
                session.ChallengeStatus = session.RedeemRewards ? PaymentChallengeStatus.NotApplicable : PaymentChallengeStatus.ByPassed;
                session.Signature = session.GenerateSignature();

                PayerAuth.AuthenticationRequest authRequest = new PayerAuth.AuthenticationRequest()
                {
                    PaymentSession = payerAuthPaymentSession,
                };

                await CallSafetyNetOperation(
                    async () =>
                    {
                        await this.PayerAuthServiceAccessor.Authenticate(authRequest, traceActivityId);
                    },
                    traceActivityId,
                    exposedFeatures: exposedFlightFeatures,
                    excludeErrorFeatureFormat: "PSD2SafetyNet-MotoAuthN-{0}-{1}");

                this.storedSession.ChallengeStatus = session.ChallengeStatus;
                await CallSafetyNetOperation(
                    async () =>
                    {
                        await this.SessionServiceAccessor.CreateSessionFromData<PXInternal.PaymentSession>(
                        sessionId: this.storedSession.Id,
                        sessionData: this.storedSession,
                        traceActivityId: traceActivityId);
                    },
                    traceActivityId,
                    exposedFeatures: exposedFlightFeatures,
                    excludeErrorFeatureFormat: "PSD2SafetyNet-CreateSessionFromData-{0}-{1}");

                // TODO: Inspect authResponse and raise Integration exception if EnrollmentStatus is not ByPassed
                await this.PostProcessOnSuccess(
                    traceActivityId: traceActivityId,
                    piQueryParams: GetPiQueryParams(
                        billableAccountId: paymentSessionData.BillableAccountId,
                        classicProduct: paymentSessionData.ClassicProduct,
                        partner: paymentSessionData.Partner));

                return session;
            }

            // 6. Store the PaymentSession object
            if (await CallSafetyNetOperation(
                async () =>
                {
                    await this.SessionServiceAccessor.CreateSessionFromData<PXInternal.PaymentSession>(
                        sessionId: this.storedSession.Id,
                        sessionData: this.storedSession,
                        traceActivityId: traceActivityId);
                },
                traceActivityId))
            {
                return GetSafetyNetPaymentSession(paymentSessionData);
            }

            // 7. Store the PaymentInstrumentSession object
            if (paymentInstrument != null && exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnablePSD2PaymentInstrumentSession, StringComparer.OrdinalIgnoreCase))
            {
                PaymentInstrumentSession piSession = new PaymentInstrumentSession(this.storedSession.Id, accountId, paymentInstrument.PaymentInstrumentDetails?.RequiredChallenge);
                await CallSafetyNetOperation(
                    async () =>
                    {
                        // performing an upsert operation to update the PaymentInstrumentSession object if it already exists
                        await this.SessionServiceAccessor.UpdateSessionResourceData<PaymentInstrumentSession>(
                            sessionId: V7.Constants.Prefixes.PaymentInstrumentSessionPrefix + paymentInstrument.PaymentInstrumentId,
                            newSessionData: piSession,
                            traceActivityId: traceActivityId);
                    },
                    traceActivityId);
            }

            // 8. Update PX's PaymentSession object, PostProcessOnSuccess and return
            if (piRequires3ds2Authentication)
            {
                session.IsChallengeRequired = true;
                session.ChallengeStatus = PaymentChallengeStatus.Unknown;
                if (PartnerHelper.IsValidatePIOnAttachEnabled(paymentSessionData.Partner, exposedFlightFeatures))
                {
                    session.ChallengeType = V7.Constants.ChallengeTypes.PSD2Challenge;
                }
                else if (paymentInstrument.IsLegacyBilldeskPayment())
                {
                    session.ChallengeType = V7.Constants.ChallengeTypes.LegacyBillDeskPaymentChallenge;
                }
            }
            else if (string.Equals(payerAuthPaymentSession.PaymentMethodType, V7.Constants.PaymentMethodType.UPI, StringComparison.OrdinalIgnoreCase)
                && (session.ChallengeScenario == ChallengeScenario.RecurringTransaction || session.ChallengeScenario == ChallengeScenario.PaymentTransaction))
            {
                session.IsChallengeRequired = true;
                session.ChallengeStatus = PaymentChallengeStatus.Unknown;
                session.ChallengeType = V7.Constants.ChallengeTypes.UPIChallenge;
            }
            else if (string.Equals(payerAuthPaymentSession.PaymentMethodType, V7.Constants.PaymentMethodType.UPIQr, StringComparison.OrdinalIgnoreCase)
                && (session.ChallengeScenario == ChallengeScenario.RecurringTransaction || session.ChallengeScenario == ChallengeScenario.PaymentTransaction)
                && exposedFlightFeatures.Contains(Flighting.Features.EnableLtsUpiQRConsumer, StringComparer.OrdinalIgnoreCase))
            {
                session.IsChallengeRequired = true;
                session.ChallengeStatus = PaymentChallengeStatus.Unknown;
                session.ChallengeType = V7.Constants.ChallengeTypes.UPIChallenge;
            }
            else if (((PartnerHelper.IsIndiaThreeDSCommercialPartner(paymentSessionData.Partner)
                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ThreeDSOne, session.Country, setting, Constants.DisplayCustomizationDetail.EnableIndia3dsForNonZeroPaymentTransaction))
                && session.ChallengeScenario == ChallengeScenario.PaymentTransaction
                && session.Amount != 0
                && string.Equals(session.Country, "IN", StringComparison.OrdinalIgnoreCase))
                || piRequires3ds1Authentication)
            {
                session.IsChallengeRequired = true;
                session.ChallengeStatus = PaymentChallengeStatus.Unknown;
                session.ChallengeType = V7.Constants.ChallengeTypes.India3DSChallenge;
            }
            else if (PartnerHelper.IsValidatePIOnAttachEnabled(paymentSessionData.Partner, exposedFlightFeatures)
                     && paymentInstrument.IsCreditCard() && !string.Equals(session.Country, "IN", StringComparison.OrdinalIgnoreCase)
                     && !piIsIssuedIn3ds1RequiredCountry)
            {
                session.IsChallengeRequired = true;
                session.ChallengeStatus = PaymentChallengeStatus.Unknown;
                session.ChallengeType = V7.Constants.ChallengeTypes.ValidatePIOnAttachChallenge;
            }
            else
            {
                session.IsChallengeRequired = false;
                session.ChallengeStatus = PaymentChallengeStatus.NotApplicable;
            }

            if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnableGettingStoredSessionForChallengeDescriptionsController, StringComparer.OrdinalIgnoreCase))
            {
                this.storedSession.ChallengeType = session.ChallengeType;
                this.storedSession.PiRequiresAuthentication = session.IsChallengeRequired;
                if (await this.SafetyNetUpdateSession(traceActivityId))
                {
                    return GetSafetyNetPaymentSession(paymentSessionData);
                }
            }

            // Place holder for generate signature
            session.Signature = session.GenerateSignature();
            return session;
        }

        /// <summary>
        /// Used by browser flow
        /// 1. Gather Information to generate ThreeDSMethod form (fingerprinting)
        /// 2. Store Information to the Session which required in the later ThreeDS call
        /// </summary>
        /// <param name="accountId">The account Id of PI</param>
        /// <param name="browserInfo">The browser info, collected from user request, need store in the session and used in the later call</param>
        /// <param name="paymentSession">The serialized purchase context, need store in the session and used in the later call</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <param name="exposedFlightFeatures">Exposed flights</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns the data BrowserFlowContext for PIDL to consume </returns>
        public virtual async Task<BrowserFlowContext> GetThreeDSMethodURL(
            string accountId,
            PayerAuth.BrowserInfo browserInfo,
            PaymentSession paymentSession,
            EventTraceActivity traceActivityId,
            List<string> exposedFlightFeatures = null)
        {
            PayerAuth.ThreeDSMethodData methodData = null;

            // 1. Get the stored session
            if (await CallSafetyNetOperation(
                operation: async () =>
                {
                    await this.CachedGetStoredSession(
                        sessionId: paymentSession.Id,
                        traceActivityId: traceActivityId);
                },
                traceActivityId))
            {
                return GetSafetyNetBrowserFlowContext(paymentSession);
            }

            if (await CallSafetyNetOperation(
                operation: async () =>
                {
                    // 2. Call PayerAuth.Get3DSMethodUrl
                    methodData = await this.PayerAuthServiceAccessor.Get3DSMethodURL(
                        paymentSession: new PayerAuth.PaymentSession(this.storedSession),
                        traceActivityId: traceActivityId);

                    // 3. Add browserInfo and methodData to the stored session
                    this.storedSession.BrowserInfo = browserInfo;
                    this.storedSession.MethodData = methodData;
                    await this.SafetyNetUpdateSession(traceActivityId);
                },
                traceActivityId,
                exposedFeatures: this.storedSession.ExposedFlightFeatures,
                excludeErrorFeatureFormat: "PSD2SafetyNet-GetMethodUrl-{0}-{1}"))
            {
                this.storedSession.IsSystemError = true;

                var context = GetSafetyNetBrowserFlowContext(paymentSession);
                this.storedSession.ChallengeStatus = context.PaymentSession.ChallengeStatus;

                if (this.storedSession.ChallengeStatus == PaymentChallengeStatus.Succeeded)
                {
                    await this.PostProcessOnSuccess(
                    traceActivityId: traceActivityId,
                    piQueryParams: GetPiQueryParams(
                        billableAccountId: this.storedSession.BillableAccountId,
                        classicProduct: this.storedSession.ClassicProduct,
                        partner: this.storedSession.Partner));
                }
                else
                {
                    await this.SafetyNetUpdateSession(traceActivityId);
                }

                return context;
            }

            // 4. If fingerprinting was skipped by ACS, call PayerAuth.Authenticate
            bool skipFingerprint = exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXPSD2SkipFingerprint, StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(methodData.ThreeDSMethodURL) || skipFingerprint)
            {
                BrowserFlowContext browserFlowContext = null;
                if (await CallSafetyNetOperation(
                        operation: async () =>
                        {
                            browserFlowContext = await this.Authenticate(
                                threeDSMethodCompletionIndicator: (skipFingerprint && !string.IsNullOrEmpty(methodData.ThreeDSMethodURL)) ? PayerAuth.ThreeDSMethodCompletionIndicator.N : PayerAuth.ThreeDSMethodCompletionIndicator.U,
                                exposedFlightFeatures: this.storedSession.ExposedFlightFeatures,
                                traceActivityId: traceActivityId,
                                piQueryParams: GetPiQueryParams(
                                    billableAccountId: this.storedSession.BillableAccountId,
                                    classicProduct: this.storedSession.ClassicProduct,
                                    partner: this.storedSession.Partner));
                        },
                        traceActivityId,
                        exposedFeatures: this.storedSession.ExposedFlightFeatures,
                        excludeErrorFeatureFormat: "PSD2SafetyNet-WebAuthN-{0}-{1}"))
                {
                    this.storedSession.IsSystemError = true;
                    await this.SafetyNetUpdateSession(traceActivityId);
                    browserFlowContext = GetSafetyNetBrowserFlowContext(paymentSession);
                }

                return browserFlowContext;
            }

            PXService.Model.ThreeDSExternalService.ThreeDSMethodData formInputThreeDSMethod = new PXService.Model.ThreeDSExternalService.ThreeDSMethodData()
            {
                ThreeDSServerTransID = methodData.ThreeDSServerTransID,
                ThreeDSMethodNotificationURL = string.Format("{0}/paymentSessions/{1}/authenticate", this.PifdBaseUrl, paymentSession.Id),
            };

            return new BrowserFlowContext
            {
                IsFingerPrintRequired = true,
                FormActionURL = methodData.ThreeDSMethodURL,
                FormInputThreeDSMethodData = ThreeDSUtils.EncodeUrl(ThreeDSUtils.EncodeObjectToBase64(formInputThreeDSMethod))
            };
        }

        public virtual async Task<BrowserFlowContext> AuthenticateUpiPaymentTxn(
            string accountId,
            PayerAuth.BrowserInfo browserInfo,
            PaymentSession paymentSession,
            EventTraceActivity traceActivityId,
            List<string> exposedFlightFeatures = null)
        {
            if (await CallSafetyNetOperation(
                operation: async () =>
                {
                    await this.CachedGetStoredSession(
                        sessionId: paymentSession.Id,
                        traceActivityId: traceActivityId);
                },
                traceActivityId))
            {
                return GetSafetyNetBrowserFlowContext(paymentSession);
            }

            PaymentInstrument paymentInstrument = null;

            try
            {
                paymentInstrument = await this.PimsAccessor.GetPaymentInstrument(
                       accountId: accountId,
                       piid: paymentSession.PaymentInstrumentId,
                       traceActivityId: traceActivityId);
            }
            catch (ServiceErrorResponseException ex)
            {
                if (ex.Error?.ErrorCode == "AccountPINotFound")
                {
                    throw new ValidationException(ErrorCode.PaymentInstrumentNotFound, "Caller is not authorized to access specified PaymentInstrumentId.");
                }

                throw;
            }

            if (paymentInstrument == null || (!paymentInstrument.PaymentMethod.IsUpi() && !paymentInstrument.PaymentMethod.IsUpiQr()))
            {
                throw new ValidationException(ErrorCode.InvalidPaymentInstrumentDetails, "Provided Payment Instrument is not a valid UPI PI.");
            }

            PayerAuth.AuthenticationRequest authRequest = new PayerAuth.AuthenticationRequest
            {
                PaymentSession = this.storedSession,
                BrowserInfo = browserInfo
            };

            PayerAuth.AuthenticationResponse resp = null;
            await CallSafetyNetOperation(
                async () =>
                {
                    resp = await this.PayerAuthServiceAccessor.Authenticate(authRequest, traceActivityId);
                },
                traceActivityId,
                exposedFeatures: null,
                excludeErrorFeatureFormat: "PSD2SafetyNet-MotoAuthN-{0}-{1}");

            paymentSession.ChallengeStatus = resp.EnrollmentStatus == PayerAuth.PaymentInstrumentEnrollmentStatus.Bypassed ? PaymentChallengeStatus.ByPassed : PaymentChallengeStatus.Failed;

            return new BrowserFlowContext()
            {
                PaymentSession = paymentSession,
            };
        }

        /// <summary>
        /// Used by Browser Flow, after users submit ThreeDSMethod (Fingerprinting) form to 3DS Server,
        /// the users will call the API to notify us the ThreeDSMethod is completed
        /// We will contact the underline service to check either user need to be challenged or not
        /// and return the next step to the user.
        /// </summary>
        /// <param name="sessionId">Identity the user's purchase session</param>
        /// <param name="isThreeDSMethodCompleted">boolean value indicating whether 3ds method completed successfully or not</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns the data BrowserFlowContext for PIDL to consume </returns>
        public virtual async Task<BrowserFlowContext> Authenticate(
            string sessionId,
            bool isThreeDSMethodCompleted,
            EventTraceActivity traceActivityId)
        {
            // 1. Get stored session
            if (await CallSafetyNetOperation(
                    operation: async () =>
                    {
                        await this.CachedGetStoredSession(
                            sessionId: sessionId,
                        traceActivityId: traceActivityId);
                    },
                    traceActivityId))
            {
                return GetSafetyNetBrowserFlowContext(
                    GetSafetyNetPaymentSession(sessionId));
            }

            // 2. Call Authenticate
            BrowserFlowContext browserFlowContext = null;
            if (await CallSafetyNetOperation(
                    operation: async () =>
                    {
                        browserFlowContext = await this.Authenticate(
                            threeDSMethodCompletionIndicator: isThreeDSMethodCompleted ? PayerAuth.ThreeDSMethodCompletionIndicator.Y : PayerAuth.ThreeDSMethodCompletionIndicator.N,
                            traceActivityId: traceActivityId,
                            exposedFlightFeatures: this.storedSession.ExposedFlightFeatures,
                            piQueryParams: GetPiQueryParams(
                                billableAccountId: this.storedSession.BillableAccountId,
                                classicProduct: this.storedSession.ClassicProduct,
                                partner: this.storedSession.Partner));
                    },
                    traceActivityId,
                    exposedFeatures: this.storedSession.ExposedFlightFeatures,
                    excludeErrorFeatureFormat: "PSD2SafetyNet-WebAuthN-{0}-{1}"))
            {
                this.storedSession.IsSystemError = true;
                await this.SafetyNetUpdateSession(traceActivityId);

                var ps = new PaymentSession(this.storedSession);
                ps.Signature = ps.GenerateSignature();
                browserFlowContext = GetSafetyNetBrowserFlowContext(ps);
            }

            return browserFlowContext;
        }

        /// <summary>
        /// Used by Browser Flow, after users provide CVV in 3DS 1.0 authentication flow
        /// </summary>
        /// <param name="sessionId">Identity the user's purchase session</param>
        /// <param name="cvvToken">CVV token</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <param name="userId">puid from x-ms-msaprofile header</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns the data BrowserFlowContext for PIDL to consume </returns>
        public virtual async Task<BrowserFlowContext> AuthenticateThreeDSOne(
            string sessionId,
            string cvvToken,
            EventTraceActivity traceActivityId,
            string userId)
        {
            // 1. Get stored session
            if (await CallSafetyNetOperation(
                    operation: async () =>
                    {
                        await this.CachedGetStoredSession(
                            sessionId: sessionId,
                        traceActivityId: traceActivityId);
                    },
                    traceActivityId))
            {
                return GetSafetyNetBrowserFlowContext(
                    GetSafetyNetPaymentSession(sessionId));
            }

            // 2. Call AuthenticateThreeDSOne
            BrowserFlowContext browserFlowContext = null;
            if (await CallSafetyNetOperation(
                    operation: async () =>
                    {
                        browserFlowContext = await this.AuthenticateThreeDSOne(
                            cvvToken: cvvToken,
                            piQueryParams: GetPiQueryParams(
                                billableAccountId: this.storedSession.BillableAccountId,
                                classicProduct: this.storedSession.ClassicProduct,
                                partner: this.storedSession.Partner),
                            traceActivityId: traceActivityId,
                            userId: userId);
                    },
                    traceActivityId,
                    exposedFeatures: this.storedSession.ExposedFlightFeatures))
            {
                var ps = new PaymentSession(this.storedSession);
                ps.Signature = ps.GenerateSignature();
                List<string> exposedFlightFeatures = await this.GetExposedFlightFeatures(ps.Id, traceActivityId);
                browserFlowContext = GetSafetyNetBrowserFlowContext(ps, exposedFlightFeatures);
            }

            return browserFlowContext;
        }

        public virtual async Task<BrowserFlowContext> AuthenticateRedirectionThreeDSOne(
            string sessionId,
            string successUrl,
            string failureUrl,
            EventTraceActivity traceActivityId)
        {
            // 1. Get stored session
            if (await CallSafetyNetOperation(
                    operation: async () =>
                    {
                        await this.CachedGetStoredSession(
                            sessionId: sessionId,
                        traceActivityId: traceActivityId);
                    },
                    traceActivityId))
            {
                return GetSafetyNetBrowserFlowContext(
                    GetSafetyNetPaymentSession(sessionId));
            }

            PaymentSession paymentSession = new PaymentSession(this.storedSession);
            paymentSession.Signature = paymentSession.GenerateSignature();

            // 2. Update stored session with ru and rx
            this.storedSession.SuccessUrl = successUrl;
            this.storedSession.FailureUrl = failureUrl;
            await this.SessionServiceAccessor.UpdateSessionResourceData<PXInternal.PaymentSession>(
                sessionId: this.storedSession.Id,
                newSessionData: this.storedSession,
                traceActivityId: traceActivityId);

            return new BrowserFlowContext()
            {
                IsAcsChallengeRequired = true,
                FormInputCReq = this.storedSession.AuthenticationResponse.AcsSignedContent,
                FormActionURL = this.storedSession.AuthenticationResponse.AcsUrl,
                FormPostAcsURL = this.storedSession.AuthenticationResponse.IsFormPostAcsUrl,
                FormFullPageRedirectAcsURL = this.storedSession.AuthenticationResponse.IsFullPageRedirect,
                CardHolderInfo = this.storedSession.AuthenticationResponse.CardHolderInfo,
                PaymentSession = paymentSession
            };
        }

        /// <summary>
        /// Used by App Flow,
        /// the users will call the API to authenticate
        /// and return the next step to the user.
        /// </summary>
        /// <param name="accountId">Identify the user by account Id</param>
        /// <param name="sessionId">Identify the sessionId</param>
        /// <param name="authRequest">Authenticate request payload sent by user</param>
        /// <param name="exposedFlightFeatures">Exposed flight features</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <param name="testContext">Test Context</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns the data AuthenticationResponse for PIDL to consume </returns>
        public virtual async Task<AuthenticationResponse> Authenticate(
            string accountId,
            string sessionId,
            AuthenticationRequest authRequest,
            List<string> exposedFlightFeatures,
            EventTraceActivity traceActivityId,
            TestContext testContext)
        {
            // 1. Get stored session
            if (await CallSafetyNetOperation(
                    async () =>
                    {
                        await this.CachedGetStoredSession(
                            sessionId: sessionId,
                            traceActivityId: traceActivityId);
                    },
                    traceActivityId))
            {
                await this.TransactionDataServiceAccessor.UpdateCustomerChallengeAttestation(accountId, sessionId, true, traceActivityId);

                return GetSafetyNetAuthenticationResponse();
            }

            if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXAuthenticateChallengeTypeOnStoredSession, StringComparer.OrdinalIgnoreCase)
                && this.storedSession.ChallengeType == V7.Constants.ChallengeTypes.ValidatePIOnAttachChallenge)
            {
                await this.TransactionDataServiceAccessor.UpdateCustomerChallengeAttestation(accountId, sessionId, true, traceActivityId);

                return GetSafetyNetAuthenticationResponse();
            }

            // Set the device channel to App. Webblends calls createPaymentChallenge from the browser
            // and if challenge is needed, store calls handlePaymentChallenge. So, here set the device channel
            // to App.
            this.storedSession.DeviceChannel = DeviceChannel.AppBased;

            // 2. Convert PX's AuthRequest object to PayerAuth's AuthRequest object
            var payerAuthRequest = new PayerAuth.AuthenticationRequest(
                paymentSession: new PayerAuth.PaymentSession(this.storedSession),
                authRequest: authRequest);

            if (string.IsNullOrEmpty(payerAuthRequest.MessageVersion))
            {
                payerAuthRequest.MessageVersion = GlobalConstants.PSD2Constants.FallbackMessageVersion;
            }

            // 3. Call PayerAuth.Authenticate
            PayerAuth.AuthenticationResponse payerAuthResponse = null;
            if (await CallSafetyNetOperation(
                    async () =>
                    {
                        payerAuthResponse = await this.PayerAuthServiceAccessor.Authenticate(
                            authRequest: payerAuthRequest,
                            traceActivityId: traceActivityId);

                        this.storedSession.TransactionStatus = payerAuthResponse.TransactionStatus;
                        this.storedSession.TransactionStatusReason = payerAuthResponse.TransactionStatusReason;
                        await this.SafetyNetUpdateSession(traceActivityId);
                    },
                    traceActivityId,
                    exposedFeatures: this.storedSession.ExposedFlightFeatures,
                    excludeErrorFeatureFormat: "PSD2SafetyNet-AppAuthN-{0}-{1}"))
            {
                // TODO : Remove after Fraud Investigation
                SllWebLogger.TraceServerMessage("V1 Authenticate App", null, sessionId, payerAuthResponse?.TransactionStatus != null ? "TransStatus: " + payerAuthResponse.TransactionStatus.ToString() : "No transStatus available", EventLevel.Informational);

                await this.TransactionDataServiceAccessor.UpdateCustomerChallengeAttestation(accountId, sessionId, true, traceActivityId);

                this.storedSession.IsSystemError = true;
                this.storedSession.ChallengeStatus = PaymentChallengeStatus.Succeeded;

                await this.PostProcessOnSuccess(
                    traceActivityId: traceActivityId,
                    piQueryParams: GetPiQueryParams(
                        billableAccountId: this.storedSession.BillableAccountId,
                        classicProduct: this.storedSession.ClassicProduct,
                        partner: this.storedSession.Partner));

                return GetSafetyNetAuthenticationResponse();
            }

            var mappedStatus = GetMappedStatusForAuthentication(
                transStatus: payerAuthResponse.TransactionStatus,
                isMoto: this.storedSession.IsMOTO,
                additionalInputs: new List<string>() { payerAuthResponse.TransactionStatusReason.ToString() },
                exposedFlights: this.storedSession.ExposedFlightFeatures);

            // TODO : Remove after Fraud Investigation
            SllWebLogger.TraceServerMessage("V1 Authenticate App", null, sessionId, "Mapped Status: " + mappedStatus.ToString(), EventLevel.Informational);

            // Localized text to display in PSD2 app native challenges
            var localizations = GetPSD2NativeChallengeLocalizations(authRequest.Language);

            if (mappedStatus != PaymentChallengeStatus.Unknown)
            {
                this.storedSession.ChallengeStatus = mappedStatus;

                if (IsAuthenticationVerified(mappedStatus))
                {
                    await this.PostProcessOnSuccess(
                        traceActivityId: traceActivityId,
                        piQueryParams: GetPiQueryParams(
                            billableAccountId: this.storedSession.BillableAccountId,
                            classicProduct: this.storedSession.ClassicProduct,
                            partner: this.storedSession.Partner));
                }
                else
                {
                    await this.SafetyNetUpdateSession(traceActivityId);
                }

                var authenticationVerified = IsAuthenticationVerified(mappedStatus);
                await this.TransactionDataServiceAccessor.UpdateCustomerChallengeAttestation(accountId, sessionId, authenticationVerified, traceActivityId);

                return new AuthenticationResponse()
                {
                    AcsTransactionID = payerAuthResponse.AcsTransactionId,
                    AcsSignedContent = payerAuthResponse.AcsSignedContent,
                    ThreeDSServerTransactionID = payerAuthResponse.ThreeDSServerTransactionId,
                    EnrollmentStatus = payerAuthResponse.EnrollmentStatus,
                    ChallengeStatus = mappedStatus,
                    AcsRenderingType = new AcsRenderingType(payerAuthResponse.AcsRenderingType),
                    CardHolderInfo = payerAuthResponse.CardHolderInfo,
                    AuthenticationType = payerAuthResponse.AuthenticationType,
                    AcsChallengeMandated = payerAuthResponse.AcsChallengeMandated,
                    AcsReferenceNumber = payerAuthResponse.AcsReferenceNumber,
                    DsReferenceNumber = payerAuthResponse.DsReferenceNumber,
                    AcsOperatorID = payerAuthResponse.AcsOperatorID,
                    MessageVersion = payerAuthResponse.MessageVersion,
                    DisplayStrings = localizations
                };
            }
            else
            {
                if (exposedFlightFeatures.Contains(Flighting.Features.PXEnablePSD2ServiceSideCertificateValidation))
                {
                    // ACSSignedContent will be assigned to the AuthenticationResponse
                    // only if the PaymentChallengeStatus is Unknown,
                    // therefore further checks for validity are required.
                    mappedStatus = this.ValidateACSSignedContent(authRequest, mappedStatus, exposedFlightFeatures, payerAuthResponse, testContext, traceActivityId, sessionId);
                }
            }

            // 5. Update stored session with AuthResponse. This will be used when calling CompleteChallenge
            this.storedSession.AuthenticationResponse = payerAuthResponse;
            this.storedSession.ChallengeStatus = mappedStatus;

            if (await this.SafetyNetUpdateSession(traceActivityId))
            {
                await this.TransactionDataServiceAccessor.UpdateCustomerChallengeAttestation(accountId, sessionId, true, traceActivityId);
                return GetSafetyNetAuthenticationResponse();
            }

            // 6. Covert PayerAuth's AuthResponse to PX's AuthResponse and return
            return new AuthenticationResponse()
            {
                AcsTransactionID = payerAuthResponse.AcsTransactionId,
                AcsSignedContent = payerAuthResponse.AcsSignedContent,
                ThreeDSServerTransactionID = payerAuthResponse.ThreeDSServerTransactionId,
                EnrollmentStatus = payerAuthResponse.EnrollmentStatus,
                ChallengeStatus = mappedStatus,
                AcsRenderingType = new AcsRenderingType(payerAuthResponse.AcsRenderingType),
                CardHolderInfo = payerAuthResponse.CardHolderInfo,
                AuthenticationType = payerAuthResponse.AuthenticationType,
                AcsChallengeMandated = payerAuthResponse.AcsChallengeMandated,
                AcsReferenceNumber = payerAuthResponse.AcsReferenceNumber,
                DsReferenceNumber = payerAuthResponse.DsReferenceNumber,
                AcsOperatorID = payerAuthResponse.AcsOperatorID,
                MessageVersion = payerAuthResponse.MessageVersion ?? GlobalConstants.PSD2Constants.FallbackMessageVersion,
                DisplayStrings = localizations
            };
        }

        /// <summary>
        /// Used by Browser and App Flow, after users submit ThreeDSChallenge to ACS,
        /// the users will call the API to notify us the ThreeDSChallenge is completed
        /// We will contact the underline service to fetch result
        /// </summary>
        /// <param name="accountId">Identity the user by accountId</param>
        /// <param name="sessionId">Identity the user's purchase session</param>
        /// <param name="exposedFlightFeatures">Exposed flight features</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <returns>Returns the data PaymentSession for PIDL to consume </returns>
        public virtual async Task<PaymentSession> CompleteThreeDSChallenge(
            string accountId,
            string sessionId,
            List<string> exposedFlightFeatures,
            EventTraceActivity traceActivityId)
        {
            // 1. Get stored session
            if (await CallSafetyNetOperation(
                    async () =>
                    {
                        await this.CachedGetStoredSession(
                            sessionId: sessionId,
                            traceActivityId: traceActivityId);
                    },
                    traceActivityId))
            {
                await this.TransactionDataServiceAccessor.UpdateCustomerChallengeAttestation(accountId, sessionId, true, traceActivityId);

                return GetSafetyNetPaymentSession(sessionId);
            }

            // 2. Create a CompletionRequest object
            var completionRequest = new PayerAuth.CompletionRequest()
            {
                PaymentSession = new PayerAuth.PaymentSession(this.storedSession),
                AuthenticationContext = this.storedSession.AuthenticationResponse
            };

            // 3. Call PayerAuth.CompleteChallenge
            PayerAuth.CompletionResponse completionResponse = null;
            if (await CallSafetyNetOperation(
                    async () =>
                    {
                        completionResponse = await this.PayerAuthServiceAccessor.CompleteChallenge(
                           completionRequest: completionRequest,
                           traceActivityId: traceActivityId);

                        this.storedSession.TransactionStatus = completionResponse.TransactionStatus;
                        this.storedSession.TransactionStatusReason = completionResponse.TransactionStatusReason;
                        this.storedSession.ChallengeCancelIndicator = completionResponse.ChallengeCancelIndicator;
                        await this.SafetyNetUpdateSession(traceActivityId);
                    },
                    traceActivityId,
                    exposedFeatures: this.storedSession.ExposedFlightFeatures,
                    excludeErrorFeatureFormat: "PSD2SafetyNet-Completion-{0}-{1}"))
            {
                this.storedSession.IsSystemError = true;
                completionResponse = GetSafetyNetCompletionResponse();
            }

            // 4. Convert PayerAuth's PaymentSession object to a PX PaymentSession
            var paymentSession = new PXService.V7.PaymentChallenge.Model.PaymentSession(this.storedSession);
            paymentSession.Signature = paymentSession.GenerateSignature();

            // 5. Update PaymentSession object and return
            var mappedStatusString = GetMappedStatusString(
                mapPrefix: "PXPSD2Comp",
                transStatus: completionResponse.TransactionStatus,
                isMoto: this.storedSession.IsMOTO,
                additionalInputs: new List<string>() { completionResponse.TransactionStatusReason.ToString(), completionResponse.ChallengeCancelIndicator },
                exposedFlights: this.storedSession.ExposedFlightFeatures);

            var mappedStatus = PaymentChallengeStatus.Succeeded;
            if (mappedStatusString != null && Enum.TryParse<PaymentChallengeStatus>(mappedStatusString, true, out mappedStatus))
            {
                paymentSession.ChallengeStatus = mappedStatus;
                this.storedSession.ChallengeStatus = mappedStatus;

                if (IsAuthenticationVerified(mappedStatus))
                {
                    await this.PostProcessOnSuccess(
                        traceActivityId: traceActivityId,
                        piQueryParams: GetPiQueryParams(
                            billableAccountId: this.storedSession.BillableAccountId,
                            classicProduct: this.storedSession.ClassicProduct,
                            partner: this.storedSession.Partner));
                }
                else
                {
                    await this.SafetyNetUpdateSession(traceActivityId);
                }
            }
            else
            {
                switch (completionResponse.TransactionStatus)
                {
                    case PayerAuth.TransactionStatus.FR:
                    case PayerAuth.TransactionStatus.R:
                        paymentSession.ChallengeStatus = PaymentChallengeStatus.Failed;
                        break;

                    case PayerAuth.TransactionStatus.N:
                        paymentSession.ChallengeStatus = PaymentChallengeStatus.Succeeded;
                        if (!string.IsNullOrWhiteSpace(completionResponse.ChallengeCancelIndicator))
                        {
                            PayerAuth.ChallengeCancelIndicator cancelIndicator;
                            if (Enum.TryParse(completionResponse.ChallengeCancelIndicator, out cancelIndicator))
                            {
                                switch (cancelIndicator)
                                {
                                    case PayerAuth.ChallengeCancelIndicator.TransactionCReqTimedOut:
                                    case PayerAuth.ChallengeCancelIndicator.TransactionTimedOut:
                                        paymentSession.ChallengeStatus = PaymentChallengeStatus.TimedOut;
                                        break;

                                    case PayerAuth.ChallengeCancelIndicator.CancelledByCardHolder:
                                    case PayerAuth.ChallengeCancelIndicator.CancelledByRequestor:
                                    case PayerAuth.ChallengeCancelIndicator.TransactionAbandoned:
                                        paymentSession.ChallengeStatus = PaymentChallengeStatus.Cancelled;
                                        break;
                                }
                            }
                        }
                        else if (completionResponse.TransactionStatusReason == PayerAuth.TransactionStatusReason.TSR14)
                        {
                            paymentSession.ChallengeStatus = PaymentChallengeStatus.TimedOut;
                        }

                        break;
                    default:
                        // Authentication was Successful
                        await this.PostProcessOnSuccess(
                            traceActivityId: traceActivityId,
                            piQueryParams: GetPiQueryParams(
                                billableAccountId: this.storedSession.BillableAccountId,
                                classicProduct: this.storedSession.ClassicProduct,
                                partner: this.storedSession.Partner));

                        paymentSession.ChallengeStatus = PaymentChallengeStatus.Succeeded;
                        break;
                }

                this.storedSession.ChallengeStatus = paymentSession.ChallengeStatus;
                await this.SafetyNetUpdateSession(traceActivityId);
            }

            var authenticationVerified = IsAuthenticationVerified(paymentSession.ChallengeStatus);

            // BrowserNotifyThreeDSChallengeCompleted passes a null accountId.  Retrieve value from storedSession otherwise attestation will not execute.
            if (string.IsNullOrEmpty(accountId) && this.storedSession != null)
            {
                if (!string.IsNullOrWhiteSpace(this.storedSession.AccountId))
                {
                    accountId = this.storedSession.AccountId;
                }
                else if (!string.IsNullOrWhiteSpace(this.storedSession.PaymentInstrumentAccountId))
                {
                    accountId = this.storedSession.PaymentInstrumentAccountId;
                }
                else if (!string.IsNullOrWhiteSpace(this.storedSession.BillableAccountId))
                {
                    accountId = this.storedSession.BillableAccountId;
                }
                else if (!string.IsNullOrWhiteSpace(this.storedSession.CommercialAccountId))
                {
                    accountId = this.storedSession.CommercialAccountId;
                }
            }

            await this.TransactionDataServiceAccessor.UpdateCustomerChallengeAttestation(accountId, sessionId, authenticationVerified, traceActivityId);

            return paymentSession;
        }

        /// <summary>
        /// Used by Browser and App Flow, after users submit ThreeDSChallenge to ACS,
        /// the users will call the API to notify us the ThreeDSChallenge is completed
        /// We will contact the underline service to fetch result
        /// </summary>
        /// <param name="accountId">Identity the user by accountId</param>
        /// <param name="sessionId">Identity the user's purchase session</param>
        /// <param name="exposedFlightFeatures">Exposed flight features</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <param name="authParameters">ACS parameters</param>
        /// <returns>Returns the data PaymentSession for PIDL to consume </returns>
        public virtual async Task<PaymentSession> CompleteThreeDSOneChallenge(
            string accountId,
            string sessionId,
            List<string> exposedFlightFeatures,
            EventTraceActivity traceActivityId,
            Dictionary<string, string> authParameters = null)
        {
            // 1. Get stored session
            if (await CallSafetyNetOperation(
                    async () =>
                    {
                        await this.CachedGetStoredSession(
                            sessionId: sessionId,
                            traceActivityId: traceActivityId);
                    },
                    traceActivityId))
            {
                await this.TransactionDataServiceAccessor.UpdateCustomerChallengeAttestation(accountId, sessionId, true, traceActivityId);

                return GetSafetyNetPaymentSession(sessionId);
            }

            // 2. Create a CompletionRequest object
            var completionRequest = new PayerAuth.CompletionRequest()
            {
                PaymentSession = new PayerAuth.PaymentSession(this.storedSession),
                AuthenticationContext = this.storedSession.AuthenticationResponse,
            };

            // We need to send PaRes as part of step 2
            if (authParameters != null)
            {
                foreach (var key in authParameters.Keys)
                {
                    completionRequest.AuthorizationParameters.Add(key, authParameters[key]);
                }
            }

            // 3. Call PayerAuth.CompleteChallenge
            bool isSafetyNetCalled = false;
            PayerAuth.CompletionResponse completionResponse = null;
            if (await CallSafetyNetOperation(
                    async () =>
                    {
                        completionResponse = await this.PayerAuthServiceAccessor.CompleteChallenge(
                           completionRequest: completionRequest,
                           traceActivityId: traceActivityId);
                    },
                    traceActivityId,
                    exposedFeatures: this.storedSession.ExposedFlightFeatures))
            {
                completionResponse = GetSafetyNetThreeDSOneCompletionResponse();
                isSafetyNetCalled = true;
            }

            // 4. Convert PayerAuth's PaymentSession object to a PX PaymentSession
            var paymentSession = new PXService.V7.PaymentChallenge.Model.PaymentSession(this.storedSession);
            paymentSession.Signature = paymentSession.GenerateSignature();

            // 5. Update PaymentSession object and return
            switch (completionResponse.TransactionStatus)
            {
                case PayerAuth.TransactionStatus.U:
                    paymentSession.ChallengeStatus = PaymentChallengeStatus.Failed;
                    break;

                case PayerAuth.TransactionStatus.R:
                    paymentSession.ChallengeStatus = isSafetyNetCalled ? PaymentChallengeStatus.InternalServerError : PaymentChallengeStatus.Failed;
                    break;

                case PayerAuth.TransactionStatus.N:
                    paymentSession.ChallengeStatus = PaymentChallengeStatus.Succeeded;
                    if (!string.IsNullOrWhiteSpace(completionResponse.ChallengeCancelIndicator))
                    {
                        PayerAuth.ChallengeCancelIndicator cancelIndicator;
                        if (Enum.TryParse(completionResponse.ChallengeCancelIndicator, out cancelIndicator))
                        {
                            switch (cancelIndicator)
                            {
                                case PayerAuth.ChallengeCancelIndicator.TransactionCReqTimedOut:
                                case PayerAuth.ChallengeCancelIndicator.TransactionTimedOut:
                                    paymentSession.ChallengeStatus = PaymentChallengeStatus.TimedOut;
                                    break;

                                case PayerAuth.ChallengeCancelIndicator.CancelledByCardHolder:
                                case PayerAuth.ChallengeCancelIndicator.CancelledByRequestor:
                                case PayerAuth.ChallengeCancelIndicator.TransactionAbandoned:
                                    paymentSession.ChallengeStatus = PaymentChallengeStatus.Cancelled;
                                    break;
                            }
                        }
                    }
                    else if (completionResponse.TransactionStatusReason == PayerAuth.TransactionStatusReason.TSR14)
                    {
                        paymentSession.ChallengeStatus = PaymentChallengeStatus.TimedOut;
                    }

                    break;
                default:
                    // Authentication was Successful
                    await this.PostProcessOnSuccess(
                        traceActivityId: traceActivityId,
                        piQueryParams: GetPiQueryParams(
                            billableAccountId: this.storedSession.BillableAccountId,
                            classicProduct: this.storedSession.ClassicProduct,
                            partner: this.storedSession.Partner));

                    paymentSession.ChallengeStatus = PaymentChallengeStatus.Succeeded;
                    break;
            }

            this.storedSession.ChallengeStatus = paymentSession.ChallengeStatus;

            await this.SessionServiceAccessor.UpdateSessionResourceData<PXInternal.PaymentSession>(sessionId, this.storedSession, traceActivityId);

            var authenticationVerified = IsAuthenticationVerified(paymentSession.ChallengeStatus);
            await this.TransactionDataServiceAccessor.UpdateCustomerChallengeAttestation(accountId, sessionId, authenticationVerified, traceActivityId);

            return paymentSession;
        }

        /// <summary>
        /// Used by Commercial Portals for 3DS (1.0) in India market
        /// </summary>
        /// <param name="accountId">Identify the user by account Id</param>
        /// <param name="sessionId">Identify the sessionId</param>
        /// <param name="cvvToken">Tokenized CVV</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <param name="setting">Setting data from Partner Settings Service</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns the RDS URL </returns>
        public virtual async Task<TransactionResource> AuthenticateIndiaThreeDS(
            string accountId,
            string sessionId,
            string cvvToken,
            EventTraceActivity traceActivityId,
            PaymentExperienceSetting setting = null)
        {
            await CallSafetyNetOperation(
                   operation: async () =>
                   {
                       await this.CachedGetStoredSession(
                           sessionId: sessionId,
                           traceActivityId: traceActivityId);
                   },
                   traceActivityId);

            PaymentResource paymentResource = await this.TransactionServiceAccessor.CreatePaymentObject(accountId, traceActivityId);
            string paymentId = paymentResource.Id;

            string threeDSSessionId = Guid.NewGuid().ToString();

            ValidationParameters validationParameters = new ValidationParameters
            {
                Amount = this.storedSession.Amount,
                Country = this.storedSession.Country.ToUpperInvariant(),
                Currency = this.storedSession.Currency.ToUpperInvariant(),
                PaymentInstrument = this.storedSession.PaymentInstrumentId,
                Store = GetTransactionServiceStore(this.storedSession.Partner, setting),
                CommercialTransaction = true,
                SessionId = threeDSSessionId,
                AuthenticationData = new AuthenticationData
                {
                    ChallengeType = ChallengeType.Cvv,
                    Data = cvvToken
                },
                UpdateValidation = false
            };

            TransactionResource transactionResource = await this.TransactionServiceAccessor.ValidateCvv(accountId, paymentId, validationParameters, traceActivityId);

            if (!string.IsNullOrEmpty(transactionResource.RedirectUrl))
            {
                await this.PostProcessOnSuccessIndiaThreeDS(
                    traceActivityId: traceActivityId,
                    sessionId: threeDSSessionId,
                    piQueryParams: GetPiQueryParams(
                        billableAccountId: this.storedSession.BillableAccountId,
                        classicProduct: this.storedSession.ClassicProduct,
                        partner: this.storedSession.Partner));
            }

            return transactionResource;
        }

        public virtual async Task<PaymentSession> TryGetPaymentSession(string sessionId, EventTraceActivity traceActivityId)
        {
            await CallSafetyNetOperation(
                   operation: async () =>
                   {
                       await this.CachedGetStoredSession(
                           sessionId: sessionId,
                           traceActivityId: traceActivityId);
                   },
                   traceActivityId);

            PaymentSession paymentSession = null;
            if (this.storedSession != null)
            {
                paymentSession = new PaymentSession(this.storedSession);
                paymentSession.Language = this.storedSession.Language;
                paymentSession.BillableAccountId = this.storedSession.BillableAccountId;
                paymentSession.ClassicProduct = this.storedSession.ClassicProduct;
                paymentSession.Signature = paymentSession.GenerateSignature();

                paymentSession.ChallengeStatus = this.storedSession.ChallengeStatus;
                paymentSession.PaymentInstrumentId = this.storedSession.PaymentInstrumentId;
                paymentSession.UserDisplayMessage = this.storedSession.AuthenticationResponse?.CardHolderInfo;
            }

            return paymentSession;
        }

        public virtual async Task<List<string>> GetExposedFlightFeatures(string sessionId, EventTraceActivity traceActivityId)
        {
            PXInternal.PaymentSession paymentSession = await this.SessionServiceAccessor.GetSessionResourceData<PXInternal.PaymentSession>(
                    sessionId: sessionId,
                    traceActivityId: traceActivityId);
            return paymentSession.ExposedFlightFeatures;
        }

        public virtual async Task<BrowserFlowContext> GetThreeDSMethodData(string sessionId, EventTraceActivity traceActivityId)
        {
            PXInternal.PaymentSession paymentSession = await this.SessionServiceAccessor.GetSessionResourceData<PXInternal.PaymentSession>(
                    sessionId: sessionId,
                    traceActivityId: traceActivityId);

            PXService.Model.ThreeDSExternalService.ThreeDSMethodData formInputThreeDSMethod = new PXService.Model.ThreeDSExternalService.ThreeDSMethodData()
            {
                ThreeDSServerTransID = paymentSession.MethodData.ThreeDSServerTransID,
                ThreeDSMethodNotificationURL = string.Format("{0}/paymentSessions/{1}/authenticate", this.PifdBaseUrl, paymentSession.Id),
            };

            return new BrowserFlowContext
            {
                IsFingerPrintRequired = true,
                FormActionURL = paymentSession.MethodData.ThreeDSMethodURL,
                FormInputThreeDSMethodData = ThreeDSUtils.EncodeUrl(ThreeDSUtils.EncodeObjectToBase64(formInputThreeDSMethod))
            };
        }

        public virtual async Task<BrowserFlowContext> GetThreeDSAuthenticationData(string sessionId, EventTraceActivity traceActivityId)
        {
            PXInternal.PaymentSession paymentSession = await this.SessionServiceAccessor.GetSessionResourceData<PXInternal.PaymentSession>(
                    sessionId: sessionId,
                    traceActivityId: traceActivityId);

            PayerAuth.AuthenticationResponse authResponse = paymentSession.AuthenticationResponse;
            ThreeDSSessionData threeDSSessionData = new ThreeDSSessionData
            {
                ThreeDSServerTransID = authResponse.ThreeDSServerTransactionId,
                AcsTransID = paymentSession.AuthenticationResponse.AcsTransactionId,
            };

            // 5. Calculate the Form Data to Return for Challenge Scenario.
            ChallengeRequest creq = new ChallengeRequest
            {
                ThreeDSServerTransID = authResponse.ThreeDSServerTransactionId,
                AcsTransID = authResponse.AcsTransactionId,
                MessageType = "CReq",
                MessageVersion = authResponse.MessageVersion ?? GlobalConstants.PSD2Constants.FallbackMessageVersion,
                ChallengeWindowSize = paymentSession.BrowserInfo.ChallengeWindowSize
            };

            return new BrowserFlowContext()
            {
                IsFingerPrintRequired = false,
                IsAcsChallengeRequired = true,
                FormInputThreeDSSessionData = ThreeDSUtils.EncodeUrl(ThreeDSUtils.EncodeObjectToBase64(threeDSSessionData)),
                FormInputCReq = ThreeDSUtils.EncodeUrl(ThreeDSUtils.EncodeObjectToBase64(creq)),
                FormActionURL = authResponse.AcsUrl,
                CardHolderInfo = authResponse.CardHolderInfo,
                TransactionSessionId = authResponse.TransactionSessionId
            };
        }

        public virtual async Task<bool> CheckOwnership(string accountId, string paymentInstrumentId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures)
        {
            // Call GetPI to verify the account owns paymentInstrumentId
            try
            {
                PaymentInstrument piDetails = await this.PimsAccessor.GetPaymentInstrument(
                    accountId: accountId,
                    piid: paymentInstrumentId,
                    traceActivityId: traceActivityId);
                return true;
            }
            catch (ServiceErrorResponseException ex)
            {
                if (ex.Error?.ErrorCode == "AccountPINotFound")
                {
                    return false;
                }

                throw;
            }
        }

        public virtual async Task<BrowserFlowContext> HandlePaymentChallenge(
            string accountId,
            PayerAuth.BrowserInfo browserInfo,
            PaymentSession paymentSession,
            EventTraceActivity traceActivityId,
            List<string> exposedFlightFeatures,
            string email = null,
            string puid = null)
        {
            if (string.Equals(paymentSession.ChallengeType, V7.Constants.ChallengeTypes.ValidatePIOnAttachChallenge, StringComparison.OrdinalIgnoreCase))
            {
                var piQueryParams = GetPiQueryParams(
                        billableAccountId: paymentSession.BillableAccountId,
                        classicProduct: paymentSession.ClassicProduct,
                        partner: paymentSession.Partner);

                await CallSafetyNetOperation(
                   operation: async () =>
                   {
                       await this.CachedGetStoredSession(
                           sessionId: paymentSession.Id,
                           traceActivityId: traceActivityId);
                   },
                   traceActivityId);

                bool successValidatePI = await this.ValidatePI(traceActivityId, piQueryParams);
                if (successValidatePI)
                {
                    paymentSession.ChallengeStatus = PaymentChallengeStatus.Succeeded;
                }
                else
                {
                    paymentSession.ChallengeStatus = PaymentChallengeStatus.Failed;
                }

                return new BrowserFlowContext()
                {
                    IsFingerPrintRequired = false,
                    IsAcsChallengeRequired = false,
                    PaymentSession = paymentSession
                };
            }
            else
            {
                // else return what current 3ds challenge does. Yet under flighting this code will never get called.
                return await this.GetThreeDSMethodURL(
                accountId: accountId,
                browserInfo: browserInfo,
                paymentSession: paymentSession,
                traceActivityId: traceActivityId,
                exposedFlightFeatures: exposedFlightFeatures);
            }
        }

        /// <summary>
        /// Add browserInfo to stored session
        /// </summary>
        /// <param name="browserInfo">The browser info, collected from user request, need store in the session and used in the later call</param>
        /// <param name="paymentSession">The serialized purchase context, need store in the session and used in the later call</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns the data BrowserFlowContext for PIDL to consume </returns>
        public virtual async Task UpdateSessionResourceData(
            PayerAuth.BrowserInfo browserInfo,
            PaymentSession paymentSession,
            EventTraceActivity traceActivityId)
        {
            // 1. Get the stored session
            if (await CallSafetyNetOperation(
                operation: async () =>
                {
                    await this.CachedGetStoredSession(
                        sessionId: paymentSession.Id,
                        traceActivityId: traceActivityId);
                },
                traceActivityId))
            {
                return;
            }

            if (await CallSafetyNetOperation(
                    operation: async () =>
                    {
                        // 2. Add browserInfo to the stored session
                        this.storedSession.BrowserInfo = browserInfo;
                        await this.SessionServiceAccessor.UpdateSessionResourceData<PXInternal.PaymentSession>(
                            sessionId: paymentSession.Id,
                            newSessionData: this.storedSession,
                            traceActivityId: traceActivityId);
                    },
                    traceActivityId,
                    exposedFeatures: this.storedSession.ExposedFlightFeatures))
            {
                return;
            }
        }

        public virtual async Task LinkSession(string paymentSessionId, string linkSessionId, EventTraceActivity traceActivityId)
        {
            // 1. Get the stored session
            if (await CallSafetyNetOperation(
                operation: async () =>
                {
                    await this.CachedGetStoredSession(
                        sessionId: paymentSessionId,
                        traceActivityId: traceActivityId);
                },
                traceActivityId))
            {
                return;
            }

            // 2. Save session against PI
            await this.PimsAccessor.LinkSession(
                accountId: this.storedSession.PaymentInstrumentAccountId,
                piid: this.storedSession.PaymentInstrumentId,
                payload: new LinkSession(linkSessionId),
                traceActivityId: traceActivityId,
                queryParams: GetPiQueryParams(
                    billableAccountId: this.storedSession.BillableAccountId,
                    classicProduct: this.storedSession.ClassicProduct,
                    partner: this.storedSession.Partner));
        }

        public virtual async Task<PXInternal.PaymentSession> GetStoredSession(string sessionId, EventTraceActivity traceActivityId)
        {
            PXInternal.PaymentSession storedSession = await this.SessionServiceAccessor.GetSessionResourceData<PXInternal.PaymentSession>(
                sessionId: sessionId,
                traceActivityId: traceActivityId);
            return storedSession;
        }

        public virtual async Task<PXInternal.PaymentSession> TryGetStoredSession(string sessionId, EventTraceActivity traceActivityId)
        {
            await CallSafetyNetOperation(
                operation: async () =>
                {
                    await this.CachedGetStoredSession(sessionId: sessionId, traceActivityId: traceActivityId);
                },
                traceActivityId);

            return this.storedSession;
        }

        protected static bool IsAuthenticationVerified(PaymentChallengeStatus status)
        {
            return status == PaymentChallengeStatus.Succeeded ||
                    status == PaymentChallengeStatus.ByPassed ||
                    status == PaymentChallengeStatus.NotApplicable;
        }

        private static bool PaymentMethodRequiresPaymentChallenge(PaymentInstrument piDetails, List<string> exposedFlightFeatures)
        {
            if (piDetails.IsUpiQr() && exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.EnableLtsUpiQRConsumer, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }

            return !piDetails.IsCreditCard() && !piDetails.IsLegacyBilldeskPayment() && !PIHelper.IsUpi(piDetails.PaymentMethod.PaymentMethodFamily, piDetails.PaymentMethod.PaymentMethodType);
        }

        private static bool IsPSD2EnabledInPartnerSetting(PaymentExperienceSetting setting)
        {
            if (setting?.Features == null)
            {
                return false;
            }

            PartnerSettingsModel.FeatureConfig featureConfig;
            return setting.Features.TryGetValue(Constants.FeatureNames.PSD2, out featureConfig);
        }

        // VerifyAuthorization functions is called at the time of session creation. For all subsequent calls,
        // the session ids act as secret/credential and there is no authorization needed. If the session id
        // is invalid, the retrieval from session service will fail.
        private static void VerifyMOTOAuthorization(
            PaymentSessionData sessionData,
            string isMotoAuthorized)
        {
            if (sessionData.IsMOTO && !string.Equals("true", isMotoAuthorized, StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException(ErrorCode.UnauthorizedMotoPaymentSession, "Unauthorized Moto payment session");
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> GetPiQueryParams(
            string billableAccountId,
            string classicProduct,
            string partner)
        {
            var retVal = new System.Collections.Generic.List<KeyValuePair<string, string>>();

            if (!string.IsNullOrEmpty(billableAccountId))
            {
                retVal.Add(new KeyValuePair<string, string>(GlobalConstants.QueryParamNames.BillableAccountId, billableAccountId));
            }

            if (!string.IsNullOrEmpty(classicProduct))
            {
                retVal.Add(new KeyValuePair<string, string>(GlobalConstants.QueryParamNames.ClassicProduct, classicProduct));
            }

            if (!string.IsNullOrEmpty(partner))
            {
                retVal.Add(new KeyValuePair<string, string>(GlobalConstants.QueryParamNames.Partner, partner));
            }

            return retVal;
        }

        private static string GetLatestAvailablePSD2SettingVersion(List<string> exposedFlightFeatures)
        {
            var regex = new Regex(PSD2SettingVersionFlightRegex, RegexOptions.IgnoreCase);
            IEnumerable<int> availableVersions = exposedFlightFeatures.Where(x => regex.IsMatch(x))
                .Select(x => int.Parse(regex.Match(x).Groups["versionNumber"].Value));
            return availableVersions.Any() ? $"V{availableVersions.Max()}" : DefaultPSD2SettingsVersion;
        }

        /// <summary>
        /// Calls a specified operation within a "safety net". That is certain types of exceptions are caught
        /// and logged.
        /// </summary>
        /// <param name="operation">The operation that could throw an exception</param>
        /// <param name="traceActivity">The event trace activity</param>
        /// <param name="exposedFeatures">Features exposes for the current request (per flight configuration on carbon)</param>
        /// <param name="excludeErrorFeatureFormat">Format of a carbon feature name that that has two place holders. 1. Http Status code 2. ErrorCode.</param>
        /// <returns>
        /// True if an exception was caught. This can be used by the caller to take remedial actions.
        /// </returns>
        private static async Task<bool> CallSafetyNetOperation(
            Func<Task> operation,
            EventTraceActivity traceActivity,
            List<string> exposedFeatures = null,
            string excludeErrorFeatureFormat = null)
        {
            try
            {
                await operation();
            }
            catch (ServiceErrorResponseException ex)
            {
                if (exposedFeatures == null || excludeErrorFeatureFormat == null || !exposedFeatures.Contains(
                        string.Format(excludeErrorFeatureFormat, ex.Response?.StatusCode.ToString(), ex.Error?.ErrorCode)))
                {
                    SllWebLogger.TracePXServiceException($"SafetyNet caught ServiceErrorResponseException: {ex}", traceActivity);
                    return true;
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException($"SafetyNet caught nonServiceErrorResponseException : {ex}", traceActivity);
                return true;
            }

            return false;
        }

        private static BrowserFlowContext GetSafetyNetBrowserFlowContext(PaymentSession ps, List<string> exposedFlightFeatures = null)
        {
            ps.ChallengeStatus = PaymentChallengeStatus.Succeeded;

            if ((!string.IsNullOrEmpty(ps.Country) && string.Equals(ps.Country, "IN", StringComparison.OrdinalIgnoreCase))
                && (!string.IsNullOrEmpty(ps.Currency) && string.Equals(ps.Currency, "INR", StringComparison.OrdinalIgnoreCase))
                && (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXReturnFailedSessionState)))
            {
                ps.ChallengeStatus = PaymentChallengeStatus.Failed;
            }

            return new BrowserFlowContext
            {
                IsFingerPrintRequired = false,
                IsAcsChallengeRequired = false,
                PaymentSession = ps
            };
        }

        private static PaymentSession GetSafetyNetPaymentSession(
            PaymentSessionData paymentSessionData,
            PaymentChallengeStatus challengeStatus = PaymentChallengeStatus.NotApplicable)
        {
            var ps = new PaymentSession(paymentSessionData);
            ps.Id = Guid.NewGuid().ToString();
            ps.IsChallengeRequired = false;
            ps.ChallengeStatus = challengeStatus;
            ps.Signature = ps.GenerateSignature();
            return ps;
        }

        private static PaymentSession GetSafetyNetPaymentSession(
            string id,
            bool isChallengeRequired = true,
            PaymentChallengeStatus challengeStatus = PaymentChallengeStatus.Succeeded)
        {
            var ps = new PaymentSession()
            {
                Id = id,
                IsChallengeRequired = isChallengeRequired,
                ChallengeStatus = challengeStatus
            };

            ps.Signature = ps.GenerateSignature();
            return ps;
        }

        private static AuthenticationResponse GetSafetyNetAuthenticationResponse()
        {
            return new AuthenticationResponse()
            {
                EnrollmentStatus = PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentStatus.Bypassed,
                ChallengeStatus = PaymentChallengeStatus.Succeeded
            };
        }

        private static PayerAuth.CompletionResponse GetSafetyNetCompletionResponse()
        {
            return new PayerAuth.CompletionResponse()
            {
                ChallengeCompletionIndicator = PXService.Model.PayerAuthService.ChallengeCompletionIndicator.Y,
            };
        }

        private static PayerAuth.CompletionResponse GetSafetyNetThreeDSOneCompletionResponse()
        {
            return new PayerAuth.CompletionResponse()
            {
                TransactionStatus = PayerAuth.TransactionStatus.R,
                ChallengeCompletionIndicator = PXService.Model.PayerAuthService.ChallengeCompletionIndicator.Y,
            };
        }

        private static PaymentChallengeStatus GetMappedStatusForAuthentication(
            PayerAuth.TransactionStatus transStatus,
            bool isMoto,
            List<string> additionalInputs,
            List<string> exposedFlights)
        {
            var mappedStatusString = GetMappedStatusString(
                 mapPrefix: "PXPSD2Auth",
                 transStatus: transStatus,
                 isMoto: isMoto,
                 additionalInputs: additionalInputs,
                 exposedFlights: exposedFlights);

            var mappedStatus = PaymentChallengeStatus.Succeeded;
            if (mappedStatusString == null || !Enum.TryParse<PaymentChallengeStatus>(mappedStatusString, true, out mappedStatus))
            {
                // Fallback to hardcoded status mapping if parsing from flights did not work for some reasons
                if (transStatus == PayerAuth.TransactionStatus.C)
                {
                    mappedStatus = PaymentChallengeStatus.Unknown;
                }
                else if (transStatus == PayerAuth.TransactionStatus.R)
                {
                    mappedStatus = PaymentChallengeStatus.Failed;
                }
                else
                {
                    mappedStatus = isMoto ? PaymentChallengeStatus.ByPassed : PaymentChallengeStatus.Succeeded;
                }
            }

            if (transStatus == PayerAuth.TransactionStatus.FR)
            {
                mappedStatus = PaymentChallengeStatus.Failed;
            }

            return mappedStatus;
        }

        private static PaymentChallengeStatus GetMappedStatusForThreeDSOneAuthentication(PayerAuth.TransactionStatus transStatus)
        {
            var mappedStatus = PaymentChallengeStatus.Succeeded;
            if (transStatus == PayerAuth.TransactionStatus.C)
            {
                mappedStatus = PaymentChallengeStatus.Unknown;
            }
            else if (transStatus == PayerAuth.TransactionStatus.U || transStatus == PayerAuth.TransactionStatus.N)
            {
                mappedStatus = PaymentChallengeStatus.Failed;
            }

            return mappedStatus;
        }

        /// <summary>
        /// At the time of PSD2 launch, there was no clarity on how external actors (banks, processors) would interpret
        /// PSD2 specifications.  For example, during Completion API (mapPrefix "PXPSD2Comp"), if transaction status is "N"
        /// and transaction status reason is TSR10 and status indicator is 01, does it mean that the flow is successful,
        /// cancelled, failed, or timedout - it was unclear.  So, we had to have this mapping be configurable and use flights
        /// as configuration that we could tweak without requiring a re-deployment.  Similar ambiguity existed about how to
        /// interpret returned values during the Auth call (mapPrefix "PXPSD2Auth").  Hence the below function to find mapping
        /// if it exists as a flight name for the current set of returned values.
        /// Details:
        /// It creates every combination of strings in the additionalInputs list and appends it to "{mapPrefix}-{transStatus}-"
        /// and verifies if a flight begins with that string.  On first match, returns the flight suffix.  For example, let's
        /// say inputs are as below:
        ///  - mapPrefix = "PXPSD2Comp"
        ///  - transStatus = "N"
        ///  - additionalInputs = { "TSR10", "01" }
        /// For these inputs, the function generates the following strings (order is important) and for each, it checks if
        /// exposedFlights has any string that begins with it.
        ///  1. PXPSD2Comp-N-TSR10-01-
        ///  2. PXPSD2Comp-N-_-01-
        ///  3. PXPSD2Comp-N-TSR10-_-
        ///  4. PXPSD2Comp-N-_-_-
        /// On first match, it returns the suffix.  So, let's say exposedFlights contains the following (order is NOT important):
        ///  - PXPSD2Comp-R-_-_-Failed
        ///  - PXPSD2Comp-N-_-04-TimedOut
        ///  - PXPSD2Comp-N-TSR10-_-Failed
        ///  - PXPSD2Comp-_-_-01-Cancelled
        ///  - PXPSD2Comp-_-_-_-Succeeded
        /// The function returns "Failed" because in this example, "PXPSD2Comp-N-TSR10-_-Failed" is the first (and only) match
        /// </summary>
        /// <param name="mapPrefix">e.g. PXPSD2Comp</param>
        /// <param name="transStatus">e.g. N</param>
        /// <param name="isMoto">On behalf of scenario</param>
        /// <param name="additionalInputs">Additional inputs that can affect what the outcome is.  All combination of these will be added to the return string</param>
        /// <param name="exposedFlights">Flights that are ON currently</param>
        /// <returns>Given the above inputs, returns the output indicating if we should consider this transaction Cancelled, Failed, Timedout etc.</returns>
        private static string GetMappedStatusString(
            string mapPrefix,
            PayerAuth.TransactionStatus transStatus,
            bool isMoto,
            List<string> additionalInputs,
            List<string> exposedFlights)
        {
            // Get mapped status from flights
            try
            {
                for (int i = 0; i < Math.Pow(2, additionalInputs.Count); i++)
                {
                    string input = string.Format("{0}-{1}-", mapPrefix, transStatus.ToString());

                    // Append every possible combination of strings in additionalInputs.  "_" is a wildcard (we couldn't use
                    // "*" because its not allowed to be part of a flight name on carbon.  Hence using "_" instead).
                    for (int j = 0; j < additionalInputs.Count; j++)
                    {
                        // Bitwise shift 1 (0000 0000 0000 0001) left by j positions and logical-and with i
                        input += ((i & (1 << j)) == 0) ? additionalInputs[j] ?? string.Empty : "_";
                        input += "-";
                    }

                    var inputOutputMap = exposedFlights.FirstOrDefault((f) => f.StartsWith(input, StringComparison.OrdinalIgnoreCase));
                    if (inputOutputMap != null)
                    {
                        return inputOutputMap.Substring(input.Length);
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static bool PiRequiresAuthentication(bool piRequires3ds2Authentication, bool piRequires3ds1Authentication, PimsModel.V4.PaymentMethod pm, List<string> exposedFlightFeatures)
        {
            if (pm.IsUpiQr() && exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.EnableLtsUpiQRConsumer, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            return piRequires3ds2Authentication || piRequires3ds1Authentication || pm.IsUpi();
        }

        private static PSD2NativeChallengeLocalizations GetPSD2NativeChallengeLocalizations(string language = "en-GB")
        {
            string header = LocalizationRepository.Instance.GetLocalizedString(V7.Constants.PSD2NativeChallengeStrings.Header, language);
            string cancel = LocalizationRepository.Instance.GetLocalizedString(V7.Constants.PSD2NativeChallengeStrings.CancelButtonLabel, language);
            string back = LocalizationRepository.Instance.GetLocalizedString(V7.Constants.PSD2NativeChallengeStrings.BackButtonLabel, language);
            string pressBack = LocalizationRepository.Instance.GetLocalizedString(V7.Constants.PSD2NativeChallengeStrings.BackButtonAccessibilityLabel, language);
            string pressCancel = LocalizationRepository.Instance.GetLocalizedString(V7.Constants.PSD2NativeChallengeStrings.CancelButtonAccessibilityLabel, language);
            string ordering = LocalizationRepository.Instance.GetLocalizedString(V7.Constants.PSD2NativeChallengeStrings.OrderingAccessibilityLabel, language);
            string bankLogo = LocalizationRepository.Instance.GetLocalizedString(V7.Constants.PSD2NativeChallengeStrings.BankLogoAccessibilityLabel, language);
            string cardLogo = LocalizationRepository.Instance.GetLocalizedString(V7.Constants.PSD2NativeChallengeStrings.CardLogoAccessibilityLabel, language);

            return new PSD2NativeChallengeLocalizations
            {
                ChallengePageHeader = header,
                BackButtonLabel = back,
                BackButtonAccessibilityLabel = pressBack,
                CancelButtonLabel = cancel,
                CancelButtonAccessibilityLabel = pressCancel,
                OrderingAccessibilityLabel = ordering,
                BankLogoAccessibilityLabel = bankLogo,
                CardLogoAccessibilityLabel = cardLogo
            };
        }

        private async Task<BrowserFlowContext> Authenticate(
            PayerAuth.ThreeDSMethodCompletionIndicator threeDSMethodCompletionIndicator,
            List<string> exposedFlightFeatures,
            EventTraceActivity traceActivityId,
            IEnumerable<KeyValuePair<string, string>> piQueryParams = null)
        {
            // 1. Create an AuthRequest object from stored session
            PayerAuth.AuthenticationRequest authRequest = new PayerAuth.AuthenticationRequest()
            {
                PaymentSession = new PayerAuth.PaymentSession(this.storedSession),
                BrowserInfo = this.storedSession.BrowserInfo,
                ThreeDSServerTransId = this.storedSession.MethodData.ThreeDSServerTransID,
                ThreeDSMethodCompletionIndicator = threeDSMethodCompletionIndicator,
                AcsChallengeNotificationUrl = string.Format(
                    "{0}/paymentSessions/{1}/NotifyThreeDSChallengeCompleted",
                    this.PifdBaseUrl,
                    this.storedSession.Id),
                RiskChallengIndicator = PayerAuth.RiskChallengeIndicator.NoPreference,
                MessageVersion = GlobalConstants.PSD2Constants.DefaultBrowserRequestMessageVersion
            };

            // 2. Call PayerAuth.Authenticate
            var authResponse = await this.PayerAuthServiceAccessor.Authenticate(authRequest, traceActivityId);
            this.storedSession.TransactionStatus = authResponse.TransactionStatus;
            this.storedSession.TransactionStatusReason = authResponse.TransactionStatusReason;
            await this.SafetyNetUpdateSession(traceActivityId);

            // TODO : Remove after Fraud Investigation
            SllWebLogger.TraceServerMessage("V1 Authenticate App", null, this.storedSession.Id, authResponse?.TransactionStatus != null ? "TransStatus: " + authResponse.TransactionStatus.ToString() : "No transStatus available", EventLevel.Informational);

            // 3. Return if challenge is not required.
            PaymentSession paymentSession = new PaymentSession(this.storedSession);
            paymentSession.Signature = paymentSession.GenerateSignature();

            var mappedStatus = GetMappedStatusForAuthentication(
                transStatus: authResponse.TransactionStatus,
                isMoto: this.storedSession.IsMOTO,
                additionalInputs: new List<string>() { authResponse.TransactionStatusReason.ToString() },
                exposedFlights: this.storedSession.ExposedFlightFeatures);

            paymentSession.ChallengeStatus = mappedStatus;
            if (mappedStatus != PaymentChallengeStatus.Unknown)
            {
                this.storedSession.ChallengeStatus = mappedStatus;

                if (IsAuthenticationVerified(mappedStatus))
                {
                    await this.PostProcessOnSuccess(
                        traceActivityId: traceActivityId,
                        piQueryParams: piQueryParams);
                }
                else
                {
                    await this.SafetyNetUpdateSession(traceActivityId);
                }

                var authenticationVerified = IsAuthenticationVerified(mappedStatus);

                var accountId = string.Empty;
                if (!string.IsNullOrWhiteSpace(this.storedSession.AccountId))
                {
                    accountId = this.storedSession.AccountId;
                }
                else if (!string.IsNullOrWhiteSpace(this.storedSession.PaymentInstrumentAccountId))
                {
                    accountId = this.storedSession.PaymentInstrumentAccountId;
                }
                else if (!string.IsNullOrWhiteSpace(this.storedSession.BillableAccountId))
                {
                    accountId = this.storedSession.BillableAccountId;
                }
                else if (!string.IsNullOrWhiteSpace(this.storedSession.CommercialAccountId))
                {
                    accountId = this.storedSession.CommercialAccountId;
                }

                await this.TransactionDataServiceAccessor.UpdateCustomerChallengeAttestation(accountId, this.storedSession.Id, authenticationVerified, traceActivityId);

                return new BrowserFlowContext()
                {
                    IsFingerPrintRequired = false,
                    IsAcsChallengeRequired = false,
                    PaymentSession = paymentSession,
                    CardHolderInfo = authResponse.CardHolderInfo,
                };
            }

            // 4. Update stored session with AuthResponse
            this.storedSession.AuthenticationResponse = authResponse;
            await this.SessionServiceAccessor.UpdateSessionResourceData<PXInternal.PaymentSession>(
                sessionId: this.storedSession.Id,
                newSessionData: this.storedSession,
                traceActivityId: traceActivityId);

            // 5. Calculate the Form Data to Return for Challenge Scenario.
            ChallengeRequest creq = new ChallengeRequest
            {
                ThreeDSServerTransID = authResponse.ThreeDSServerTransactionId,
                AcsTransID = authResponse.AcsTransactionId,
                MessageType = "CReq",
                MessageVersion = authResponse.MessageVersion ?? GlobalConstants.PSD2Constants.FallbackMessageVersion,
                ChallengeWindowSize = this.storedSession.BrowserInfo.ChallengeWindowSize
            };

            ThreeDSSessionData threeDSSessionData = new ThreeDSSessionData
            {
                ThreeDSServerTransID = authResponse.ThreeDSServerTransactionId,
                AcsTransID = authResponse.AcsTransactionId,
            };

            paymentSession.ChallengeStatus = PaymentChallengeStatus.Unknown;

            WindowSize wz = null;
            ChallengeWindowSizes.TryGetValue(this.storedSession.BrowserInfo.ChallengeWindowSize, out wz);

            if (wz == null)
            {
                throw new KeyNotFoundException(
                    string.Format(
                        "Not matching window size found for {0}",
                        this.storedSession.BrowserInfo.ChallengeWindowSize));
            }

            return new BrowserFlowContext()
            {
                IsFingerPrintRequired = false,
                IsAcsChallengeRequired = true,
                FormInputThreeDSSessionData = ThreeDSUtils.EncodeUrl(ThreeDSUtils.EncodeObjectToBase64(threeDSSessionData)),
                FormInputCReq = ThreeDSUtils.EncodeUrl(ThreeDSUtils.EncodeObjectToBase64(creq)),
                FormActionURL = authResponse.AcsUrl,
                CardHolderInfo = authResponse.CardHolderInfo,
                PaymentSession = paymentSession,
                ChallengeWindowSize = wz,
                TransactionSessionId = authResponse.TransactionSessionId
            };
        }

        private async Task<BrowserFlowContext> AuthenticateThreeDSOne(
           string cvvToken,
           IEnumerable<KeyValuePair<string, string>> piQueryParams,
           EventTraceActivity traceActivityId,
           string userId)
        {
            // 1. Create an AuthRequest object from stored session
            PayerAuth.AuthenticationRequest authRequest = new PayerAuth.AuthenticationRequest()
            {
                PaymentSession = new PayerAuth.PaymentSession(this.storedSession)
                {
                    CvvToken = cvvToken,
                    UserId = userId
                },
                BrowserInfo = this.storedSession.BrowserInfo,
                AcsChallengeNotificationUrl = string.Format(
                    "{0}/paymentSessions/{1}/BrowserNotifyThreeDSOneChallengeCompleted",
                    this.PifdBaseUrl,
                    this.storedSession.Id)
            };

            // 2. Call PayerAuth.Authenticate
            var authResponse = await this.PayerAuthServiceAccessor.AuthenticateThreeDSOne(authRequest, traceActivityId);

            // 3. Return if challenge is not required.
            PaymentSession paymentSession = new PaymentSession(this.storedSession);
            paymentSession.Signature = paymentSession.GenerateSignature();

            var mappedStatus = GetMappedStatusForThreeDSOneAuthentication(authResponse.TransactionStatus);

            paymentSession.ChallengeStatus = mappedStatus;
            if (mappedStatus != PaymentChallengeStatus.Unknown)
            {
                // In the case of 3DS 1.0, we should never get a status other than Unknown
                if (IsAuthenticationVerified(mappedStatus))
                {
                    await this.PostProcessOnSuccess(
                        traceActivityId: traceActivityId,
                        piQueryParams: piQueryParams);
                }

                return new BrowserFlowContext()
                {
                    IsAcsChallengeRequired = false,
                    PaymentSession = paymentSession,
                    CardHolderInfo = authResponse.CardHolderInfo,
                };
            }

            // 4. Update stored session with AuthResponse
            this.storedSession.AuthenticationResponse = authResponse;
            await this.SessionServiceAccessor.UpdateSessionResourceData<PXInternal.PaymentSession>(
                sessionId: this.storedSession.Id,
                newSessionData: this.storedSession,
                traceActivityId: traceActivityId);

            // 5. Calculate the Form Data to Return for Challenge Scenario.
            WindowSize wz = null;
            ChallengeWindowSizes.TryGetValue(this.storedSession.BrowserInfo.ChallengeWindowSize, out wz);

            return new BrowserFlowContext()
            {
                IsAcsChallengeRequired = true,
                FormInputCReq = authResponse.AcsSignedContent,
                FormActionURL = authResponse.AcsUrl,
                FormPostAcsURL = authResponse.IsFormPostAcsUrl,
                FormFullPageRedirectAcsURL = authResponse.IsFullPageRedirect,
                CardHolderInfo = authResponse.CardHolderInfo,
                PaymentSession = paymentSession,
                ChallengeWindowSize = wz
            };
        }

        private async Task<bool> ValidatePI(
            EventTraceActivity traceActivityId,
            IEnumerable<KeyValuePair<string, string>> piQueryParams = null)
        {
            bool isValidatePiSuccessful = true;
            await CallSafetyNetOperation(
                operation: async () =>
                {
                    // For ValidatePIOnAttachChallenge, call ValidatePaymentInstrument on PIMS only when the payment (session) is for a pre-order or for Zero amount
                    if (this.storedSession.HasPreOrder
                        || this.storedSession.Amount == 0)
                    {
                        var validatePiResponse = await this.PimsAccessor.ValidatePaymentInstrument(
                            accountId: this.storedSession.PaymentInstrumentAccountId,
                            piid: this.storedSession.PaymentInstrumentId,
                            payload: new ValidatePaymentInstrument(
                                this.storedSession.Id,
                                new RiskData()
                                {
                                    UserInfo = new UserInfo()
                                    {
                                        Email = this.storedSession.EmailAddress
                                    }
                                }),
                            traceActivityId: traceActivityId,
                            queryParams: piQueryParams);

                        isValidatePiSuccessful = !string.Equals(validatePiResponse.Result, V7.Constants.ValidateResultMessages.Failed, StringComparison.OrdinalIgnoreCase);
                    }
                },
                traceActivityId,
                exposedFeatures: this.storedSession?.ExposedFlightFeatures,
                excludeErrorFeatureFormat: "PSD2SafetyNet-ValidatePI-{0}-{1}");

            return isValidatePiSuccessful;
        }

        private async Task PostProcessOnSuccess(
            EventTraceActivity traceActivityId,
            IEnumerable<KeyValuePair<string, string>> piQueryParams = null)
        {
            // 1. Call ValidatePI if PreOrder or MIT (subscription setup) to notify PIMS->PayMod
            await this.ValidatePI(traceActivityId, piQueryParams);

            // 2. Save session against PI
            await CallSafetyNetOperation(
                operation: async () =>
                {
                    await this.PimsAccessor.LinkSession(
                        accountId: this.storedSession.PaymentInstrumentAccountId,
                        piid: this.storedSession.PaymentInstrumentId,
                        payload: new LinkSession(this.storedSession.Id),
                        traceActivityId: traceActivityId,
                        queryParams: piQueryParams);
                },
                traceActivityId,
                exposedFeatures: this.storedSession.ExposedFlightFeatures,
                excludeErrorFeatureFormat: "PSD2SafetyNet-LinkSessionToPI-{0}-{1}");

            // 3. Update session
            await this.SafetyNetUpdateSession(traceActivityId);
        }

        private async Task PostProcessOnSuccessIndiaThreeDS(
            EventTraceActivity traceActivityId,
            string sessionId,
            IEnumerable<KeyValuePair<string, string>> piQueryParams = null)
        {
            await CallSafetyNetOperation(
                operation: async () =>
                {
                    await this.PimsAccessor.LinkSession(
                        accountId: this.storedSession.PaymentInstrumentAccountId,
                        piid: this.storedSession.PaymentInstrumentId,
                        payload: new LinkSession(sessionId),
                        traceActivityId: traceActivityId,
                        queryParams: piQueryParams);
                },
                traceActivityId,
                exposedFeatures: this.storedSession.ExposedFlightFeatures,
                excludeErrorFeatureFormat: "PSD2SafetyNet-LinkSessionToPI-{0}-{1}");
        }

        private async Task CachedGetStoredSession(
            string sessionId,
            EventTraceActivity traceActivityId)
        {
            // 1. If local copy of the stored session exists but is not the corect one (should never happen),
            // clear it and log telemetry
            if (this.storedSession != null && this.storedSession.Id != sessionId)
            {
                // TODO: Log telemetry but continue to service the request
                this.storedSession = null;
            }

            // 2. Get stored session if local copy is not available
            if (this.storedSession == null)
            {
                this.storedSession = await this.SessionServiceAccessor.GetSessionResourceData<PXInternal.PaymentSession>(
                    sessionId: sessionId,
                    traceActivityId: traceActivityId);
            }
        }

        // The following method will only be called if the payment method type is a credit card
        private PaymentChallengeStatus ValidateACSSignedContent(
            AuthenticationRequest authRequest,
            PaymentChallengeStatus mappedStatus,
            List<string> exposedFlightFeatures,
            PayerAuth.AuthenticationResponse payerAuthResponse,
            TestContext testContext,
            EventTraceActivity traceActivityId,
            string sessionId)
        {
            string version = GetLatestAvailablePSD2SettingVersion(exposedFlightFeatures);

            // Only versions above V17 will have the attribute caRootCertificates, which is prefered to parse through.
            if (int.Parse(version.Substring(1, 2)) <= V7.Constants.RootCertVersion.UpdatedVersionInt17)
            {
                version = V7.Constants.RootCertVersion.UpdatedVersion17;
            }

            string paymentMethodType = this.storedSession.PaymentMethodType;
            try
            {
                string paymentClientConfig = File.ReadAllText(
                                            Path.Combine(
                                                AppDomain.CurrentDomain.BaseDirectory,
                                                string.Format(@"App_Data\PSD2Config\certs-{0}\DsCertificates.json", version)));

                PaymentClientSettingsData settingsConfig = JsonConvert.DeserializeObject<PaymentClientSettingsData>(paymentClientConfig);

                if (!string.IsNullOrEmpty(paymentMethodType))
                {
                    DirectoryServerData dsInfo = null;

                    if (!settingsConfig.DirectoryServerInfo.TryGetValue(paymentMethodType, out dsInfo))
                    {
                        mappedStatus = PaymentChallengeStatus.Failed;

                        SllWebLogger.TracePXServiceIntegrationError(
                            V7.Constants.ServiceNames.DsCertificateValidation,
                            IntegrationErrorCode.DSInfoNotFound,
                            $"{V7.Constants.PSD2ValidationErrorMessage.PaymentMethodTypeIssue} Payment Method Type: {paymentMethodType}, SessionId: {sessionId} ",
                            traceActivityId.ToString());
                        return mappedStatus;
                    }

                    List<string> certs = dsInfo.CaRootCertificates ?? new List<string> { dsInfo.CaRootCertificate };

                    PaymentPSD2CertificatesValidator certVal = new PaymentPSD2CertificatesValidator(payerAuthResponse.AcsSignedContent, certs, testContext, sessionId);

                    if (!certVal.VerifySignature(traceActivityId))
                    {
                        // Intentially kept status as Unknown in order to mitigate blockage.
                        // Errors will be logged and data regarding this change will be monitored.
                        // If there are not many unexpected errors, the status will be switched back to Failed
                        // Update PaymentSessionsHandlerTests (ValidatePaymentChallenge_CheckCertificateValidation_UnknownOrFailedAsync) accordingly.
                        mappedStatus = PaymentChallengeStatus.Unknown;
                        SllWebLogger.TracePXServiceIntegrationError(
                            V7.Constants.ServiceNames.DsCertificateValidation,
                            IntegrationErrorCode.InvalidBuild,
                            $"Mapped Status intentially passed as Unknown, but there was an issue in PaymentPSD2CertificatesValidator.cs. Check the logs to determine the error. " +
                            $"{V7.Constants.PSD2ValidationErrorMessage.PaymentMethodTypeIssue} Payment Method Type: {paymentMethodType}, SessionId: {sessionId}",
                            traceActivityId.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                mappedStatus = PaymentChallengeStatus.Failed;

                SllWebLogger.TracePXServiceException(
                    ex.ToString(),
                    traceActivityId);
            }

            return mappedStatus;
        }

        private async Task<bool> SafetyNetUpdateSession(EventTraceActivity traceActivityId)
        {
            return await CallSafetyNetOperation(
                async () =>
                {
                    await this.SessionServiceAccessor.UpdateSessionResourceData<PXInternal.PaymentSession>(
                    sessionId: this.storedSession.Id,
                    newSessionData: this.storedSession,
                    traceActivityId: traceActivityId);
                },
                traceActivityId,
                exposedFeatures: this.storedSession.ExposedFlightFeatures,
                excludeErrorFeatureFormat: "PSD2SafetyNet-UpdateSessionResourceData-{0}-{1}");
        }
    }
}