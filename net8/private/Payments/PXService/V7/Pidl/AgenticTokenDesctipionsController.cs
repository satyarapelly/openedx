// <copyright file="AgenticTokenDesctipionsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    public class AgenticTokenDesctipionsController : ProxyController
    {
        /// <summary>
        /// Get Payment Token Descriptions
        /// </summary>
        /// <group>PaymentTokenDesctipions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/PaymentTokenDesctipions/</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="country" required="true" cref="string" in="path">two letter country id</param>
        /// <param name="type" required="true" cref="string" in="query">address type</param>
        /// <param name="operation" required="true" cref="string" in="query">operation namee</param>        
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="piid" required="false" cref="string" in="query">payment instrument id</param>
        /// <response code="200">A list of PIDLResource</response>
        /// <returns>A list of PIDLResource object</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        public List<PIDLResource> Get([FromRoute] string accountId, [FromRoute] string country, [FromQuery] string type, [FromQuery] string operation, [FromQuery] string? language = null, [FromQuery] string partner = Constants.ServiceDefaults.DefaultPartnerName, [FromQuery] string? piid = null)
        {
            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(operation);
            this.EnableFlightingsInPartnerSetting(setting, country);
            string action = "detect";
            PIDLData defaultValue = new PIDLData()
            {
                { Constants.PropertyDescriptionIds.PaymentMethodType, type }
            };

            if (!string.IsNullOrEmpty(piid))
            {
                action = "create";
                defaultValue.Add(Constants.PropertyDescriptionIds.PaymentInstrumentId, piid);
            }

            // If Piid is not present, the call is from CheckAgenticTokenEligibility PIDLSDK component, then action should be set as detect
            // If Piid is present, the call is from GetAgenticToken, then action should be set as create
            var tokenPidl = PIDLResourceFactory.Instance.GetPaymentTokenDescriptions(country, type, language, action, operation, partner, setting: setting, piid: piid);

            AgenticPaymentHelper.UpdateDefaultValuesFromPayload(tokenPidl, defaultValue);
            return tokenPidl;
        }
    }
}