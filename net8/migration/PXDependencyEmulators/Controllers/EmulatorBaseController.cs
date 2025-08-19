// <copyright file="EmulatorBaseController.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.Common.Transaction;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Test.Common;
    using Test.Common.Extensions;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class EmulatorBaseController : ControllerBase
    {
        private readonly TestScenarioManager testScenarioManager;
        private readonly string testScenarioManagerName;
        private readonly string? defaultTestScenario;

        public EmulatorBaseController(
            TestScenarioManager testScenarioManager,
            string testScenarioManagerName,
            string? defaultTestScenario = null)
        {
            this.testScenarioManager = testScenarioManager;
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

        protected async Task<IActionResult> GetResponseAsync(string apiName)
        {
            if (!HttpContext.TryGetTestContext(out var testContext))
            {
                if (!string.IsNullOrEmpty(this.defaultTestScenario))
                {
                    testContext = new TestContext(
                        $"DependencyEmulator.{this.testScenarioManagerName}",
                        DateTime.UtcNow,
                        this.defaultTestScenario);
                }
                else
                {
                    return BadRequest("TestContext not found.");
                }
            }

            var result = this.testScenarioManager.GetResponseContent(apiName, testContext);
            var responseContent = ReplacePlaceholders(result.Content);

            HttpContext.Response.StatusCode = result.StatusCode;
            HttpContext.Response.ContentType = result.ContentType ?? "application/json";

            if (!string.IsNullOrEmpty(responseContent))
            {
                await HttpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(responseContent));
            }

            return new EmptyResult();
        }

        protected string ReplacePlaceholders(string? responseContent)
        {
            if (string.IsNullOrEmpty(responseContent))
            {
                return responseContent ?? string.Empty;
            }

            foreach (var kvp in this.PlaceholderReplacements)
            {
                responseContent = responseContent.Replace(kvp.Key, kvp.Value ?? string.Empty);
            }

            return responseContent;
        }
    }
}