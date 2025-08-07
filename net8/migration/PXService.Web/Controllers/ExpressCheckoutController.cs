// <copyright file="ExpressCheckoutController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Common.Web;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using PaymentInstrument = PimsModel.V4.PaymentInstrument;

    [ApiController]
    [Route("api/[controller]")]
    public class ExpressCheckoutController : ProxyController
    {
        /// <summary>
        /// Express checkout confirm
        /// </summary>
        /// <group>ExpressCheckout</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/expressCheckout/confirm</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="confirmPayload" required="true" cref="object" in="body">Google pay/Apple pay payload</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <response code="200">ExpressCheckoutResult object</response>
        /// <returns>ExpressCheckoutResult object</returns>
        [HttpPost]
        public async Task<ActionResult<ExpressCheckoutResult>> Confirm(
            string accountId,
            [FromBody] PIDLData confirmPayload,
            string partner = V7.Constants.ServiceDefaults.DefaultPartnerName)
        {
            EventTraceActivity traceActivityId;

            if (Guid.TryParse(HttpContext.TraceIdentifier, out Guid guid))
            {
                traceActivityId = new EventTraceActivity(guid);
            }
            else
            {
                traceActivityId = new EventTraceActivity(Guid.NewGuid()); // fallback
            }

            string paymentMethodType = confirmPayload?.TryGetPropertyValueFromPIDLData(V7.Constants.ExpressCheckoutPropertyValue.PaymentMethodType);

            if (string.IsNullOrEmpty(paymentMethodType)
                || !(paymentMethodType.Equals(V7.Constants.PaymentMethodType.ApplePay, StringComparison.OrdinalIgnoreCase)
                    || paymentMethodType.Equals(V7.Constants.PaymentMethodType.GooglePay, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("Invalid Payment Method Type");
            }

            // Extract apple pay or google pay token as string from payload.
            string expressCheckoutData = confirmPayload?.TryGetPropertyValueFromPIDLData(V7.Constants.ExpressCheckoutPropertyValue.ExpressCheckoutPaymentData);
            string country = confirmPayload?.TryGetPropertyValueFromPIDLData(V7.Constants.ExpressCheckoutPropertyValue.Country);

            // Transform payment token into appropriate payment type data.
            var paymentTokenHandler = ExternalPaymentTokenTransformerFactory.Instance(paymentMethodType, expressCheckoutData, traceActivityId);

            // Get addressInfoV3 from expressCheckoutData create an address in Jarvis
            AddressInfoV3 addressV3 = paymentTokenHandler.ExtractAddressInfoV3();
            AddressInfoV3 addressToReturn = null;
            try
            {
                addressToReturn = await this.Settings.AccountServiceAccessor.PostAddress(accountId, addressV3, GlobalConstants.AccountServiceApiVersion.V3, traceActivityId);
            }
            catch (Exception ex)
            {
                var expressCheckoutException = GenerateExpressCheckoutException(Constants.ExpressCheckoutErrorCodes.InvalidAddress, "Invalid address: " + ex.ToString(), PXCommon.Constants.ServiceNames.AccountService, traceActivityId);
                throw TraceCore.TraceException(traceActivityId, expressCheckoutException);
            }

            // Check if first/last/emails are available in the profile, if not, update the profile by using fisrt/last/emails from the payload
            try
            {
                var updateResult = await this.UpdateProfileIfNeeded(accountId, addressV3, traceActivityId);
                if (!updateResult.Success)
                {
                    return BadRequest(updateResult.Message);
                }
            }
            catch (Exception ex)
            {
                var expresscheckoutException = GenerateExpressCheckoutException(Constants.ExpressCheckoutErrorCodes.InvalidProfile, "Invalid profile: " + ex.ToString(), PXCommon.Constants.ServiceNames.AccountService, traceActivityId);
                throw TraceCore.TraceException(traceActivityId, expresscheckoutException);
            }

            // Extract PI from payload and post pi to PIMS
            PIDLData pi = paymentTokenHandler.ExtractPaymentInstrument(AttachmentType.Wallet);

            var queryParams = this.GenerateQueryParamsForPostPi(country);
            PaymentInstrument newPI = null;
            try
            {
                newPI = await this.Settings.PIMSAccessor.PostPaymentInstrument(accountId, pi, traceActivityId, queryParams, null, partner, this.ExposedFlightFeatures);
            }
            catch (Exception ex)
            {
                var expressCheckoutException = GenerateExpressCheckoutException(Constants.ExpressCheckoutErrorCodes.InvalidPaymentInstrument, "Invalid payment instrument: " + ex.ToString(), PXCommon.Constants.ServiceNames.InstrumentManagementService, traceActivityId);
                throw TraceCore.TraceException(traceActivityId, expressCheckoutException);
            }

            ExpressCheckoutResult result = new ExpressCheckoutResult
            {
                Pi = newPI,
                BillingAddress = addressToReturn
            };

            return result;
        }

        private static ExpressCheckoutException GenerateExpressCheckoutException(string errorCode, string message, string source, EventTraceActivity traceActivityId)
        {
            var serviceErrorResponse = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), message)
            {
                ErrorCode = errorCode,
                Source = source,
            };

            var expressCheckoutException = new ExpressCheckoutException(serviceErrorResponse);
            return TraceCore.TraceException(traceActivityId, expressCheckoutException);
        }

        private IEnumerable<KeyValuePair<string, string>> GenerateQueryParamsForPostPi(string country)
        {
            var queryParams = this.Request.Query.AsEnumerable().Select(q => new KeyValuePair<string, string>(q.Key, q.Value));
            if (!this.Request.Query.TryGetValue(V7.Constants.QueryParameterName.Country, out _))
            {
                queryParams = queryParams.Concat(new[] { new KeyValuePair<string, string>(V7.Constants.QueryParameterName.Country, country) });
            }

            return queryParams;
        }

        private async Task<(bool Success, string Message)> UpdateProfileIfNeeded(string accountId, AddressInfoV3 addressV3, EventTraceActivity traceActivityId)
        {
            var profileType = this.GetProfileType();
            var profile = await this.Settings.AccountServiceAccessor.GetProfileV3(accountId, profileType, traceActivityId);

            if (profile == null)
            {
                return (false, "Invalid Profile / Profile not found");
            }

            if (profileType == GlobalConstants.ProfileTypes.Consumer)
            {
                AccountConsumerProfileV3 accountConsumerProfileV3 = profile as AccountConsumerProfileV3;
                if (accountConsumerProfileV3.FirstName == null || accountConsumerProfileV3.LastName == null || accountConsumerProfileV3.EmailAddress == null)
                {
                    accountConsumerProfileV3.FirstName = accountConsumerProfileV3.FirstName ?? addressV3.FirstName;
                    accountConsumerProfileV3.LastName = accountConsumerProfileV3.LastName ?? addressV3.LastName;
                    accountConsumerProfileV3.EmailAddress = accountConsumerProfileV3.EmailAddress ?? addressV3.EmailAddress;
                    await this.Settings.AccountServiceAccessor.UpdateProfileV3(accountId, accountConsumerProfileV3, GlobalConstants.ProfileTypes.Consumer, traceActivityId, this.ExposedFlightFeatures, true, true);
                }
            }
            else
            {
                return (false, "profile type " + profileType + " is not supported");
            }

            return (true, null);
        }
    }
}