// <copyright file="TraceBuilderHelper.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using Microsoft.AspNetCore.Http;
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

        // For backward compatibility (HttpRequestMessage)
        public static string BuildTraceMessage(HttpRequestMessage request, string payload)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(request.Method?.Method ?? "<no-method>");
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
            builder.Append(payload ?? "<none>");

            return builder.ToString();
        }

        public static string BuildTraceMessage(HttpResponseMessage response, string payload)
        {
            if (response == null)
            {
                return "<none>";
            }

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
            builder.Append(payload ?? "<none>");

            return builder.ToString();
        }

        // ASP.NET Core: IHeaderDictionary support
        public static string BuildHeaderString(IHeaderDictionary headers)
        {
            var list = headers.Select(header => new KeyValuePair<string, string>(
                header.Key,
                string.Join(",", header.Value.Select(SanitizeHeaderValue)))).ToList();

            return JsonConvert.SerializeObject(list, SerializerSettings);
        }

        // For HttpHeaders (System.Net.Http)
        public static string BuildHeaderString(HttpHeaders headers)
        {
            var list = new List<KeyValuePair<string, string>>();

            foreach (var header in headers)
            {
                list.Add(new KeyValuePair<string, string>(header.Key, GetSanitizeValueForLogging(header.Value)));
            }

            return JsonConvert.SerializeObject(list, SerializerSettings);
        }

        private static void AddHeaders(StringBuilder builder, HttpHeaders headers)
        {
            foreach (var header in headers)
            {
                builder.Append(header.Key);
                builder.Append(": ");
                builder.Append(GetSanitizeValueForLogging(header.Value));
                builder.AppendLine();
            }
        }

        private static string GetSanitizeValueForLogging(IEnumerable<string> values)
        {
            return values == null ? string.Empty : string.Join(",", values.Select(SanitizeHeaderValue));
        }

        private static string SanitizeHeaderValue(string value)
        {
            // Implement actual redaction logic as needed
            return value?.Length > 200 ? value.Substring(0, 200) + "..." : value;
        }
    }
}
