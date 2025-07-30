// <copyright file="EmulatorBaseController.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Web.Http;
    using Common.Transaction;
    using Common.Web;
    using Test.Common;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class EmulatorBaseController : ApiController
    {
        private readonly string testScenarioManagerName;
        private readonly string defaultTestScenario;

        public EmulatorBaseController(string testScenarioManagerName, string defaultTestScenario = null)
        {
            this.testScenarioManagerName = testScenarioManagerName;
            this.defaultTestScenario = defaultTestScenario;
            this.PlaceholderReplacements = new Dictionary<string, string>()
            {
                { Constants.Placeholders.AddressId, Guid.NewGuid().ToString() }
            };
        }

        protected Dictionary<string, string> PlaceholderReplacements { get; }

        protected TestScenarioManager TestScenarioManager
        {
            get
            {
                return this.Configuration.GetTestScenarioManager(this.testScenarioManagerName);
            }
        }

        protected virtual HttpResponseMessage GetResponse(string apiName)
        {
            TestContext testContext = null;

            if (this.Request.TryGetTestContext(out testContext))
            {
                return TestScenarioManager.GetResponse(apiName, testContext);
            }
            else if (!string.IsNullOrEmpty(this.defaultTestScenario))
            {
                // Return response from default scenario
                testContext = new TestContext($"DependencyEmulator.{this.testScenarioManagerName}", DateTime.UtcNow, this.defaultTestScenario);
                
                return TestScenarioManager.GetResponse(apiName, testContext);
            }
            else
            {
                // If the flow entering else that means the test context is not available and default test scenario is not provided
                // and might return error from TestScenarioManager class, needs to be handled from devloper
                return TestScenarioManager.GetResponse(apiName, testContext);
            }
        }

        protected HttpResponseMessage ReplacePlaceholders(HttpResponseMessage response)
        {
            var responseContent = response?.Content?.ReadAsStringAsync().Result;

            if (string.IsNullOrEmpty(responseContent))
            {
                return response;
            }

            foreach (var kvp in this.PlaceholderReplacements)
            {
                responseContent = responseContent.Replace(kvp.Key, kvp.Value ?? string.Empty);
            }

            response.Content = new StringContent(responseContent, System.Text.Encoding.UTF8, Test.Common.Constants.HeaderValues.JsonContent); // lgtm[cs/web/xss] Suppressing Semmle warning

            return response;
        }
    }
}