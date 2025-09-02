// <copyright file="HttpResponseExtensions.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Commerce.Payments.Common.Tracing;
using Newtonsoft.Json;

namespace Microsoft.Commerce.Payments.Common.Web
{
    public static class HttpResponseExtensions
    {
        public static int GetRequestContentLength(this HttpResponse response)
        {
            return response.ContentLength.HasValue ? (int)response.ContentLength.Value : 0;
        }

        public static string GetResponseContentType(this HttpResponse response)
        {
            return response.ContentType ?? string.Empty;
        }

        public static async Task<string> GetResponsePayloadAsync(this HttpResponse response)
        {
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            response.Body.Position = 0;
            return string.IsNullOrWhiteSpace(body) ? "<none>" : body;
        }

        public static string GetResponseHeaderString(this HttpResponse response)
        {
            if (response.Headers == null || response.Headers.Count == 0)
            {
                return "<none>";
            }

            return TraceBuilderHelper.BuildHeaderString(response.Headers);
        }

        public static bool DoesReponseIndicateIdempotentTransaction(this HttpResponse response)
        {
            if (response.Headers.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.IdempotencyHeaderName, out var values))
            {
                return values.Any(value => value.Equals("true", StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }

        public static int GetResponseContentLength(this HttpResponse response)
        {
            return response.ContentLength.HasValue ? (int)response.ContentLength.Value : -1;
        }

        public static async Task<TObject> ReadAsObjectAsync<TObject>(this HttpResponse response, EventTraceActivity eta, params HttpStatusCode[] goodStatusCodes)
        {
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body, leaveOpen: true);
            string content = await reader.ReadToEndAsync();
            response.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(content))
            {
                throw TraceCore.TraceException(new EventTraceActivity(eta.ActivityId), new Exception("<empty response>"));
            }

            try
            {
                return JsonConvert.DeserializeObject<TObject>(content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                throw TraceCore.TraceException(new EventTraceActivity(eta.ActivityId), ex);
            }
        }
    }
}
