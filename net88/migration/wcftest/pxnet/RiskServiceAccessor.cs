// <copyright file="RiskServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.RiskService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Accessors.RiskService;
    using Newtonsoft.Json;

    public class RiskServiceAccessor : IRiskServiceAccessor
    {
        private const char PaymentMethodFamilyTypeDelimiter = '-';

        private readonly HttpClient riskServiceHttpClient;
        private readonly string serviceBaseUrl;
        private readonly string emulatorBaseUrl;
        private readonly string apiVersion;

        private readonly List<string> passThroughHeaders = new List<string>
        {
            GlobalConstants.HeaderValues.ExtendedFlightName,
            PaymentConstants.PaymentExtendedHttpHeaders.TestHeader
        };

        public RiskServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            string apiVersion,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;
            this.apiVersion = apiVersion;

            this.riskServiceHttpClient = new PXTracingHttpClient(Constants.ServiceNames.RiskService, messageHandler, ApplicationInsightsProvider.LogOutgoingOperation);
            this.riskServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.riskServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.riskServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
        }

        private string BaseUrl
        {
            get
            {
                if (HttpRequestHelper.IsPXTestRequest("px.risk") && !string.IsNullOrWhiteSpace(this.emulatorBaseUrl))
                {
                    return this.emulatorBaseUrl;
                }
                else
                {
                    return this.serviceBaseUrl;
                }
            }
        }

        public async Task<IList<PaymentMethod>> FilterPaymentMethods(string puid, string client, string orderId, string sessionId, IList<PaymentMethod> paymentMethods, EventTraceActivity traceActivityId)
        {
            IDictionary<string, PaymentMethod> originalPaymentMethods = paymentMethods.ToDictionary(pm => GetIdentifier(pm));
            RiskServicePISelectionRequest request = new RiskServicePISelectionRequest(puid, client, "6.0", orderId, sessionId, paymentMethods);
            return await this.FilterPaymentMethods(request, originalPaymentMethods, traceActivityId);
        }

        public async Task<IList<PaymentInstrument>> FilterPaymentInstruments(string puid, string client, string orderId, string sessionId, IList<PaymentInstrument> paymentInstruments, List<PaymentInstrument> disabledPaymentInstruments, EventTraceActivity traceActivityId)
        {
            IDictionary<string, PaymentMethod> originalPaymentMethods = new Dictionary<string, PaymentMethod>();
            foreach (PaymentInstrument pi in paymentInstruments)
            {
                string identifier = GetIdentifier(pi.PaymentMethod);
                if (!originalPaymentMethods.ContainsKey(identifier))
                {
                    originalPaymentMethods[identifier] = pi.PaymentMethod;
                }
            }

            RiskServicePISelectionRequest request = new RiskServicePISelectionRequest(puid, client, "6.0", orderId, sessionId, paymentInstruments);

            IList<PaymentMethod> filteredPaymentMethods = await this.FilterPaymentMethods(request, originalPaymentMethods, traceActivityId);
            IList<PaymentInstrument> filteredPaymentInstruments = new List<PaymentInstrument>();

            foreach (PaymentInstrument pi in paymentInstruments)
            {
                if (filteredPaymentMethods.Any(pm => pm.EqualByFamilyAndType(pi.PaymentMethod)))
                {
                    filteredPaymentInstruments.Add(pi);
                }
                else
                {
                    disabledPaymentInstruments.Add(pi);
                }
            }

            return filteredPaymentInstruments;
        }

        public async Task<IList<PaymentMethod>> FilterBasedOnRiskEvaluation(string client, string puid, string tid, string oid, IList<PaymentMethod> paymentMethods, string ipAddress, string locale, string deviceType, EventTraceActivity traceActivityId)
        {
            IDictionary<string, PaymentMethod> originalPaymentMethods = paymentMethods.ToDictionary(pm => GetIdentifier(pm));
            RiskEligibilityRequest request = new RiskEligibilityRequest(client, puid, tid, oid, ipAddress, locale, deviceType, paymentMethods);
            return await this.PerformRiskEvaluation(request, originalPaymentMethods, traceActivityId);
        }

        private static string GetIdentifier(PaymentMethod paymentMethod)
        {
            return GetIdentifier(new RiskServicePaymentInformation(paymentMethod));
        }

        private static string GetIdentifier(RiskServicePaymentInformation rspi)
        {
            return rspi.PaymentMethodFamily + PaymentMethodFamilyTypeDelimiter + rspi.PaymentMethodType;
        }

        private static string GetIdentifier(RiskServiceResponsePaymentInstrument rspi)
        {
            return rspi.PaymentInstrumentFamily + PaymentMethodFamilyTypeDelimiter + rspi.PaymentInstrumentType;
        }

        private async Task<IList<PaymentMethod>> FilterPaymentMethods(RiskServicePISelectionRequest request, IDictionary<string, PaymentMethod> originalPaymentMethods, EventTraceActivity traceActivityId)
        {
            try
            {
                RiskServicePISelectionResponse response = await this.SendPostRequest<RiskServicePISelectionResponse>(
                    Constants.UriTemplate.PiSelection,
                    request,
                    traceActivityId);

                IList<PaymentMethod> filteredPaymentMethods = new List<PaymentMethod>();
                foreach (RiskServicePaymentInformation rspi in response.PaymentInfo)
                {
                    string identifier = GetIdentifier(rspi);
                    if (originalPaymentMethods.ContainsKey(identifier))
                    {
                        if (rspi.Allowed)
                        {
                            filteredPaymentMethods.Add(originalPaymentMethods[identifier]);
                        }
                    } 
                }

                return filteredPaymentMethods;
            }
            catch (Exception e)
            {
                string message = e.InnerException != null ? e.InnerException.Message : e.Message;
                SllWebLogger.TracePXServiceException($"Exception Calling Risk Service:" + message, traceActivityId);
                return originalPaymentMethods.Values.ToList();
            }
        }

        private async Task<T> SendPostRequest<T>(string requestUrl, object request, EventTraceActivity traceActivityId)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, requestUrl);
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString()); // unused
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, this.apiVersion);

                HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, requestMessage);

                if (request != null)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
                }

                try
                {
                    using (HttpResponseMessage response = await this.riskServiceHttpClient.SendAsync(requestMessage))
                    {
                        string responseMessage = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            try
                            {
                                return JsonConvert.DeserializeObject<T>(responseMessage);
                            }
                            catch
                            {
                                throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize success response from Risk Service"));
                            }
                        }
                        else
                        {
                            RiskServiceErrorResponse error = null;
                            try
                            {
                                error = JsonConvert.DeserializeObject<RiskServiceErrorResponse>(responseMessage);
                            }
                            catch
                            {
                                throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize error response from Risk Service: " + responseMessage));
                            }

                            throw TraceCore.TraceException(traceActivityId, new RiskServiceErrorResponseException(response, error));
                        }
                    }
                }
                catch (Exception e)
                {
                    string message = e.InnerException != null ? e.InnerException.Message : e.Message;
                    throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to send POST request to the Risk Service: " + message));
                }
            }
        }

        private async Task<IList<PaymentMethod>> PerformRiskEvaluation(RiskEligibilityRequest request, IDictionary<string, PaymentMethod> originalPaymentMethods, EventTraceActivity traceActivityId)
        {
            try
            {
                IList<PaymentMethod> filteredPaymentMethods = new List<PaymentMethod>();

                RiskEligibilityResponse response = await this.SendPostRequest<RiskEligibilityResponse>(
                    Constants.UriTemplate.RiskEligibilty,
                    request,
                    traceActivityId);

                foreach (RiskServiceResponsePaymentInstrument riskServiceResponsePaymentInstrument in response.PaymentInstrumentTypes)
                {
                    string identifier = GetIdentifier(riskServiceResponsePaymentInstrument);
                    if (originalPaymentMethods.ContainsKey(identifier))
                    {
                        if (riskServiceResponsePaymentInstrument.Allowed)
                        {
                            filteredPaymentMethods.Add(originalPaymentMethods[identifier]);
                        }
                    }
                }

                return filteredPaymentMethods;
            }
            catch (Exception e)
            {
                string message = e.InnerException != null ? e.InnerException.Message : e.Message;
                SllWebLogger.TracePXServiceException($"Exception Calling Risk Service Risk Eligibility:" + message, traceActivityId);
                return originalPaymentMethods.Values.ToList();
            }
        }
    }
}