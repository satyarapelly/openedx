// <copyright file="WalletServiceMockResponseProvider.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Test.Common;

    public class WalletServiceMockResponseProvider : IMockResponseProvider
    { 
        public WalletServiceMockResponseProvider()
        {
        }

        public string WalletResponse { get; set; }

        public void ResetDefaults()
        {
            this.WalletResponse = null;
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            string responseContent = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            if (request.RequestUri.ToString().Contains("wallet"))
            {
                responseContent = "{\"merchantName\":\"Microsoft\",\"integrationType\":\"DIRECT\",\"directIntegrationData\":[{\"piFamily\":\"Ewallet\",\"piType\":\"ApplePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"fr\":[\"visa\",\"mastercard\",\"amex\"],\"de\":[\"visa\",\"mastercard\",\"amex\"],\"it\":[\"visa\",\"mastercard\",\"amex\"],\"es\":[\"visa\",\"mastercard\",\"amex\"],\"gb\":[\"visa\",\"mastercard\",\"amex\"]},\"providerData\":{\"merchantIdentifier\":\"merchant.com.microsoft.paymicrosoft.sandbox\",\"merchantCapabilities\":[\"supports3DS\",\"supportsEMV\"],\"version\":\"MASKED(1)\"}},{\"piFamily\":\"Ewallet\",\"piType\":\"GooglePay\",\"countrySupportedNetworks\":{\"us\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"ca\":[\"visa\",\"mastercard\",\"discover\",\"amex\"],\"fr\":[\"visa\",\"mastercard\",\"amex\"],\"de\":[\"visa\",\"mastercard\",\"amex\"],\"it\":[\"visa\",\"mastercard\",\"amex\"],\"es\":[\"visa\",\"mastercard\",\"amex\"],\"gb\":[\"visa\",\"mastercard\",\"amex\"]},\"providerData\":{\"allowedMethods\":[\"PAN_ONLY\",\"CRYPTOGRAM_3DS\"],\"assuranceDetailsRequired\":true,\"protocolVersion\":\"ECv2\",\"publicKey\":\"MASKED(88)\",\"publicKeyVersion\":\"01032025\",\"apiMajorVersion\":2,\"apiMinorVersion\":0,\"merchantId\":\"BCR2DN4TZ244PH2A\"}}]}";
                this.WalletResponse = responseContent;
            }

            if (string.IsNullOrEmpty(responseContent))
            {
                return null;
            }

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