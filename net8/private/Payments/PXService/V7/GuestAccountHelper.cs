// <copyright file="GuestAccountHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXService.V7.Contexts;
    using Microsoft.Commerce.Tracing;
    using Microsoft.Commerce.Payments.PXCommon;
    using System;
    using System.Net.Http;
    using Microsoft.AspNetCore.Http;
    using static Microsoft.Commerce.Payments.PXService.V7.Contexts.Constants;

    public class GuestAccountHelper
    {
        /// <summary>
        /// Customer type key. This key is used to store customer type in HttpContext.
        /// The customer type is extracted from x-ms-customer request header.
        /// </summary>
        private const string CustomerTypeKey = "x-ms-customer_customerType";

        /// <summary>
        /// Check if the request is from guest account based on the metadata in the x-ms-customer request header.
        /// </summary>
        /// <param name="request">HttpRequest object</param>
        /// <returns>Returns bool isGuestAccount </returns>
        public static bool IsGuestAccount(HttpRequestMessage request)
        {
            try
            {
                string? customerType;
                if (request.ContainsProperty(CustomerTypeKey))
                {
                    customerType = request.GetProperty<string>(CustomerTypeKey);
                }
                else
                {
                    CustomerHeader customerHeader = CustomerHeader.Parse(request);
                    customerType = customerHeader?.TargetCustomer?.CustomerType;
                    if (!string.IsNullOrWhiteSpace(customerType))
                    {
                        // Store customer type in request properties so that it can be used without parsing customer header again.
                        request.SetProperty(CustomerTypeKey, customerType);
                    }
                }

                return customerType?.Equals(
                    CustomerType.AnonymousUser,
                    StringComparison.InvariantCultureIgnoreCase)
                    ?? false;
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException("GuestAccountHelper.IsGuestAccount: " + ex.ToString(), EventTraceActivity.Empty);
            }

            return false;
        }

        public static bool IsGuestAccount(HttpRequest request)
        {
            try
            {
                var httpContext = request.HttpContext;

                // Try retrieving cached value from HttpContext.Items
                if (httpContext.Items.TryGetValue(CustomerTypeKey, out var cachedType) && cachedType is string customerTypeFromContext)
                {
                    return customerTypeFromContext.Equals(CustomerType.AnonymousUser, StringComparison.OrdinalIgnoreCase);
                }

                // Parse customer header if not already cached
                CustomerHeader customerHeader = CustomerHeader.Parse(request.ToHttpRequestMessage()); // This method must accept HttpRequest now
                string customerType = customerHeader?.TargetCustomer?.CustomerType;

                if (!string.IsNullOrWhiteSpace(customerType))
                {
                    // Store in HttpContext for future use
                    httpContext.Items[CustomerTypeKey] = customerType;
                }

                return customerType?.Equals(CustomerType.AnonymousUser, StringComparison.OrdinalIgnoreCase) ?? false;
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException("GuestAccountHelper.IsGuestAccount: " + ex, EventTraceActivity.Empty);
            }

            return false;
        }
    }
}