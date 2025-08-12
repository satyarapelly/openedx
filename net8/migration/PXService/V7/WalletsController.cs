// <copyright file="WalletsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using Common.Web;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.PXInternal;
    using Microsoft.Commerce.Payments.PXService.Model.WalletService;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Tracing;
    using static Microsoft.Commerce.Payments.PXService.V7.Constants;

    [ApiController]
    [Route("api/[controller]")]
    public class WalletsController : ProxyController
    {
        /// <summary>
        /// Gets the wallet configurations to intialize payment agent.
        /// </summary>
        /// <group>Wallets</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/getWalletConfig</url>
        /// <param name="partner" required="false" cref="string" in="query">Partner requesting config</param>
        /// <param name="client" required="false" cref="string" in="query">Stringified object of client data</param>
        /// <response code="200">A json configuration</response>
        /// <returns>Json configuration</returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<HttpResponseMessage> GetWalletConfig(
            string partner = null,
            string client = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            ProviderDataResponse response = await this.Settings.WalletServiceAccessor.GetProviderData(traceActivityId, this.ExposedFlightFeatures);

            this.Request.AddPartnerProperty(partner?.ToLower());
            try
            {
                // get piid from PIMS
                var additionalHeaders = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(GlobalConstants.HeaderValues.ExtendedFlightName, "vnext")
                };

                var paymentMethods = await this.Settings.PIMSAccessor.GetPaymentMethods(null, Constants.PaymentMethodFamily.ewallet.ToString(), null, null, traceActivityId, additionalHeaders: additionalHeaders);

                var singleMarkets = await this.Settings.CatalogServiceAccessor.GetSingleMarkets(traceActivityId);

                if (singleMarkets == null)
                {
                    // FallBack to default markets if singleMarkets is null
                    singleMarkets = new List<string>(PIDLResourceFactory.GetCopiedDictionaryFromDomainDictionaries("MarketsEUSMD").Keys);
                }

                if (singleMarkets != null && singleMarkets.Count > 0)
                {
                    singleMarkets = singleMarkets.ConvertAll(market => market.ToUpper());
                }

                HttpResponseMessage walletConfigResponse = Request.CreateResponse(
                    HttpStatusCode.OK,
                    WalletsHandler.AdaptWalletResponseToPIDLConfig(
                        response,
                        client,
                        partner,
                        this.Request.ToHttpRequestMessage(),
                        this.ExposedFlightFeatures,
                        traceActivityId,
                        paymentMethods,
                        isExpressCheckout: false,
                        singleMarkets: singleMarkets));
                return walletConfigResponse;
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(ex.Message, traceActivityId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, string.Format("Error in fetching piid from PIMS", ex));
            }
        }

        /// <summary>
        /// Setup wallet provider session
        /// </summary>        
        /// <group>Wallets</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/setupWalletProviderSession</url>
        /// <param name="payload">wallet provider session payload</param>
        /// <response code="200">Returns the wallet session object</response>
        /// <returns>Returns wallet session object</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<string> SetupWalletProviderSession(
            [FromBody] SetupProviderSessionIncomingPayload payload)
        {
            // call Wallet service to setup session
            // Pass on the object received as is
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            string sessionData = await this.Settings.WalletServiceAccessor.SetupProviderSession(payload, traceActivityId);
            return sessionData;
        }

        /// <summary>
        /// Provision wallet token
        /// </summary>        
        /// <group>Wallets</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/provisionWalletToken/validatePayload</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="payload">Payload of the user.</param>
        /// <response code="200">Returns the session id</response>
        /// <returns>Returns session id</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<HttpResponseMessage> ProvisionWalletToken(
            string accountId,
            [FromBody] ProvisionWalletTokenIncomingPayload payload)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            var paymentSession = new PaymentChallenge.Model.PaymentSession(payload.SessionData);

            // call PIMS update PI for instance PI flow
            if (IsInstancePI(payload.SessionData.PaymentInstrumentId))
            {
                var paymentTokenHandler = ExternalPaymentTokenTransformerFactory.Instance(payload.PiType, payload.PaymentData, traceActivityId);
                PIDLData pi = paymentTokenHandler.ExtractPaymentInstrument(AttachmentType.Wallet);
                var queryParams = this.GetQueryParamsUpdatePI(paymentSession.Country, paymentSession.Partner);

                await this.Settings.PIMSAccessor.UpdatePaymentInstrument(accountId, paymentSession.PaymentInstrumentId, pi, traceActivityId, paymentSession.Partner, this.ExposedFlightFeatures, queryParams);
                HttpResponseMessage instancePiFlowResponse = Request.CreateResponse(HttpStatusCode.OK, payload.SessionData);
                return instancePiFlowResponse;
            }

            // call new session service for the session id to be passed to Wallet service
            // Call wallet service to provision token iwth the token reference from body
            // return the session id
            var dataId = await this.Settings.TransactionDataServiceAccessor.GenerateDataId(traceActivityId);
            var provisionWalletResponse = await this.Settings.WalletServiceAccessor.Provision(dataId, accountId, payload, traceActivityId);

            // Challenge required is always false for US market
            // To determine challenge required for other markets: https://microsoft.sharepoint.com/teams/PaymentExperience/_layouts/OneNote.aspx?id=%2Fteams%2FPaymentExperience%2FSiteAssets%2FPayment%20Experience&wd=target%28Projects%2FApple%20google%20pay.one%7CE6DD02DC-B3A5-44CD-8F03-DF9EF9DEC219%2FMarket%20Expansion%7C7A9A574A-BFCC-4198-A068-82D8BA233C5D%2F%29
            // onenote: https://microsoft.sharepoint.com/teams/PaymentExperience/SiteAssets/Payment%20Experience/Projects/Apple%20google%20pay.one#Market%20Expansion&section-id={E6DD02DC-B3A5-44CD-8F03-DF9EF9DEC219}&page-id={7A9A574A-BFCC-4198-A068-82D8BA233C5D}&end
            var isChallengeRequired = false;
            paymentSession.Id = dataId;
            paymentSession.IsChallengeRequired = isChallengeRequired;
            paymentSession.ChallengeStatus = PaymentChallengeStatus.NotApplicable;
            paymentSession.Signature = paymentSession.GenerateSignature();

            // Validate data in Wallet service 
            // Only call validate PI when transaction amount = 0 for now (later may be extended to preorder), since rest of flows purchase FD will call risk for risk check and billing for charge/auth.
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableValidateAPIForGPAP) && paymentSession.Amount == 0)
            {
                var validatePayload = new ValidateIncomingPayload
                {
                    PiFamily = payload.PiFamily,
                    PiType = payload.PiType,
                    SessionData = paymentSession,
                    TokenReference = payload.TokenReference
                };
                var successValidate = await this.Settings.WalletServiceAccessor.Validate(dataId, accountId, validatePayload, traceActivityId);
                if (!successValidate.Result.Equals(WalletValidationStatusConstants.Approved))
                {
                    paymentSession.ChallengeStatus = PaymentChallengeStatus.Failed;
                }
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, paymentSession);
            return response;
        }

        private static bool IsInstancePI(string piid)
        {
            return !string.IsNullOrEmpty(piid)
                && (piid.StartsWith(Constants.WalletServiceConstants.GooglePayPiidPrefix)
                || piid.StartsWith(Constants.WalletServiceConstants.ApplePayPiidPrefix));
        }

        private IEnumerable<KeyValuePair<string, string>> GetQueryParamsUpdatePI(string country, string partner)
        {
            var queryParams = this.Request.Query.AsEnumerable().Select(q => new KeyValuePair<string, string>(q.Key, q.Value));
            if (!this.Request.Query.TryGetValue(V7.Constants.QueryParameterName.Country, out _))
            {
                queryParams = queryParams.Concat(new[] { new KeyValuePair<string, string>(V7.Constants.QueryParameterName.Country, country) });
            }

            if (!this.Request.Query.TryGetValue(V7.Constants.QueryParameterName.Partner, out _))
            {
                queryParams = queryParams.Concat(new[] { new KeyValuePair<string, string>(V7.Constants.QueryParameterName.Partner, partner) });
            }

            return queryParams;
        }
    }
}