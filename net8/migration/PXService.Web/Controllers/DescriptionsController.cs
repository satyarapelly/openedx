// <copyright file="DescriptionsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/[controller]")]
    public class DescriptionsController : ProxyController
    {
        /// <summary>
        /// Returns a PIDL description for payment client.
        /// </summary>
        /// <group>PIDL Descriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/PaymentClient/Descriptions</url>
        /// <param name="component">Component name</param>
        /// <param name="partner">partner name</param>
        /// <param name="family">Payment method family</param>
        /// <param name="type">Payment method type</param>
        /// <param name="operation">operations [select/add/list</param>
        /// <param name="piid">Payment instrument Id</param>
        /// <param name="scenario">scenario name</param>
        /// <response code="200">List&lt;PIDLResource&gt;</response>
        /// <returns>Returns a PIDL for the given component</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        public async Task<HttpResponseMessage> Get(
            string component,
            string partner = V7.Constants.TemplateName.DefaultTemplate,
            string family = null,
            string type = null,
            string operation = null,
            string piid = null,
            string scenario = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            ComponentDescriptionFactory.ValidateRequiredParam(component, Constants.QueryParameterName.Component);
            ComponentDescriptionFactory.ValidateRequiredParam(partner, Constants.QueryParameterName.Partner);

            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(operation);

            // Extract request context - payment/checkout/wallet
            Contexts.RequestContext requestContext = this.GetRequestContext(traceActivityId);

            if (requestContext == null)
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, "Failed to get request context from the request headers"));
            }

            ComponentDescription componentInstance = null;

            // Create instance of the component
            if (this.UsePaymentRequestApiEnabled()) 
            {
                componentInstance = ComponentDescriptionFactory.CreateInstance(component, requestContext, setting, this.Generate3DS2ChallengePIDLResource);
            }
            else
            {
                componentInstance = ComponentDescriptionFactory.CreateInstance(component, requestContext, this.Generate3DS2ChallengePIDLResource);
            }

            // This block will be removed after PSS setting created for candy crush partner.
            if (setting == null && (partner?.Equals(V7.Constants.PartnerName.CandyCrush, System.StringComparison.OrdinalIgnoreCase) ?? false))
            {
                partner = V7.Constants.TemplateName.DefaultTemplate;
            }

            // Set description property to helps generate PIDL
            await componentInstance.LoadComponentDescription(requestContext?.RequestId, this.Settings, traceActivityId, setting, this.ExposedFlightFeatures, operation: operation, partner: partner, family: family, type: type, scenario: scenario, request: this.Request.ToHttpRequestMessage(), piid: piid);
            this.EnableFlightingsInPartnerSetting(componentInstance.PSSSetting, componentInstance.Country);

            // Component description Generation
            List<PIDLResource> retVal = await componentInstance.GetDescription();

            if (type == null)
            {
                type = componentInstance.DescriptionType;
            }

            // Post Porcess
            FeatureContext featureContext = new FeatureContext(
                    componentInstance.Country,
                    GetSettingTemplate(partner, componentInstance.PSSSetting, componentInstance.DescriptionType),
                    componentInstance.DescriptionType,
                    operation,
                    scenario,
                    componentInstance.Language,
                    null,
                    this.ExposedFlightFeatures,
                    componentInstance.PSSSetting?.Features,
                    family,
                    type,
                    originalPartner: partner,
                    isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request),
                    defaultPaymentMethod: null,
                    xmsFlightHeader: this.GetPartnerXMSFlightExposed(),
                    tokenizationPublicKey: await this.GetTokenizationPublicKey(traceActivityId),
                    tokenizationServiceUrls: this.Settings.TokenizationServiceAccessor.GetTokenizationServiceUrls());

            PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

            return this.Request.CreateResponse(HttpStatusCode.OK, retVal);
        }
    }
}