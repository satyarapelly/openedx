// <copyright file="TenantDescriptionsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    [ApiController]
    [Route("api/[controller]")]
    public class TenantDescriptionsController : ControllerBase
    {
        /// <summary>
        /// Get Tenant Descriptions 
        /// </summary>
        /// <group>TenantDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/TenantDescriptions</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="type" required="true" cref="string" in="query">type name</param>
        /// <param name="country" required="true" cref="string" in="path">two letter country id</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <response code="200">A list of PIDLResource</response>
        /// <returns>A list of PIDLResource object</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        [Route("[action]")]
        public List<PIDLResource> Get(string accountId, string type, string country, string language = null, string partner = Constants.ServiceDefaults.DefaultPartnerName)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            accountId = accountId + string.Empty;
            return PIDLResourceFactory.Instance.GetTenantDescription(type, country, language, partner);
        }
    }
}
