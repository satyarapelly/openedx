// <copyright file="TraceBuilderHelper.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using Newtonsoft.Json;

    public static class TraceBuilderHelper
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new SnakeCasingJsonContractResolver
            {
                UseDefaultDictionaryPropertyNameResolution = true
            }
        };

        /// <summary>
        /// Build trace message for an HTTP request message.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="payload">The payload (content) of the request. The content should not contains sensitive data</param>
        /// <returns>The trace message.</returns>
        public static string BuildTraceMessage(HttpRequestMessage request, string payload)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(request.Method.Method);
            builder.Append(" ");

            if (request.RequestUri != null)
            {
                builder.Append(request.RequestUri.IsAbsoluteUri ? request.RequestUri.PathAndQuery : request.RequestUri.ToString());
            }

            builder.AppendLine();

            AddHeaders(builder, request.Headers);
            if (request.Content != null)
            {
                AddHeaders(builder, request.Content.Headers);
            }

            builder.AppendLine();
            if (payload == null)
            {
                builder.Append("<none>");
            }
            else
            {
                builder.Append(payload);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Build trace message for an HTTP response message.
        /// </summary>
        /// <param name="response">The response message.</param>
        /// <param name="payload">The payload (content) of the response. The content should not contains sensitive data</param>
        /// <returns>The trace message.</returns>
        public static string BuildTraceMessage(HttpResponseMessage response, string payload)
        {
            if (response == null)
            {
                return "<none>";
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                builder.Append((int)response.StatusCode);
                builder.Append(" ");
                builder.Append(response.StatusCode);
                builder.AppendLine();

                AddHeaders(builder, response.Headers);
                if (response.Content != null)
                {
                    AddHeaders(builder, response.Content.Headers);
                }

                builder.AppendLine();
                if (payload == null)
                {
                    builder.Append("<none>");
                }
                else
                {
                    builder.Append(payload);
                }

                return builder.ToString();
            }
        }

        public static string BuildHeaderString(HttpHeaders httpHeaders)
        {
            List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
            foreach (KeyValuePair<string, IEnumerable<string>> header in httpHeaders)
            {
                headers.Add(new KeyValuePair<string, string>(header.Key, header.GetSanitizeValueForLogging()));
            }

            return JsonConvert.SerializeObject(headers, SerializerSettings);
        }

        /// <summary>
        /// Adds a set of request or response headers to the builder that is constructing
        /// a trace message.
        /// </summary>
        /// <param name="builder">The trace message builder.</param>
        /// <param name="headers">The headers to add to the trace message.</param>
        private static void AddHeaders(StringBuilder builder, HttpHeaders headers)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
            {
                builder.Append(header.Key);
                builder.Append(": ");
                builder.Append(header.GetSanitizeValueForLogging());
                builder.AppendLine();
            }
        }
    }
}
