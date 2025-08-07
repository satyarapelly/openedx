// <copyright file="SessionsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.PayerAuthService;
    using Microsoft.Commerce.Payments.PXService.Model.SessionService;
    using Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService;

    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController : ProxyController
    {
        /// <summary>
        /// Get session by id
        /// </summary>
        /// <group>Sessions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/Sessions</url>
        /// <param name="sessionId" required="true" cref="string" in="query">session dd</param>
        /// <response code="200">A session object</response>
        /// <returns>A session object</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        [Route("[action]")]
        public async Task<SecondScreenSessionData> GetBySessionId(string sessionId)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            SecondScreenSessionData sessionData = await this.Settings.SessionServiceAccessor.GetSessionResourceData<SecondScreenSessionData>(sessionId, traceActivityId);
            sessionData.AddToConfig("accountId", "anonymous");
            sessionData.AddToConfig("partner", Constants.PartnerName.Webblends);
            sessionData.AddToConfig(GlobalConstants.QueryParamNames.ClassicProduct, null);
            sessionData.AddToConfig(GlobalConstants.QueryParamNames.BillableAccountId, null);
            sessionData.AddToConfig("piid", null);

            this.Request.AddTracingProperties(sessionData.GetFromConfig("accountId"), null, sessionData.Family, sessionData.PaymentType, sessionData.Country);

            return sessionData;
        }

        /// <summary>
        /// POST session by id
        ///  ("/PostSessionById" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>Sessions</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/Sessions/PostSessionById</url>
        /// <param name="sessionId" required="true" cref="string" in="query">session dd</param>
        /// <response code="200">A session object</response>
        /// <returns>A session object</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpPost]
        [Route("[action]")]
        public string PostBySessionId(string sessionId)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            return sessionId;
        }

        /// <summary>
        ///  This function should be refactored to allow POSTs of different types of sessions
        /// </summary>
        /// <group>Sessions</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/Sessions</url>
        /// <param name="sessionTokenInfo" required="true" cref="object" in="body">This should be changed to a generic session object with a type property 
        /// </param>
        /// <response code="200">The created session resource</response>
        /// <returns>The created session resource</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<SessionResponse> PostSession([FromBody]SessionRequest sessionTokenInfo)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            if (sessionTokenInfo.ShopperId == "8e22c40d-9011-411c-a09c-c64921959f15") 
            {
                var sessionData = new PaymentSessionData();
                sessionData.PaymentInstrumentAccountId = sessionTokenInfo.ShopperId;
                sessionData.PaymentInstrumentId = "jxWigAAAAAABAACA";
                sessionData.Partner = "PX.COT";
                sessionData.Amount = 15.15m;
                sessionData.Currency = "EUR";
                sessionData.HasPreOrder = false;
                sessionData.IsLegacy = false;
                sessionData.IsMOTO = false;
                sessionData.ChallengeScenario = ChallengeScenario.PaymentTransaction;
                sessionData.PaymentMethodFamily = "credit_card";
                sessionData.DeviceChannel = DeviceChannel.AppBased;
                sessionData.Country = "us";
                var paymentSessionResponse = await this.Settings.PayerAuthServiceAccessor.CreatePaymentSessionId(sessionData, traceActivityId);
                return new SessionResponse() { Token = paymentSessionResponse.PaymentSessionId };
            }
            else if (sessionTokenInfo.ShopperId == "7e5242d0-33ea-4bd1-a691-5193af93c4c7")
            {
                var sessionData = new PaymentSessionData();
                sessionData.PaymentInstrumentAccountId = sessionTokenInfo.ShopperId;
                sessionData.PaymentInstrumentId = "d4bd8a76-1de1-4949-8e58-071789b8188f";
                sessionData.Partner = "PX.COT";
                sessionData.Amount = 15.15m;
                sessionData.Currency = "EUR";
                sessionData.HasPreOrder = false;
                sessionData.IsLegacy = false;
                sessionData.IsMOTO = false;
                sessionData.ChallengeScenario = ChallengeScenario.PaymentTransaction;
                sessionData.PaymentMethodFamily = "credit_card";
                sessionData.DeviceChannel = DeviceChannel.AppBased;
                sessionData.Country = "us";
                var paymentSessionResponse = await this.Settings.PayerAuthServiceAccessor.CreatePaymentSessionId(sessionData, traceActivityId);
                return new SessionResponse() { Token = paymentSessionResponse.PaymentSessionId };
            }

            var merchantId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.RetailServerInfo.MerchantId);
            var tokenSigningCreds = new IdentityModel.Tokens.X509SigningCredentials(this.Settings.SessionServiceClientCertificate);

            var token = new JwtSecurityToken(
                issuer: this.Settings.PXSessionTokenIssuer,
                audience: this.Settings.PifdBaseUrl.Substring(0, this.Settings.PifdBaseUrl.LastIndexOf('/')),
                claims: new List<Claim>()
                {
                    new Claim(Constants.SessionTokenClaimTypes.MerchantId, merchantId),
                    new Claim(Constants.SessionTokenClaimTypes.ShopperId, sessionTokenInfo.ShopperId)
                },
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(this.Settings.PXSessionTokenValidityPeriod),
                signingCredentials: tokenSigningCreds);

            var response = new SessionResponse()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };

            return response;
        }
    }
}