// <copyright file="RDSSessionController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Common.Tracing;
    using Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Tracing;

    public class RDSSessionController : ProxyController
    {
        /// <summary>
        /// Post RDSSession
        /// </summary>
        /// <group>RDSSession</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/RDSSession</url>
        /// <param name="sessionDetails" required="true" cref="object" in="body">session details</param>
        /// <param name="sessionId" required="false" cref="string" in="query">session dd</param>
        /// <param name="piid" required="false" cref="string" in="query">payment instrument id</param>
        /// <param name="partner" required="false" cref="string" in="query">partner name</param>
        /// <param name="language" required="false" cref="string" in="query">language code</param>
        /// <param name="country" required="false" cref="string" in="query">country code</param>
        /// <param name="scenario" required="false" cref="string" in="query">scenario name</param>
        /// <response code="200">A rds Status Pidl</response>
        /// <returns>A rds Status Pidl</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> Query([FromBody]PIDLData sessionDetails, [FromUri]string sessionId = null, [FromUri] string piid = null, [FromUri] string partner = null, [FromUri] string language = null, [FromUri] string country = null, [FromUri] string scenario = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            PaymentsEventSource.Log.InstrumentManagementServiceTraceRequest(GlobalConstants.APINames.RDSSessionQuery, this.Request.RequestUri.AbsolutePath, traceActivityId);

            try
            {
                string rdsSessionState = await this.Settings.RDSServiceAccessor.GetRDSSessionState(sessionId, traceActivityId);
                string paymentSessionId = sessionDetails?.TryGetPropertyValue("paymentSessionId");
                string paymentMethodFamilyTypeId = sessionDetails?.TryGetPropertyValue("paymentMethodFamilyTypeId");

                // Use Partner Settings if enabled for the partner
                PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(V7.Constants.Component.HandlePaymentChallenge);

                if (!string.IsNullOrEmpty(paymentSessionId))
                {
                    PIDLResource rdsStatusPidl = new PIDLResource();
                    ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
                    if (string.Equals(rdsSessionState, "success", StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(rdsSessionState, "failure", StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(scenario, Constants.ScenarioNames.PollingAction, StringComparison.OrdinalIgnoreCase))
                    {
                        PaymentSession paymentSession = await this.PaymentSessionsHandler.TryGetPaymentSession(paymentSessionId, traceActivityId);
                        if (paymentSession == null)
                        {
                            paymentSession = new PaymentSession { Id = paymentSessionId, IsChallengeRequired = true };
                        }

                        if (string.Equals(rdsSessionState, "success", StringComparison.InvariantCultureIgnoreCase))
                        {
                            paymentSession.ChallengeStatus = PaymentChallengeStatus.Succeeded;
                            clientAction = new ClientAction(ClientActionType.ReturnContext);
                            clientAction.Context = paymentSession;
                        }
                        else if (string.Equals(scenario, Constants.ScenarioNames.PollingAction, StringComparison.OrdinalIgnoreCase))
                        {
                            paymentSession.ChallengeStatus = PaymentChallengeStatus.Unknown;
                            clientAction = new ClientAction(ClientActionType.ReturnContext);
                            clientAction.Context = paymentSession;
                        }
                        else
                        {
                            paymentSession.ChallengeStatus = PaymentChallengeStatus.Failed;
                            clientAction = new ClientAction(ClientActionType.Failure);
                            clientAction.Context = paymentSession;
                        }
                    }
                    else
                    {
                        clientAction = new ClientAction(ClientActionType.Pidl);
                        string resourceType = Constants.StaticDescriptionTypes.Cc3DSStatusCheckPidl;

                        if (!string.IsNullOrEmpty(paymentMethodFamilyTypeId) && string.Equals(paymentMethodFamilyTypeId, PidlFactory.GlobalConstants.PaymentMethodFamilyTypeIds.EwalletLegacyBilldeskPayment))
                        {
                            resourceType = Constants.StaticDescriptionTypes.LegacyBillDesk3DSStatusCheckPidl;
                        }

                        List<PIDLResource> redirectPidls = PIDLResourceFactory.Instance.Get3DSStatusCheckDescriptionForPaymentAuth(sessionId, paymentSessionId, partner, language, country, resourceType, null, paymentMethodFamilyTypeId, setting: setting);

                        clientAction.Context = redirectPidls;
                    }

                    rdsStatusPidl.ClientAction = clientAction;
                    return this.Request.CreateResponse(HttpStatusCode.OK, rdsStatusPidl);
                }

                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new ErrorMessage() { ErrorCode = V7.Constants.PSD2ErrorCodes.InvalidPaymentSession, Message = "paymentSessionId is missing" });
            }
            catch (ServiceErrorResponseException ex)
            {
                return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error);
            }
        }
    }
}
