// <copyright file="PartnerSettingsServiceMockResponseProvider.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Test.Common;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class PartnerSettingsServiceMockResponseProvider : IMockResponseProvider
    {
        public PartnerSettingsServiceMockResponseProvider()
        {
        }

        public static JObject SettingsByPartner
        {
            get
            {
                return JObject.Parse(File.ReadAllText(Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Mocks",
                        "PartnerSettings",
                        "PartnerSettingsByPartner.json")));
            }
        }

        // This functions help to get the mock data from the PSSTestSettingsById.json file.
        public static JArray SettingsById
        {
            get
            {
                return JArray.Parse(File.ReadAllText(Path.Combine(
                                        AppDomain.CurrentDomain.BaseDirectory,
                                        "Mocks",
                                        "PartnerSettings",
                                        "PSSTestSettingsById.json")));
            }
        }

        public string PSSResponse { get; set; }

        /// <summary>
        /// Retrieve the PSS mock response by its unique identifier 
        /// and return the response as a string containing the data of the inner array corresponding 
        /// to the paymentExperienceSettings of that identifier.
        /// </summary>
        /// <param name="pssSettingId">The unique identifier of the PSS mock response.</param>
        /// <returns>A string representing the data of the inner array associated with the paymentExperienceSettings of the specified identifier.</returns>
        public static string GetPSSMockResponseById(string pssSettingId)
        {
            var pssMockDataById = SettingsById.Where(pssSetting => pssSetting["id"].ToString() == pssSettingId);
            var pssMockResponse = pssMockDataById.FirstOrDefault()?["paymentExperienceSettings"];
            return JsonConvert.SerializeObject(pssMockResponse);
        }

        public void ResetDefaults()
        {
            this.PSSResponse = null;
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            // For SelfHosted env, MockServiceHandler sends request first to get the GetMatchedMockResponse instead of emulator controller
            // if the response from the GetMatchedMockResponse is null then Handler will send the request to emulator controller where it utilize the testScenarioHeaders or testContext
            // Returning null here to send the request to controller when the PSS scenarioHeader is present
            bool isPSSTestScenarioHeaderRequest = request.Headers.TryGetValues(Test.Common.Constants.HeaderValues.TestHeader, out var testHeaderValue) && testHeaderValue.FirstOrDefault().Contains(Constants.TestScenarios.PXPartnerSettings);

            if (isPSSTestScenarioHeaderRequest && WebHostingUtility.IsApplicationSelfHosted())
            {
                return null;
            }

            string responseContent = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            var trimmedSegments = request.RequestUri.Segments.Select(s => s.Trim(new char[] { '/' })).ToArray();

            if (trimmedSegments.Length > 2 && string.Equals(trimmedSegments[1], "partnersettings", StringComparison.OrdinalIgnoreCase))
            {
                string partnerName = trimmedSegments[2];

                // returns the PSS partner config from the mock file if the flight is exposed and config is present for partner
                if (IsPSSPartnerMockForDiffTestFlightExposed(request) && partnerName != null && SettingsByPartner.ContainsKey(partnerName.ToLower()))
                {
                    responseContent = JsonConvert.SerializeObject(SettingsByPartner[partnerName.ToLower()]);
                }
                else
                {
                    responseContent = "{\"add\":{\"template\":\"OnePage\",\"features\":{\"addressValidation\":{\"applicableMarkets\":[\"us\",\"ca\"]},\"hideElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"hideFirstAndLastNameForCompletePrerequisites\":true}]},\"singleMarketDirective\":{\"applicableMarkets\":[\"fr\",\"gb\"]}}},\"select\":{\"template\":\"selectpmbuttonlist\",\"features\":{\"paymentMethodGrouping\":{\"applicableMarkets\":[]}}},\"validateinstance\":{\"template\":\"defaultTemplate\"},\"handlepaymentchallenge\":{\"template\":\"defaultTemplate\"}}";
                }

                this.PSSResponse = responseContent;
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

        private static bool IsPSSPartnerMockForDiffTestFlightExposed(HttpRequestMessage request)
        {
            string xMSFlightValue = request.GetRequestHeader(Test.Common.Constants.HeaderValues.ExtendedFlightName);

            return xMSFlightValue != null && xMSFlightValue.Contains(Test.Common.Constants.FlightValues.PXUsePSSPartnerMockForDiffTest);
        }
    }
}