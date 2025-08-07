// <copyright file="CheckoutRequestsExController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentClient
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Tracing;
    using System.Diagnostics.Eventing.Reader;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Common.Web;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService;
    using Microsoft.Commerce.Payments.PXService.V7.Contexts;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Microsoft.Identity.Client;
    using Newtonsoft.Json;
    using OpenTelemetry.Resources;
    using ClientAction = PXCommon.ClientAction;
    using ClientActionType = PXCommon.ClientActionType;
    using PayerAuth = PXService.Model.PayerAuthService;

    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutRequestsExController : ProxyController
    {
        /// <summary>
        /// Attach address to checkoutRequest
        /// </summary>
        /// <group>CheckoutRequestsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/paymentClient/checkoutRequestsEx/{checkoutRequestId}/attachAddress</url>
        /// <param name="address" required="true" cref="object" in="body">address object</param>
        /// <param name="checkoutRequestId" required="true" cref="string" in="path">checkout request ID</param>
        /// <param name="type" required="false" cref="string" in="query">type of address</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="scenario" required="false" cref="string" in="query">Scenario name</param>
        /// <response code="200">CheckoutRequest object</response>
        /// <returns>CheckoutRequest object</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> AttachAddress(
            [FromBody] PIDLData address,
            string checkoutRequestId,
            string type = null,
            string partner = V7.Constants.ServiceDefaults.DefaultPartnerName,
            string scenario = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            //// TODO use our own address service for address validation
            object result;
            try
            {
                result = await this.Settings.AccountServiceAccessor.LegacyValidateAddress(address, traceActivityId);
            }
            catch (ServiceErrorResponseException ex)
            {
                return Request.CreateResponse(ex.Response.StatusCode, ex.Error);
            }

            if (this.UsePaymentRequestApiEnabled())
            {
                PaymentRequestClientActions paymentRequest = await this.Settings.PaymentOrchestratorServiceAccessor.AttachAddressToPaymentRequest(CheckoutRequestsExHandler.ConvertPIDLDataToPOAddress(address), type, traceActivityId, checkoutRequestId);
                if (string.Equals(scenario, V7.Constants.ScenarioNames.MergeData, StringComparison.OrdinalIgnoreCase))
                {
                    paymentRequest.ClientAction = new PXCommon.ClientAction(PXCommon.ClientActionType.MergeData, CheckoutRequestsExHandler.CreateMergeDataActionContextForAttachAddress(paymentRequest));
                }

                return this.Request.CreateResponse(paymentRequest);
            }
            else
            {
                CheckoutRequestClientActions checkoutRequest = await this.Settings.PaymentOrchestratorServiceAccessor.AttachAddress(CheckoutRequestsExHandler.ConvertPIDLDataToPOAddress(address), type, traceActivityId, checkoutRequestId);

                if (string.Equals(scenario, V7.Constants.ScenarioNames.MergeData, StringComparison.OrdinalIgnoreCase))
                {
                    checkoutRequest.ClientAction = new PXCommon.ClientAction(PXCommon.ClientActionType.MergeData, CheckoutRequestsExHandler.CreateMergeDataActionContextForAttachAddress(checkoutRequest));
                }

                return this.Request.CreateResponse(checkoutRequest);
            }
        }

        /// <summary>
        /// Attach profile to checkoutRequest
        /// </summary>
        /// <group>CheckoutRequestsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/paymentClient/checkoutRequestsEx/{checkoutRequestId}/attachProfile</url>
        /// <param name="profile" required="true" cref="object" in="body">profile object</param>
        /// <param name="checkoutRequestId" required="true" cref="string" in="path">type of address</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <response code="200">CheckoutRequest object</response>
        /// <returns>CheckoutRequest object</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> AttachProfile(
            [FromBody] PIDLData profile,
            string checkoutRequestId,
            string partner = V7.Constants.ServiceDefaults.DefaultPartnerName)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            string email = profile?.TryGetPropertyValueFromPIDLData(V7.Constants.CheckoutRequestPropertyName.Email);

            if (string.IsNullOrEmpty(email))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "InvalidPIData", "The input email is null or empty.")));
            }

            if (this.UsePaymentRequestApiEnabled())
            {
                // Attach profile to payment request
                PaymentRequestClientActions paymentRequest = await this.Settings.PaymentOrchestratorServiceAccessor.AttachProfileToPaymentRequest(email, traceActivityId, checkoutRequestId);

                return this.Request.CreateResponse(paymentRequest);
            }
            else
            {
                // Attach profile to checkout request
                CheckoutRequestClientActions checkoutRequest = await this.Settings.PaymentOrchestratorServiceAccessor.AttachProfile(email, traceActivityId, checkoutRequestId);

                return this.Request.CreateResponse(checkoutRequest);
            }
        }

        /// <summary>
        /// confirm checkout request
        /// </summary>
        /// <group>CheckoutRequestsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/paymentClient/checkoutRequestsEx/{checkoutRequestId}/confirm</url>
        /// <param name="confirmPayload" required="true" cref="object" in="body">payment instrument object including piid</param>
        /// <param name="checkoutRequestId" required="true" cref="string" in="path">type of address</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <response code="200">CheckoutRequest object</response>
        /// <returns>CheckoutRequest object</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> Confirm(
            [FromBody] PIDLData confirmPayload,
            string checkoutRequestId,
            string partner = V7.Constants.TemplateName.DefaultTemplate)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            CheckoutRequestClientActions checkoutRequest = new CheckoutRequestClientActions();
            PaymentRequestClientActions paymentRequest = new PaymentRequestClientActions();

            string piid = confirmPayload?.TryGetPropertyValueFromPIDLData(V7.Constants.CheckoutRequestPropertyName.PIID);
            string paymentMethodType = confirmPayload?.TryGetPropertyValueFromPIDLData(V7.Constants.CheckoutRequestPropertyName.PaymentMethodType);
            var requestContext = this.GetRequestContext(traceActivityId);
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(V7.Constants.Operations.Confirm);

            if (!string.IsNullOrEmpty(paymentMethodType)
                && (paymentMethodType.Equals(V7.Constants.PaymentMethodType.ApplePay, StringComparison.OrdinalIgnoreCase)
                    || paymentMethodType.Equals(V7.Constants.PaymentMethodType.GooglePay, StringComparison.OrdinalIgnoreCase)))
            {
                // Extract apple pay or google pay token as string from payload.
                string expressCheckoutData = confirmPayload?.TryGetPropertyValueFromPIDLData(V7.Constants.CheckoutRequestPropertyName.ExpressCheckoutPaymentData);
                string country = confirmPayload?.TryGetPropertyValueFromPIDLData(V7.Constants.CheckoutRequestPropertyName.Country);

                // Transform payment token into appropriate payment type data.
                var paymentTokenHandler = ExternalPaymentTokenTransformerFactory.Instance(paymentMethodType, expressCheckoutData, traceActivityId);

                // Get Email Address and post it to PO
                string email = paymentTokenHandler.ExtractEmailAddress();
                if (this.UsePaymentRequestApiEnabled())
                {
                    await this.Settings.PaymentOrchestratorServiceAccessor.AttachProfileToPaymentRequest(email, traceActivityId, checkoutRequestId);
                }
                else
                {
                    await this.Settings.PaymentOrchestratorServiceAccessor.AttachProfile(email, traceActivityId, checkoutRequestId);
                }

                // Get address from payment token and call PO attach address
                Address address = paymentTokenHandler.ExtractAddress();
                if (this.UsePaymentRequestApiEnabled())
                {
                    await this.Settings.PaymentOrchestratorServiceAccessor.AttachAddressToPaymentRequest(address, V7.Constants.AddressTypes.Billing, traceActivityId, checkoutRequestId);
                }
                else
                {
                    await this.Settings.PaymentOrchestratorServiceAccessor.AttachAddress(address, V7.Constants.AddressTypes.Billing, traceActivityId, checkoutRequestId);
                }

                // Get PI from payment token
                PIDLData pi = paymentTokenHandler.ExtractPaymentInstrument();

                // Post express checkout data to PIMS and get PIID
                piid = await this.PostPItoPIMS(pi, country, partner, traceActivityId);
            }

            if (string.IsNullOrEmpty(piid))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "InvalidPIData", "The input email is null or empty.")));
            }

            // remove the else condition block and related functions once the flight is 100% enabled
            if (this.UsePaymentRequestApiEnabled())
            {
                paymentRequest = await this.Settings.PaymentOrchestratorServiceAccessor.ConfirmToPaymentRequest(piid, traceActivityId, checkoutRequestId);

                if (IsPaymentInstrumentChallengeRequired(paymentRequest))
                {
                    PaymentRequestConfirmDescription paymentRequestConfirmDescription = new PaymentRequestConfirmDescription(requestContext, Generate3DS2ChallengePIDLResource);
                    paymentRequestConfirmDescription.PaymentRequestClientActions = paymentRequest;
                    await paymentRequestConfirmDescription.LoadComponentDescription(
                        checkoutRequestId,
                        this.Settings,
                        traceActivityId,
                        setting,
                        this.ExposedFlightFeatures,
                        partner: partner,
                        type: paymentMethodType,
                        request: this.Request.ToHttpRequestMessage(),
                        piid: piid,
                        country: paymentRequest.Country,
                        operation: V7.Constants.Operations.Add,
                        language: paymentRequest.Language);

                    List<PIDLResource> pidlResources = await paymentRequestConfirmDescription.GetDescription();
                    if (pidlResources != null && pidlResources.Count > 0)
                    {
                        // For PaymentClient, as confirm is not the first PIDLSDK call, we will need to make it as PIDL clientAction to show the PIDL.
                        PIDLResource pidlClientAction = pidlResources[0];
                        if (pidlClientAction.ClientAction == null)
                        {
                            pidlClientAction = new PIDLResource
                            {
                                ClientAction = new ClientAction(ClientActionType.Pidl, pidlResources),
                            };
                        }

                        return this.Request.CreateResponse(pidlClientAction);
                    }
                    else
                    {
                        SllWebLogger.TraceServerMessage("CheckoutRequestEx Confirm", traceActivityId.CorrelationVectorV4.Value, traceActivityId.ActivityId.ToString(), "challenge is required but ChallengePIDL is null", System.Diagnostics.Tracing.EventLevel.Warning);
                    }
                }

                return this.Request.CreateResponse(paymentRequest);
            }
            else
            {
                checkoutRequest = await this.Settings.PaymentOrchestratorServiceAccessor.Confirm(piid, traceActivityId, checkoutRequestId);

                if (HasThreeDs2Challenge(checkoutRequest))
                {
                    PIDLResource pidlResource = await this.Generate3DS2ChallengePIDLResource(
                        GetPaymentSessionData(partner, checkoutRequest),
                        requestContext,
                        traceActivityId,
                        setting);

                    if (pidlResource != null)
                    {
                        return this.Request.CreateResponse(pidlResource);
                    }
                    else
                    {
                        SllWebLogger.TraceServerMessage("CheckoutRequestEx Confirm", traceActivityId.CorrelationVectorV4.Value, traceActivityId.ActivityId.ToString(), "ThreeDs2 challenge is required but 3DS2ChallengePIDL is null", System.Diagnostics.Tracing.EventLevel.Warning);
                    }
                }

                return this.Request.CreateResponse(checkoutRequest);
            }
        }

        /// <summary>
        /// attach payment instrument
        /// </summary>
        /// <group>CheckoutRequestsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/paymentClient/checkoutRequestsEx/{checkoutRequestId}/attachPaymentInstrument</url>
        /// <param name="paymentInstrument" required="true" cref="object" in="body">payment instrument object including piid</param>
        /// <param name="checkoutRequestId" required="true" cref="string" in="path">type of address</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <response code="200">CheckoutRequest object</response>
        /// <returns>CheckoutRequest object</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> AttachPaymentInstrument(
            [FromBody] PIDLData paymentInstrument,
            string checkoutRequestId,
            string partner = V7.Constants.ServiceDefaults.DefaultPartnerName)
        {
            //// This API is not in use and will be removed in future
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            AttachPaymentInstrumentResponse checkoutRequest = await this.Settings.PaymentOrchestratorServiceAccessor.AttachPaymentInstrumentToPaymentRequest(checkoutRequestId, "piid", "cvvToken", traceActivityId, null);

            return this.Request.CreateResponse(checkoutRequest);
        }

        //// TODO: Delete this function after cr and pr are merged
        private static PaymentSessionData GetPaymentSessionData(string partner, CheckoutRequestClientActions checkoutRequestClientActions)
        {
            var clientAction = checkoutRequestClientActions.ClientActions?.FirstOrDefault();
            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                PaymentInstrumentId = clientAction != null ? clientAction.PaymentInstrument?.PaymentInstrumentId : null,
                Partner = partner,
                Amount = checkoutRequestClientActions.Amount,
                Currency = checkoutRequestClientActions.Currency,
                Country = checkoutRequestClientActions.Country,
                Language = checkoutRequestClientActions.Language,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Five,
                HasPreOrder = checkoutRequestClientActions.PreOrder ?? false,
            };

            return paymentSessionData;
        }

        private static PaymentSessionData GetPaymentSessionData(string partner, PaymentRequestClientActions paymentRequestClientActions)
        {
            var clientAction = paymentRequestClientActions.ClientActions?.FirstOrDefault();
            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                PaymentInstrumentId = clientAction != null ? clientAction.PaymentInstrument?.PaymentInstrumentId : null,
                Partner = partner,
                Amount = paymentRequestClientActions.Amount,
                Currency = paymentRequestClientActions.Currency,
                Country = paymentRequestClientActions.Country,
                Language = paymentRequestClientActions.Language,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Five,
                HasPreOrder = paymentRequestClientActions.PreOrder ?? false,
            };

            return paymentSessionData;
        }

        //// TODO: Delete this function after cr and pr are merged
        private static bool HasThreeDs2Challenge(CheckoutRequestClientActions checkoutRequest)
        {
            if (checkoutRequest?.ClientActions == null)
            {
                return false;
            }

            return checkoutRequest.ClientActions.Any(action =>
                action.ActionType == Model.PaymentOrchestratorService.ClientActionType.HandleChallenge &&
                action.ChallengeType == PaymentInstrumentChallengeType.ThreeDs2);
        }

        private static bool IsPaymentInstrumentChallengeRequired(PaymentRequestClientActions paymentRequest)
        {
            if (paymentRequest?.ClientActions == null)
            {
                return false;
            }

            return paymentRequest.ClientActions.Any(action =>
                action.Type == Model.PaymentOrchestratorService.ClientActionType.HandleChallenge);
        }

        private async Task<string> PostPItoPIMS(PIDLData pi, string country, string partner, EventTraceActivity traceActivityId)
        {
            // Set payment instrument details
            var requestContext = this.GetRequestContext(traceActivityId);
            var metaData = ProxyController.GetMetaData(requestContext);
            var additionalProps = new Dictionary<string, object>() { { "UsageType", UsageType.Inline }, { "MetaData", metaData } };
            ProxyController.SetDetailsData(pi, additionalProps, traceActivityId);

            // Get query params
            var queryParams = this.Request.GetQueryNameValuePairs();
            string countryFromRequest = null;
            if (!this.Request.TryGetQueryParameterValue(V7.Constants.QueryParameterName.Country, out countryFromRequest))
            {
                queryParams = queryParams.Concat(new[] { new KeyValuePair<string, string>(V7.Constants.QueryParameterName.Country, country) });
            }

            // POST payment instrument
            PimsModel.V4.PaymentInstrument newPI = await this.Settings.PIMSAccessor.PostPaymentInstrument(pi, traceActivityId, queryParams, null, partner, this.ExposedFlightFeatures);

            return newPI?.PaymentInstrumentId;
        }
    }
}