// <copyright file="HttpResponseMessageExtensions.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using System.Web;

    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Get the content length of request for SLL tracing. 
        /// </summary>
        /// <param name="response">The http requst message</param>
        /// <returns>The content size of request.</returns>
        public static int GetRequestContentLength(this HttpResponseMessage response)
        {
            if (response != null &&
                response.Content != null &&
                response.Content.Headers != null &&
                response.Content.Headers.ContentLength.HasValue)
            {
                return Convert.ToInt32(response.Content.Headers.ContentLength.Value);
            }

            return 0;
        }

        public static string GetResponseContentType(this HttpResponseMessage response)
        {
            if (response != null
                && response.Content != null
                && response.Content.Headers != null
                && response.Content.Headers.ContentType != null)
            {
                return response.Content.Headers.ContentType.MediaType;
            }

            return string.Empty;
        }

        public static async Task<string> GetResponsePayload(this HttpResponseMessage response)
        {
            string responsePayload;
            if (response == null
                || response.Content == null
                || response.Content.Headers == null
                || response.Content.Headers.ContentLength == 0)
            {
                responsePayload = "<none>";
            }
            else
            {
                string stringContent = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(stringContent))
                {
                    responsePayload = "<none>";
                }
                else
                {
                    responsePayload = stringContent;
                }
            }

            return responsePayload;
        }

        public static string GetResponseHeaderString(this HttpResponseMessage response)
        {
            if (response == null || response.Headers == null)
            {
                return "<none>";
            }

            return TraceBuilderHelper.BuildHeaderString(response.Headers);
        }

        public static bool DoesReponseIndicateIdempotentTransaction(this HttpResponseMessage response)
        {
            if (response != null && response.Headers != null)
            {
                HttpResponseHeaders responseHeaders = response.Headers;
                if (responseHeaders.Contains(PaymentConstants.PaymentExtendedHttpHeaders.IdempotencyHeaderName))
                {
                    IEnumerable<string> data = responseHeaders.GetValues(PaymentConstants.PaymentExtendedHttpHeaders.IdempotencyHeaderName);

                    // Extract the x-ms-idempotency header data and check if it was true / false (x-ms-idempotency=true)
                    foreach (string idempotencyIndicator in data)
                    {
                        return idempotencyIndicator.ToLower().Equals("true");
                    }
                }
            }

            return false;
        }

        public static async Task<TObject> ReadAsObject<TObject>(this HttpResponseMessage responseMessage, EventTraceActivity eta, params HttpStatusCode[] goodStatusCodes)
        {
            if (((goodStatusCodes == null || goodStatusCodes.Length == 0) && responseMessage.IsSuccessStatusCode)
                || (goodStatusCodes != null && goodStatusCodes.Contains(responseMessage.StatusCode)))
            {
                return JsonConvert.DeserializeObject<TObject>(await responseMessage.Content.ReadAsStringAsync(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }

            string errorResponse = null;
            if (responseMessage.Content != null)
            {
                errorResponse = await responseMessage.Content.ReadAsStringAsync();
            }

            throw TraceCore.TraceException(
                new EventTraceActivity(eta.ActivityId),
                new Exception(errorResponse));
        }
        public static async Task<(bool success, T? value)> TryGetContentValueAsync<T>(this HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            try
            {
                var contentString = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(contentString))
                {
                    return (false, default);
                }

                var value = JsonConvert.DeserializeObject<T>(contentString);
                return (value != null, value);
            }
            catch
            {
                return (false, default);
            }
        }
    }
}
