// <copyright file="AddressEnrichmentServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.AddressEnrichmentService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Accessors.AddressEnrichmentService.DataModel;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Tracing;
    using Newtonsoft.Json;
    using static Microsoft.Commerce.Payments.PXService.GlobalConstants;

    public class AddressEnrichmentServiceAccessor : IAddressEnrichmentServiceAccessor
    {
        private readonly List<string> passThroughHeaders = new List<string> { PaymentConstants.PaymentExtendedHttpHeaders.TestHeader };

        private HttpClient addressEnrichmentServiceHttpClient;
        private string addressEnrichmentBaseUrl;
        private string emulatorBaseUrl;

        public AddressEnrichmentServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            HttpMessageHandler messageHandler)
        {
            this.addressEnrichmentBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;

            this.addressEnrichmentServiceHttpClient = new PXTracingHttpClient(PXCommon.Constants.ServiceNames.AddressEnrichmentService, messageHandler);
            this.addressEnrichmentServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.addressEnrichmentServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.addressEnrichmentServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
        }

        private string AddressEnrichmentBaseUrl
        {
            get
            {
                if (HttpRequestHelper.IsPXTestRequest() && !string.IsNullOrWhiteSpace(this.emulatorBaseUrl))
                {
                    return this.emulatorBaseUrl;
                }
                else
                {
                    return this.addressEnrichmentBaseUrl;
                }
            }
        }

        public async Task<List<Tuple<string, string>>> GetCityStateMapping(string country, string zipcode, EventTraceActivity traceActivityId)
        {
            List<Tuple<string, string>> retVal = new List<Tuple<string, string>>();

            try
            {
                var requestContent = new AddressEnrichmentRequest() { Address = zipcode, Country = country };
                AddressEnrichmentResponse response = await this.SendAddressEnrichmentRequest<AddressEnrichmentResponse>(
                    requestUrl: "/addresses/autocomplete",
                    requestContent: requestContent,
                    traceActivityId: traceActivityId,
                    actionName: "Autocomplete");

                foreach (var suggestedAddress in response.SuggestedAddresses)
                {
                    if (string.Equals(zipcode, suggestedAddress.Address.PostalCode, StringComparison.OrdinalIgnoreCase))
                    {
                        Tuple<string, string> currentCityRegionResponseSet = new Tuple<string, string>(
                            suggestedAddress.Address.City,
                            suggestedAddress.Address.Region);

                        if (!retVal.Contains(currentCityRegionResponseSet))
                        {
                            retVal.Add(currentCityRegionResponseSet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException("AddressEnrichmentServiceAccessor.GetCityStateMapping: " + ex.ToString(), traceActivityId);
            }

            return retVal;
        }

        public async Task<AddressValidateResponse> ValidateAddress(Address address, EventTraceActivity traceActivityId)
        {
            var originalRegion = address.Region;

            // Send AVS recognized region in address
            if (address.Region != null && Constants.RegionMappingFromPIDLToAVS.ContainsKey(address.Region))
            {
                address.Region = Constants.RegionMappingFromPIDLToAVS[address.Region];
            }

            AddressValidateResponse response = await this.SendAddressEnrichmentRequest<AddressValidateResponse>(
                requestUrl: "/addresses/validate",
                requestContent: address,
                traceActivityId: traceActivityId,
                actionName: "Validate",
                regionIsoEnabled: Constants.CountriesRequiredRegionIsoEnabledFlag.Contains(address.Country, StringComparer.InvariantCultureIgnoreCase));

            // Revert region to original
            address.Region = originalRegion;
            response.OriginalAddress.Region = originalRegion;

            // AVS can return either a single object in suggestedAddress or multiple in suggestedAddresses
            if (response.SuggestedAddress != null)
            {
                response.SuggestedAddresses = new List<Address>();
                response.SuggestedAddresses.Add(response.SuggestedAddress);
                response.SuggestedAddress = null;
            }

            if (response.SuggestedAddresses != null)
            {
                TransformRegionCodeAVSToPIDLFormat(address.Country, response.SuggestedAddresses);
            }

            return response;
        }

        private static void TransformRegionCodeAVSToPIDLFormat(string countryCode, List<Address> suggestedAddresses)
        {
            // PX has full region codes (e.g. VE-M) for the country Venezuela (ve) whereas AVS returns a single char region code (e.g. M)
            if (string.Equals(CountryCodes.VE, countryCode, StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (Address suggestedAddress in suggestedAddresses)
                {
                    suggestedAddress.Region = suggestedAddress.Region != null ? $"{CountryCodes.VE}-{suggestedAddress.Region}" : suggestedAddress.Region;
                }
            }
            else
            {
                foreach (Address suggestedAddress in suggestedAddresses)
                {
                    if (suggestedAddress.Region != null)
                    {
                        string pidlRegion = Constants.RegionMappingFromPIDLToAVS.FirstOrDefault(kv => string.Equals(kv.Value, suggestedAddress.Region, StringComparison.InvariantCultureIgnoreCase)).Key;
                        suggestedAddress.Region = string.IsNullOrEmpty(pidlRegion) ? suggestedAddress.Region : pidlRegion;
                    }
                }
            }
        }

        private async Task<T> SendAddressEnrichmentRequest<T>(
            string requestUrl,
            object requestContent,
            EventTraceActivity traceActivityId,
            string actionName,
            IList<KeyValuePair<string, string>> additionalHeaders = null,
            bool regionIsoEnabled = false)
        {
            string fullRequestUrl = string.Format("{0}{1}", this.AddressEnrichmentBaseUrl, requestUrl);
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add("x-ms-isfirstparty", "true");

                if (regionIsoEnabled)
                {
                    requestMessage.Headers.Add(Constants.ExtendedHttpHeaders.RegionIsoEnabled, Value.True);
                }

                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, "application/json");

                if (additionalHeaders != null)
                {
                    foreach (var headerKvp in additionalHeaders)
                    {
                        requestMessage.Headers.Add(headerKvp.Key, headerKvp.Value);
                    }
                }

                if (HttpRequestHelper.IsPXTestRequest() && !string.IsNullOrWhiteSpace(this.emulatorBaseUrl))
                {
                    HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, requestMessage);

                    // Only required to send the flight when using the emulator for mocking the postAddressValidate call with AVS
                    requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.FlightHeader, Flighting.Features.AccountEmulatorValidateAddressWithAVS);
                }

                // Add action name to the request properties so that this request's OperationName is logged properly
                requestMessage.AddOrReplaceActionName(actionName);

                PaymentsEventSource.Log.PXServiceRequestToAddressEnrichmentService(fullRequestUrl, traceActivityId);
                using (HttpResponseMessage response = await this.addressEnrichmentServiceHttpClient.SendAsync(requestMessage))
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();
                    string statusCode = response.StatusCode.ToString();
                    string traceMessage = string.Format("CM StatusCode: {0}, CM ResponseContent: {1}", response.StatusCode, responseMessage);
                    PaymentsEventSource.Log.PXServiceTraceResponseFromAddressEnrichmentService(statusCode, traceMessage, traceActivityId);

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<T>(responseMessage);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(
                                traceActivityId,
                                new FailedOperationException(string.Format(
                                    "Failed to deserialize success response from {0}",
                                     PXCommon.Constants.ServiceNames.AddressEnrichmentService)));
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        throw TraceCore.TraceException(
                            traceActivityId,
                            new InvalidOperationException(string.Format(
                                "Receive a bad request response from {0}: {1}",
                                PXCommon.Constants.ServiceNames.AddressEnrichmentService,
                                responseMessage ?? string.Empty)));
                    }
                    else
                    {
                        throw TraceCore.TraceException(
                            traceActivityId,
                            new FailedOperationException(string.Format(
                                "Received an error response from {0}: response status code: {1}; error: {2}",
                                 PXCommon.Constants.ServiceNames.AddressEnrichmentService,
                                response.StatusCode,
                                responseMessage != null ? responseMessage : string.Empty)));
                    }
                }
            }
        }
    }
}
