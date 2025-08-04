// <copyright file="PimsMockResponseProvider.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;
    using Test.Common;
    using global::Tests.Common.Model.Pims;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class PimsMockResponseProvider : IMockResponseProvider
    {
        static PimsMockResponseProvider()
        {
            var pmsByCountryJson = File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"Mocks\PIMS\PaymentMethodsByCountry.json"));

            PaymentMethodsByCountry = JsonConvert.DeserializeObject<Dictionary<string, List<PaymentMethod>>>(
                pmsByCountryJson);

            var paymentInstrumentsJson = File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"Mocks\PIMS\PaymentInstruments.json"));

            PaymentInstruments = JsonConvert.DeserializeObject<List<PaymentInstrument>>(
                paymentInstrumentsJson);

            var pmsByProviderJson = File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"Mocks\PIMS\PaymentMethodsByProvider.json"));

            PaymentMethodsByProvider = JsonConvert.DeserializeObject<Dictionary<string, List<PaymentMethod>>>(
                pmsByProviderJson);
        }

        public PimsMockResponseProvider()
        {
            this.RequiredChallenges = new List<string>();
        }

        public static Dictionary<string, List<PaymentMethod>> PaymentMethodsByCountry { get; private set; }

        public static List<PaymentInstrument> PaymentInstruments { get; private set; }

        public static Dictionary<string, List<PaymentMethod>> PaymentMethodsByProvider { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        public List<string> RequiredChallenges { get; set; }

        public bool IsBillingAccountIdSet { get; set; }

        public static IEnumerable<PaymentMethod> GetPaymentMethods(string country, string family, string type)
        {
            if (!PaymentMethodsByCountry.ContainsKey(country.ToLower()))
            {
                return new List<PaymentMethod>();
            }

            return PaymentMethodsByCountry[country.ToLower()].Where(pm =>
            {
                return (string.IsNullOrEmpty(family) || string.Equals(family, pm.PaymentMethodFamily)) &&
                       (string.IsNullOrEmpty(type) || string.Equals(type, pm.PaymentMethodType));
            });
        }

        public static IEnumerable<PaymentMethod> GetThirdPartyPaymentMethods(string provider, string sellerCountry, string buyerCountry)
        {
            if (!PaymentMethodsByProvider.ContainsKey(provider.ToLower()))
            {
                return new List<PaymentMethod>();
            }

            return PaymentMethodsByProvider[provider.ToLower()];
        }

        public static IEnumerable<PaymentInstrument> ListPaymentInstruments(string accountId)
        {
            return PaymentInstruments.Where(pi => pi.PaymentInstrumentAccountId == accountId);
        }

        public static PaymentInstrument GetPaymentInstrument(string accountId, string piId)
        {
            return PaymentInstruments.FirstOrDefault(pi =>
            {
                return pi.PaymentInstrumentAccountId == accountId
                       && pi.PaymentInstrumentId == piId;
            });
        }

        public void ResetDefaults()
        {
            this.RequiredChallenges.Clear();
            this.IsBillingAccountIdSet = false;
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            // For SelfHosted env, MockServiceHandler sends request first to get the GetMatchedMockResponse instead of emulator controller
            // if the response from the GetMatchedMockResponse is null then Handler will send the request to emulator controller where it utilize the testScenarioHeaders or testContext
            // Returning null here to send the request to controller when the pims scenarioHeader is present
            bool isPimsTestScenarioHeaderRequest = request.Headers.TryGetValues(Test.Common.Constants.HeaderValues.TestHeader, out var testHeaderValue) && testHeaderValue.FirstOrDefault().Contains(Constants.TestScenarios.PXPims);

            // Ignore request from CIT for the logic to return null. Host name for CIT pims service is mockPims
            bool isRequestFromCIT = string.Equals(request.RequestUri.Host, "mockPims", StringComparison.OrdinalIgnoreCase);

            if (isPimsTestScenarioHeaderRequest && WebHostingUtility.IsApplicationSelfHosted() & !isRequestFromCIT)
            {
                return null;
            }
            
            HttpStatusCode statusCode = HttpStatusCode.OK;
            string responseContent = string.Empty;
            var trimmedSegments = request.RequestUri.Segments.Select(s => s.Trim(new char[] { '/' })).ToArray();
            if (string.Equals("paymentMethods", trimmedSegments[2], StringComparison.OrdinalIgnoreCase))
            {
                var queryKvc = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(request.RequestUri.Query);
                var filteredPms = GetPaymentMethods(
                    country: queryKvc.TryGetValue("country", out var country) ? country.FirstOrDefault() : null,
                    family: queryKvc.TryGetValue("family", out var family) ? family.FirstOrDefault() : null,
                    type: queryKvc.TryGetValue("type", out var type) ? type.FirstOrDefault() : null).ToArray();

                responseContent = JsonConvert.SerializeObject(filteredPms);
            }
            else if (string.Equals("thirdPartyPayments", trimmedSegments[2], StringComparison.OrdinalIgnoreCase))
            {
                var queryKvc = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(request.RequestUri.Query);
                var filteredPms = GetThirdPartyPaymentMethods(
                    provider: queryKvc.TryGetValue("provider", out var provider) ? provider.FirstOrDefault() : null,
                    sellerCountry: queryKvc.TryGetValue("sellerCountry", out var sellerCountry) ? sellerCountry.FirstOrDefault() : null,
                    buyerCountry: queryKvc.TryGetValue("buyerCountry", out var buyerCountry) ? buyerCountry.FirstOrDefault() : null).ToArray();

                responseContent = JsonConvert.SerializeObject(filteredPms);
            }
            else if (string.Equals("paymentInstruments", trimmedSegments[3], StringComparison.OrdinalIgnoreCase)
                     && trimmedSegments.Count() >= 6
                     && string.Equals("validate", trimmedSegments[5], StringComparison.OrdinalIgnoreCase))
            {
                responseContent = "{\"paymentMethodFamily\":\"credit_card\",\"paymentMethodType\":\"visa\",\"result\":\"Success\"}";
            }
            else if (trimmedSegments.Length >= 4 && string.Equals("paymentInstruments", trimmedSegments[3], StringComparison.OrdinalIgnoreCase))
            {
                if (trimmedSegments.Length > 5 && string.Equals("validatecvv", trimmedSegments[5], StringComparison.OrdinalIgnoreCase))
                {
                    statusCode = HttpStatusCode.NoContent;
                }
                else if (trimmedSegments.Length > 4)
                {
                    // Get PI
                    var foundPi = PaymentInstruments.SingleOrDefault(pi =>
                    {
                        return trimmedSegments[2] == pi.PaymentInstrumentAccountId
                               && trimmedSegments[4] == pi.PaymentInstrumentId;
                    });

                    if (foundPi == null)
                    {
                        statusCode = HttpStatusCode.NotFound;
                        responseContent = $"{{\"CorrelationId\":\"{Guid.NewGuid().ToString()}\",\"ErrorCode\":\"AccountPINotFound\",\"Message\":\"The account and payment instrument pair can not be found.\",\"Target\":\"accountId\"}}";
                    }
                    else
                    {
                        if (trimmedSegments.Length > 5 && string.Equals(trimmedSegments[5], "extendedView", StringComparison.OrdinalIgnoreCase))
                        {
                            foundPi.PaymentInstrumentDetails.RequiredChallenge = this.RequiredChallenges;
                        }

                        responseContent = JsonConvert.SerializeObject(foundPi);
                    }
                }
                else
                {
                    // List PI
                    var queryKvc = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(request.RequestUri.Query);
                    var billableAccountId = queryKvc.TryGetValue("billableAccountId", out var billableAccount) ? billableAccount.FirstOrDefault() : null;
                    if (billableAccountId != null)
                    {
                        this.IsBillingAccountIdSet = true;
                    }

                    if (trimmedSegments.Contains("emporg"))
                    {
                        responseContent = JsonConvert.SerializeObject(PaymentInstruments.Where(pi => pi.PaymentInstrumentAccountId == "Account001").ToArray());
                    }
                    else
                    {
                        var filteredPis = PaymentInstruments.Where(pi => trimmedSegments[2] == pi.PaymentInstrumentAccountId).ToArray();
                        responseContent = JsonConvert.SerializeObject(filteredPis);
                    }
                }
            }
            else if (trimmedSegments.Length >= 3 && string.Equals("paymentInstruments", trimmedSegments[2], StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals("Searchbyaccountnumber", trimmedSegments[3], StringComparison.OrdinalIgnoreCase))
                {
                    statusCode = HttpStatusCode.OK;
                    responseContent = $"{{\"result\": [{{\"id\": \"lchqggAAAAABAACA\",\"accountId\": \"Account001\"}},{{\"id\": \"lchqggAAAAABAACA\",\"accountId\": \"Account003\"}}]}}";
                }
                else if (trimmedSegments.Length > 3)
                {
                    var foundPi = PaymentInstruments.Last(pi =>
                    {
                        return trimmedSegments[3] == pi.PaymentInstrumentId;
                    });

                    if (foundPi == null)
                    {
                        statusCode = HttpStatusCode.NotFound;
                        responseContent = $"{{\"CorrelationId\":\"{Guid.NewGuid().ToString()}\",\"ErrorCode\":\"AccountPINotFound\",\"Message\":\"The account and payment instrument pair can not be found.\",\"Target\":\"accountId\"}}";
                    }
                    else
                    {
                        if (trimmedSegments.Length > 4 && string.Equals(trimmedSegments[4], "extendedView", StringComparison.OrdinalIgnoreCase))
                        {
                            foundPi.PaymentInstrumentDetails.RequiredChallenge = this.RequiredChallenges;
                        }

                        responseContent = JsonConvert.SerializeObject(foundPi);
                    }
                }
            }

            HttpResponseMessage responseMessage = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(
                    responseContent,
                    System.Text.Encoding.UTF8,
                    "application/json")
            };

            responseMessage.RequestMessage = request;

            return await Task.FromResult(responseMessage);
        }
    }
}