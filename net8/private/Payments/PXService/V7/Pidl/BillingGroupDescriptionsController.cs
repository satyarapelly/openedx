// <copyright file="BillingGroupDescriptionsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    public class BillingGroupDescriptionsController : ProxyController
    {
        /// <summary>
        /// Get billing groups
        /// </summary>
        /// <group>BillingGroupDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/BillingGroupDescriptions</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="country" required="true" cref="string" in="query">Two letter country code</param>
        /// <param name="operation" required="false" cref="string" in="query">operation name</param>
        /// <param name="type" required="false" cref="string" in="query">address type</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="scenario" required="false" cref="object" in="body">scenario name</param>
        /// <response code="200">List&lt;PIDLResource&gt; for BillGroupDescriptions</response>
        /// <returns>A list of PIDLResource object</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        public List<PIDLResource> GetBillingGroupsDescription(
            [FromRoute] string accountId,
            [FromQuery] string country,
            [FromQuery] string operation = Constants.Operations.SelectInstance,
            [FromQuery] string? type = null,
            [FromQuery] string? language = null,
            [FromQuery] string partner = Constants.ServiceDefaults.DefaultPartnerName,
            [FromQuery] string? scenario = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(operation);

            if (string.Equals(operation, Constants.Operations.SelectInstance, StringComparison.InvariantCultureIgnoreCase))
            {
                // Client side prefill for list billing group
                List<PIDLResource> retVal = PIDLResourceFactory.GetBillingGroupListDescriptions(type, operation, country, language, partner, this.ExposedFlightFeatures, setting);
                return retVal;
            }
            else if (string.Equals(operation, Constants.Operations.Add, StringComparison.InvariantCultureIgnoreCase) || string.Equals(operation, Constants.Operations.Update, StringComparison.InvariantCultureIgnoreCase))
            {
                return PIDLResourceFactory.Instance.GetBillingGroupDescriptions(type, operation, country, language, partner, scenario, setting);
            }

            throw new InvalidOperationException("Parameter operation is not in the list of supported operation types");
        }
    }
}
