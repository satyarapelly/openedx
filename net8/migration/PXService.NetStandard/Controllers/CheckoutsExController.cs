// <copyright file="CheckoutsExController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.Checkouts
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentThirdPartyService;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Newtonsoft.Json;
    using Constants = Constants;

    public class CheckoutsExController : ProxyController
    {
        private const string PostMessageHtmlTemplate = "<html><script>window.parent.postMessage(\"{0}\", \"*\");</script><body/></html>";
        private const string FullPageRedirect = "<html><body onload=\"window.location.href = '{0}'\"></body></html>";

        /// <summary>
        /// iFrame for Stripe. Upon challenge completion iFrame posts message to parent window.
        /// Full page redirection for Paypal. Paypal doesn't support iFrame as of this writing
        /// (/Completed is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>        
        /// <group>CheckoutsEx</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/CheckoutsEx/Completed</url>
        /// <param name="redirectUrl">redirect url</param>
        /// <param name="checkoutId">checkout id</param>
        /// <param name="providerId">provider Id</param>
        /// <response code="200">Returns the HttpResponse to iFrame that posts message to parent window to redirect the page</response>
        /// <returns>Returns the HttpResponse to iFrame that posts message to parent window to redirect the page</returns>
        [HttpGet]
        public HttpResponseMessage Completed(
            [FromUri] string redirectUrl,
            [FromUri] string checkoutId,
            [FromUri] string providerId)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            if (string.IsNullOrEmpty(redirectUrl))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "Required param missing.", "redirectUrl missing")));
            }

            RedirectionServiceLink redirectLink = new RedirectionServiceLink { BaseUrl = redirectUrl };

            ClientAction clientAction = new ClientAction(ClientActionType.Redirect)
            {
                Context = redirectLink
            };

            string responseContent = string.Empty;

            if (!string.IsNullOrEmpty(providerId) && providerId.Equals(Constants.PaymentProviderIds.PayPal, System.StringComparison.OrdinalIgnoreCase))
            {
                responseContent = string.Format(FullPageRedirect, redirectUrl);
            }
            else if (!string.IsNullOrEmpty(providerId) && providerId.Equals(Constants.PaymentProviderIds.Stripe, System.StringComparison.OrdinalIgnoreCase))
            {
                clientAction.ActionId = checkoutId;
                string jsEncodedClientAction = HttpUtility.JavaScriptStringEncode(JsonConvert.SerializeObject(clientAction));
                responseContent = string.Format(PostMessageHtmlTemplate, jsEncodedClientAction);
            }

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(responseContent);
            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");
            return response;
        }

        /// <summary>
        /// Checkout charge
        /// </summary>        
        /// <group>CheckoutsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/CheckoutsEx</url>
        /// <param name="paymentProviderId">payment provider Id</param>
        /// <param name="checkoutId">checkout Id</param>
        /// <param name="partner">the name of partner</param>
        /// <param name="redirectUrl">once the checkout is complete, redirect back to the partner's redirectUrl</param>
        /// <param name="checkoutChargePayload">Checkout charge payload as provided by the user</param>
        /// <param name="language">language code</param>
        /// <response code="200">Returns the pidl redirect</response>
        /// <returns>Returns the pidl redirect</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> Charge(
            [FromUri] string paymentProviderId,
            [FromUri] string checkoutId,
            [FromUri] string partner,
            [FromUri] string redirectUrl,
            [FromBody] CheckoutChargePayload checkoutChargePayload,
            [FromUri] string language = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            string cV = this.Request.GetCorrelationVector().ToString();

            string pifdEndpointUrlOnChallengeComplete = $"{this.PidlBaseUrl}/checkoutsEx/{checkoutId}/completed?redirectUrl={redirectUrl}&providerId={paymentProviderId}";
            
            if (checkoutChargePayload == null)
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "InvalidCheckoutData", "The input checkout data is invalid.")));
            }

            if (string.IsNullOrEmpty(checkoutId) || string.IsNullOrEmpty(partner))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "RequiredParametersMissing", "checkoutId and partner are required.")));
            }

            // ReturnUrl is solely for PSD2 challenge flow
            // the url to return-to once the challenge is complete
            var checkoutCharge = new CheckoutCharge()
            {
                CheckoutId = checkoutId,
                PaymentInstrument = new PaymentInstrument
                {
                    PaymentMethodFamily = checkoutChargePayload.PaymentMethodFamily,
                    PaymentMethodType = checkoutChargePayload.PaymentMethodType,
                    Context = checkoutChargePayload.Context,
                    PaymentInstrumentDetails = checkoutChargePayload.PaymentInstrumentDetails,
                },
                ReturnUrl = pifdEndpointUrlOnChallengeComplete,
                ReceiptEmailAddress = checkoutChargePayload.ReceiptEmailAddress,
                FailureUrl = $"{this.Settings.PayMicrosoftBaseUrl}/error?language=" + (string.IsNullOrEmpty(language) ? GlobalConstants.Defaults.Locale : language)
            };

            try
            {
                Checkout checkout = await this.Settings.PaymentThirdPartyServiceAccessor.GetCheckout(paymentProviderId, checkoutId, traceActivityId);
                
                if (checkout.Status != CheckoutStatus.Invalid && checkout.Status != CheckoutStatus.Paid)
                {
                    checkout = await this.Settings.PaymentThirdPartyServiceAccessor.Charge(paymentProviderId, checkoutCharge, traceActivityId);

                    // If checkout response object has RedirectUrl then it indicates PSD2 flow for stripe
                    // checkout.RedirectUrl from downstream service is the provider_psd2_url
                    // RedirectUrl will be loaded in iFrame for stripe
                    if (!string.IsNullOrEmpty(checkout.RedirectUrl))
                    {
                        // RedirectUrl drives checkout and PSD2 flows for Paypal and is a full page redirect
                        if (paymentProviderId.Equals(Constants.PaymentProviderIds.PayPal, System.StringComparison.OrdinalIgnoreCase))
                        {
                            return this.Request.CreateResponse(HttpStatusCode.OK, PIDLResourceFactory.GetRedirectPidl(checkout.RedirectUrl, true));
                        }
                        else
                        {
                            List<PIDLResource> resourceList = PIDLResourceFactory.GetChallengeRedirectAndStatusCheckDescriptionForCheckout(checkoutId, partner, paymentProviderId, checkout.RedirectUrl, redirectUrl);
                            ClientAction clientAction = new ClientAction(ClientActionType.Pidl, resourceList);
                            return this.Request.CreateResponse(HttpStatusCode.OK, new PIDLResource { ClientAction = clientAction });
                        }
                    }
                }
            }
            catch (ServiceErrorResponseException ex)
            {
                if (ex != null)
                {
                    if (string.Equals(ex.Error.ErrorCode, Constants.ThirdPartyPaymentsErrorCodes.CvvValueMismatch, System.StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.ThirdPartyPaymentsErrorMessages.CvvValueMismatch, GlobalConstants.Defaults.Locale);
                    }
                    else if (string.Equals(ex.Error.ErrorCode, Constants.ThirdPartyPaymentsErrorCodes.ExpiredPaymentInstrument, System.StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.ThirdPartyPaymentsErrorMessages.ExpiredPaymentInstrument, GlobalConstants.Defaults.Locale);
                    }
                    else if (string.Equals(ex.Error.ErrorCode, Constants.ThirdPartyPaymentsErrorCodes.InvalidPaymentInstrument, System.StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.ThirdPartyPaymentsErrorMessages.InvalidPaymentInstrument, GlobalConstants.Defaults.Locale);
                    }
                    else
                    {
                        List<PIDLResource> checkoutErrorPidl = PIDLResourceFactory.Instance.GetStaticCheckoutErrorDescriptions(language, redirectUrl, Constants.PartnerName.MSTeams);
                        var errorSubText = checkoutErrorPidl[0].GetDisplayHintById("paymentErrorSubText") as ContentDisplayHint;
                        var errorCV = checkoutErrorPidl[0].GetDisplayHintById("paymentErrorCV") as ContentDisplayHint;
                        errorCV.DisplayContent = string.Format(errorCV.DisplayContent, cV);
                        if (Constants.ThirdPartyPaymentTerminalErrorsTypeOne.Contains(ex.Error.ErrorCode))
                        {
                            errorSubText.DisplayContent = string.Format(errorSubText.DisplayContent, LocalizationRepository.Instance.GetLocalizedString(Constants.ThirdPartyPaymentsErrorMessages.TryAgainMsg, language));
                        }
                        else if (Constants.ThirdPartyPaymentTerminalErrorsTypeTwo.Contains(ex.Error.ErrorCode))
                        {
                            errorSubText.DisplayContent = string.Format(errorSubText.DisplayContent, LocalizationRepository.Instance.GetLocalizedString(Constants.ThirdPartyPaymentsErrorMessages.PaymentMethodErrorMsg, language));
                        }

                        ClientAction clientAction = new ClientAction(ClientActionType.Pidl, checkoutErrorPidl);
                        return this.Request.CreateResponse(HttpStatusCode.OK, new PIDLResource { ClientAction = clientAction });
                    }

                    return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error, "application/json");
                }
            }
            
            return this.Request.CreateResponse(HttpStatusCode.OK, PIDLResourceFactory.GetRedirectPidl(redirectUrl, true));
        }

        /// <summary>
        /// PSD2 flow checkout status
        /// </summary>        
        /// <group>CheckoutsEx</group>        
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/CheckoutsEx</url>
        /// <param name="paymentProviderId">payment provider Id</param>
        /// <param name="checkoutId">checkout Id</param>
        /// <response code="200">Returns the status pidl</response>
        /// <returns>Returns the status pidl</returns>
        [HttpGet]
        public async Task<HttpResponseMessage> Status(
            [FromUri] string paymentProviderId,
            [FromUri] string checkoutId)
        {            
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            if (string.IsNullOrEmpty(checkoutId) || string.IsNullOrEmpty(paymentProviderId))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "RequiredParametersMissing", "checkoutId and paymentProviderId are required.")));
            }

            Checkout checkout = null;
            try
            {
               checkout = await this.Settings.PaymentThirdPartyServiceAccessor.GetCheckout(paymentProviderId, checkoutId, traceActivityId);
            }
            catch (ServiceErrorResponseException ex)
            {
                if (ex != null)
                {
                    ex.Error = SetServiceErrorResponseMessage(ex.Error);
                    return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error, "application/json");
                }
            }
            
            return this.Request.CreateResponse(HttpStatusCode.OK, new CheckoutStatusResponse { CheckoutStatus = checkout.Status.ToString() });
        }

        /// <summary>
        /// Sets ServiceError Response message based on the Error code
        /// </summary>
        /// <param name="response">ServiceErrorResponse oject</param>
        /// <returns>Returns ServiceErrorResponse</returns>
        private static ServiceErrorResponse SetServiceErrorResponseMessage(ServiceErrorResponse response)
        {
            if (Constants.ThirdPartyPaymentErrorMsgs.Contains(response.ErrorCode))
            {
                string val;
                Constants.ThirdPartyPaymentErrorMsgs.TryGetValue(response.ErrorCode, out val);
                response.Message = val;
            }

            return response;
        }
    }
}