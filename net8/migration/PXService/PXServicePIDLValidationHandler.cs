// <copyright file="PXServicePIDLValidationHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using static Microsoft.Commerce.Payments.Common.PaymentConstants;

    /// <summary>
    /// Middleware which validates DisplayDescription with DataDescription in PIDL document at response
    /// </summary>
    public class PXServicePIDLValidationHandler
    {
        private static readonly string[] ValidationAllowedControllers = { "AddressDescriptionsController", "PaymentMethodDescriptionsController", "ProfileDescriptionsController", "ChallengeDescriptionsController", "TaxIdDescriptionsController" };

        private readonly RequestDelegate next;

        public PXServicePIDLValidationHandler(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public static bool ValidatePIDLDocument(string pidlDocument, EventTraceActivity requestCorrelationId)
        {
            bool validationSucceeded = true;
            JArray pidlJArray = new JArray();
            bool parseSucceeded = TryParseJArray(pidlDocument, out pidlJArray);

            if (parseSucceeded)
            {
                foreach (var pidl in pidlJArray)
                {
                    var dataMissingForProperties = ValidatePIDL(pidl);
                    if (dataMissingForProperties.Count > 0)
                    {
                        validationSucceeded = false;
                        SllWebLogger.TracePXServiceException($"PXServicePIDLValidationHandler found no data in DataDescription for display properties: {dataMissingForProperties}", requestCorrelationId);
                    }
                }
            }
            else
            {
                SllWebLogger.TracePXServiceException("PXServicePIDLValidationHandler failed to parse the PIDL document Json", requestCorrelationId);
            }

            return validationSucceeded;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            var originalBody = context.Response.Body;

            using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            await this.next(context);

            buffer.Seek(0, SeekOrigin.Begin);
            string responseBody = await new StreamReader(buffer).ReadToEndAsync();
            buffer.Seek(0, SeekOrigin.Begin);
            await buffer.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            try
            {
                RouteData? routeData = context.GetRouteData();
                object? controller;

                bool validationRequired = HttpMethods.IsGet(request.Method)
                    && routeData != null
                    && routeData.Values.TryGetValue("controller", out controller)
                    && controller != null
                    && ValidationAllowedControllers.Contains(controller.ToString() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    && context.Response.StatusCode == (int)HttpStatusCode.OK
                    && context.Response.ContentType?.StartsWith(HttpMimeTypes.JsonContentType, StringComparison.OrdinalIgnoreCase) == true;

                if (validationRequired)
                {
                    ValidatePIDLDocument(responseBody, request.GetRequestCorrelationId());
                }
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(
                    $"PXServicePIDLValidationHandler had unexpected failure: {ex.Message}\n{ex.StackTrace}",
                    request.GetRequestCorrelationId());
            }
        }

        private static bool TryParseJArray(string s, out JArray jarray)
        {
            jarray = null;

            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            // This validation helps improve performance by not trying to parse if the first char is incorrect
            if (!s.Trim()[0].Equals('['))
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

            if (searchResults.Count > 0)
            {
                return true;
            }

            return false;
        }

        private static List<string> ValidatePIDL(JToken pidl)
        {
            List<string> dataMissingForProperties = new List<string>();
            JToken dataDescription = pidl?["data_description"];
            JToken displayDescription = pidl?["displayDescription"];

            if (displayDescription != null && dataDescription != null)
            {
                // Select all tokens which have propertyName in displayDescription
                List<string> displayDescriptionProperties = displayDescription.SelectTokens("$..propertyName").Select(t => t.Value<string>()).ToList();

                foreach (string displayProperty in displayDescriptionProperties)
                {
                    if (!string.IsNullOrWhiteSpace(displayProperty))
                    {
                        // Check whether the data for property exists in data description or not
                        bool propertyDataFound = IsTokenFoundInSourceToken(dataDescription, displayProperty);
                        if (!propertyDataFound)
                        {
                            dataMissingForProperties.Add(displayProperty);
                        }
                    }
                }
            }

            return dataMissingForProperties;
        }
    }
}