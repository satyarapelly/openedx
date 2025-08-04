// <copyright file="IssuerServiceMockResponseProvider.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Test.Common;

    public class IssuerServiceMockResponseProvider : IMockResponseProvider
    {
        public void ResetDefaults()
        {
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            string responseContent = string.Empty;

            request.Properties.TryGetValue("InstrumentManagement.ActionName", out object actionName);

            if (actionName == null)
            {
                responseContent = "not supported function ";
            }

            string action = (string)actionName ?? string.Empty;

            if (action.Contains("applyEligibility") || request.RequestUri.ToString().Contains("eligibility"))
            {
                responseContent = "{\"status\":\"None\",\"eligibleToApply\":true,\"prescreened\":true}";
            }
            else if (action.Contains("applicationDetails") ||
                (request.RequestUri.ToString().Contains("applications") &&
                request.RequestUri.ToString().Contains("sessionId") &&
                request.RequestUri.ToString().Contains("cardProduct")))
            {
                responseContent = "[{\"sessionId\": \"57fa10b2-2ca2-4f4e-8817-9c19379b1c29\",\"customerPuid\": null,\"jarvisAccountId\": null,\"issuerCustomerId\": null,\"cardProduct\": null,\"channel\": null,\"subChannel\": null,\"market\": null,\"issuerAccountId\": null,\"lastFourDigits\": null,\"paymentInstrumentId\": \"q62zBAAAAAAJAACA\",\"status\": \"PendingOnApplication\",\"errorDetails\": null,\"createDate\": null,\"modifiedDate\": null}]";
            }
            else if (action.Contains("initialize") || request.RequestUri.ToString().Contains("session"))
            {
                responseContent = "{\"sessionId\":\"57fa10b2-2ca2-4f4e-8817-9c19379b1c29\"}";
            }
            else if (action.Contains("apply") || request.RequestUri.ToString().Contains("applications"))
            {
                responseContent = "{\"status\":\"Approved\",\"redirectUrl\":\"https://mockRedirectUrl.com\"}";
            }

            HttpStatusCode statusCode = HttpStatusCode.OK;

            return await Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(
                    responseContent,
                    System.Text.Encoding.UTF8,
                    "application/json")
            });
        }
    }
}