// <copyright file="MerchantCapabilitiesAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.MerchantCapabilitiesService.V7
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Tracing;
    using Newtonsoft.Json;
    using Settings;

    public class MerchantCapabilitiesAccessor : IMerchantCapabilitiesAccessor
    {
        private HttpClient merchantCapabilitiesServiceHttpClient;
        private PXServiceSettings pxsettings = null;

        public MerchantCapabilitiesAccessor(PXServiceSettings settings)
        {
            this.pxsettings = settings;
            this.merchantCapabilitiesServiceHttpClient = new PXTracingHttpClient(Constants.ServiceNames.MerchantCapabilitiesService, ApplicationInsightsProvider.LogOutgoingOperation);
            this.merchantCapabilitiesServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.merchantCapabilitiesServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.merchantCapabilitiesServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
        }

        /*
         * Filters paymentMethods based on the capabilities (supported payment methods) of the merchant (based on given merchantId) for the given currency
         */
        public async Task<IList<PaymentMethod>> FilterPaymentMethods(string merchantId, string currencyCode, IList<PaymentMethod> paymentMethods, EventTraceActivity traceActivityId)
        {
            MerchantCapabilities merchantCapabilities = await this.GetMerchantCapabilities(merchantId, currencyCode, traceActivityId);
            IDictionary<string, IList<string>> paymentMethodsPerFamily = merchantCapabilities.PaymentMethodsPerFamily;
            IList<PaymentMethod> filteredPaymentMethods = new List<PaymentMethod>();
            foreach (PaymentMethod paymentMethod in paymentMethods)
            {
                if (paymentMethodsPerFamily.ContainsKey(paymentMethod.PaymentMethodFamily))
                {
                    if (paymentMethodsPerFamily[paymentMethod.PaymentMethodFamily].Contains(paymentMethod.PaymentMethodType))
                    {
                        filteredPaymentMethods.Add(paymentMethod);
                    }
                }
            }

            return filteredPaymentMethods;
        }

        public async Task<MerchantCapabilities> GetMerchantCapabilities(string merchantId, string currencyCode, EventTraceActivity traceActivityId)
        {
            string merchantCapabilitiesRoute = merchantId + "?currency=" + currencyCode;
            return await this.SendGetRequest<MerchantCapabilities>(merchantCapabilitiesRoute, traceActivityId);
        }

        private async Task<T> SendGetRequest<T>(string merchantCapabilitiesRoute, EventTraceActivity traceActivityId)
        {
            string merchantCapabilitiesRequestUrl = string.Format("{0}/{1}/capabilities/{2}", this.pxsettings.MerchantCapabilitiesUri, this.pxsettings.MerchantCapabilitiesApiVersion, merchantCapabilitiesRoute);
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, merchantCapabilitiesRequestUrl))
            {
                request.IncrementCorrelationVector(traceActivityId);
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());

                PaymentsEventSource.Log.PXServiceTraceRequestToExternalService(Constants.ServiceNames.MerchantCapabilitiesService, merchantCapabilitiesRequestUrl, traceActivityId);
                using (HttpResponseMessage response = await this.merchantCapabilitiesServiceHttpClient.SendAsync(request))
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();
                    PaymentsEventSource.Log.PXServiceTraceResponseFromExternalService(Constants.ServiceNames.MerchantCapabilitiesService, response.StatusCode.ToString(), responseMessage, traceActivityId);

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<T>(responseMessage);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Failed to deserialize {0} response message.", Constants.ServiceNames.MerchantCapabilitiesService)));
                        }
                    }
                    else
                    {
                        MerchantCapabilitiesErrorResponse errorResponse = null;
                        try
                        {
                            errorResponse = JsonConvert.DeserializeObject<MerchantCapabilitiesErrorResponse>(responseMessage);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize error response from Merchant Capabilities Service"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new MerchantCapabilitiesErrorException() { Error = errorResponse, Response = response });
                    }
                }
            }
        }
    }
}