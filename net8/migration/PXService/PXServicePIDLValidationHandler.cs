using Microsoft.AspNetCore.Http;
using Microsoft.Commerce.Payments.Common.Web;
using Microsoft.Commerce.Payments.PXCommon;
using Microsoft.Commerce.Tracing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static Microsoft.Commerce.Payments.Common.PaymentConstants;

namespace Microsoft.Commerce.Payments.PXService
{
    /// <summary>
    /// Middleware that validates PIDL documents in responses.
    /// </summary>
    public class PXServicePIDLValidationHandler
    {
        private static readonly string[] ValidationAllowedControllers =
            { "AddressDescriptionsController", "PaymentMethodDescriptionsController", "ProfileDescriptionsController", "ChallengeDescriptionsController", "TaxIdDescriptionsController" };

        private readonly RequestDelegate _next;

        public PXServicePIDLValidationHandler(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBody = context.Response.Body;
            await using var memory = new MemoryStream();
            context.Response.Body = memory;

            await _next(context);

            memory.Seek(0, SeekOrigin.Begin);
            var controller = context.Request.RouteValues["controller"]?.ToString();
            var shouldValidate = context.Request.Method == HttpMethods.Get
                && controller != null
                && ValidationAllowedControllers.Contains(controller, StringComparer.OrdinalIgnoreCase)
                && context.Response.StatusCode == (int)HttpStatusCode.OK
                && context.Response.ContentType != null
                && context.Response.ContentType.StartsWith(HttpMimeTypes.JsonContentType, StringComparison.OrdinalIgnoreCase);

            if (shouldValidate)
            {
                var body = await new StreamReader(memory).ReadToEndAsync();
                memory.Seek(0, SeekOrigin.Begin);
                ValidatePIDLDocument(body, context.Request.GetRequestCorrelationId());
            }

            await memory.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }

        public static bool ValidatePIDLDocument(string pidlDocument, EventTraceActivity requestCorrelationId)
        {
            bool validationSucceeded = true;
            if (TryParseJArray(pidlDocument, out var pidlJArray))
            {
                foreach (var pidl in pidlJArray)
                {
                    var dataMissing = ValidatePIDL(pidl);
                    if (dataMissing.Count > 0)
                    {
                        validationSucceeded = false;
                        SllWebLogger.TracePXServiceException($"PXServicePIDLValidationHandler found no data in DataDescription for display properties: {dataMissing}", requestCorrelationId);
                    }
                }
            }
            else
            {
                SllWebLogger.TracePXServiceException("PXServicePIDLValidationHandler failed to parse the PIDL document Json", requestCorrelationId);
            }

            return validationSucceeded;
        }

        private static bool TryParseJArray(string s, out JArray jarray)
        {
            jarray = null;

            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            if (!s.TrimStart().StartsWith("["))
            {
                return false;
            }

            try
            {
                jarray = JArray.Parse(s);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        private static bool IsTokenFoundInSourceToken(JToken sourceJToken, string tokenName)
        {
            var searchResults = sourceJToken.SelectTokens("$.." + tokenName).ToList();
            return searchResults.Count > 0;
        }

        private static List<string> ValidatePIDL(JToken pidl)
        {
            List<string> missing = new();
            JToken dataDescription = pidl?["data_description"];
            JToken displayDescription = pidl?["displayDescription"];

            if (displayDescription != null && dataDescription != null)
            {
                var displayProps = displayDescription.SelectTokens("$..propertyName").Select(t => t.Value<string>()).ToList();
                foreach (var prop in displayProps)
                {
                    if (!string.IsNullOrWhiteSpace(prop) && !IsTokenFoundInSourceToken(dataDescription, prop))
                    {
                        missing.Add(prop);
                    }
                }
            }

            return missing;
        }
    }
}
