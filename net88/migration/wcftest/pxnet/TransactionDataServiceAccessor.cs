// <copyright file="TransactionDataServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TransactionDataService
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Accessors.TransactionDataService.DataModel;
    using Newtonsoft.Json;

    public class TransactionDataServiceAccessor : ITransactionDataServiceAccessor
    {
        public const string TransactionDataServiceName = "TransationDataService";

        private HttpClient transationDataHttpClient;
        private string serviceBaseUrl;
        private string apiVersion;
        private string emulatorBaseUrl;

        public TransactionDataServiceAccessor(
            string serviceBaseUrl,
            string apiVersion,
            HttpMessageHandler messageHandler)
        {
            this.transationDataHttpClient = new PXTracingHttpClient(PXCommon.Constants.ServiceNames.TransactionDataService, messageHandler, ApplicationInsightsProvider.LogOutgoingOperation);
            this.serviceBaseUrl = serviceBaseUrl;
            this.apiVersion = apiVersion;
            this.emulatorBaseUrl = "http://localhost/TransactionDataEmulator";
        }

        private string BaseUrl
        {
            get
            {
                if (HttpRequestHelper.IsPXTestRequest("px.transactionData") && !string.IsNullOrWhiteSpace(this.emulatorBaseUrl))
                {
                    return this.emulatorBaseUrl;
                }
                else
                {
                    return this.serviceBaseUrl;
                }
            }
        }

        public async Task<string> GenerateDataId(EventTraceActivity traceActivityId)
        {
            string requestUrl = V7.Constants.UriTemplate.GenerateDataId;
            string response = await this.SendGetRequest<string>(requestUrl, "GenerateDataId", traceActivityId);
            return response;
        }

        public async Task<string> UpdateCustomerChallengeAttestation(string accountId, string sessionId, bool authenticationVerified, EventTraceActivity traceActivityId)
        {
            if (string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(sessionId))
            {
                return string.Empty;
            }

            string requestUrl = string.Format(V7.Constants.UriTemplate.TransactionDataStore, accountId, sessionId);
            CustomerChallengeAttestationRequest customerChallengeAttestationRequest = new CustomerChallengeAttestationRequest(authenticationVerified);
            TransactionDataStoreResponse response = new TransactionDataStoreResponse();
            try
            {
                response = await this.SendPostRequest<TransactionDataStoreResponse>(requestUrl, "UpdateCustomerChallengeAttestation", customerChallengeAttestationRequest, traceActivityId);
            }
            catch
            {
                try
                {
                    // retry once if initial call failed
                    response = await this.SendPostRequest<TransactionDataStoreResponse>(requestUrl, "UpdateCustomerChallengeAttestation", customerChallengeAttestationRequest, traceActivityId);
                }
                catch (Exception ex)
                {
                    // eat any thrown exception to let scenario pass
                    SllWebLogger.TracePXServiceException($"Error Updating Attestation: {ex}", traceActivityId);
                }
            }

            return response.DataReferenceId;
        }

        private async Task<T> SendGetRequest<T>(string requestUrl, string actionName, EventTraceActivity traceActivityId, IEnumerable<string> testHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, requestUrl);
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, fullRequestUrl))
            {
                request.IncrementCorrelationVector(traceActivityId);
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, this.apiVersion);

                if (testHeaders != null)
                {
                    request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader, testHeaders);
                }

                request.AddOrReplaceActionName(actionName);
                using (HttpResponseMessage response = await this.transationDataHttpClient.SendAsync(request))
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();
                    try
                    {
                        SllWebLogger.TraceServerMessage("SendGetRequest_TransactionDataServiceAccessor", traceActivityId.ToString(), null, JsonConvert.DeserializeObject<T>(responseMessage).ToString(), Diagnostics.Tracing.EventLevel.Informational);
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<T>(responseMessage);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize success response from TransactionDataService"));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? TransactionDataServiceName : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize error response from TransactionDataService"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response });
                    }
                }
            }
        }

        private async Task<T> SendPostRequest<T>(string requestUrl, string actionName, object payload, EventTraceActivity traceActivityId, IEnumerable<string> testHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, requestUrl);
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, fullRequestUrl))
            {
                request.IncrementCorrelationVector(traceActivityId);
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, this.apiVersion);

                if (testHeaders != null)
                {
                    request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader, testHeaders);
                }

                if (payload != null)
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
                }

                request.AddOrReplaceActionName(actionName);
                using (HttpResponseMessage response = await this.transationDataHttpClient.SendAsync(request))
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();
                    try
                    {
                        SllWebLogger.TraceServerMessage("SendPostRequest_TransactionDataServiceAccessor", traceActivityId.ToString(), null, JsonConvert.DeserializeObject<T>(responseMessage).ToString(), Diagnostics.Tracing.EventLevel.Informational);
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<T>(responseMessage);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize success response from TransactionDataService"));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? TransactionDataServiceName : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize error response from TransactionDataService"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response });
                    }
                }
            }
        }
    }
}