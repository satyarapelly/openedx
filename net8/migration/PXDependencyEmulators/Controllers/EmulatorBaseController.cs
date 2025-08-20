// <copyright file="EmulatorBaseController.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.Common.Transaction;
    using Microsoft.Extensions.DependencyInjection;
    using Test.Common;
    using Test.Common.Extensions;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class EmulatorBaseController : ControllerBase
    {
        private readonly string testScenarioManagerName;
        private readonly string? defaultTestScenario;

        public EmulatorBaseController(string testScenarioManagerName, string? defaultTestScenario = null)
        {
            this.testScenarioManagerName = testScenarioManagerName;
            this.defaultTestScenario = defaultTestScenario;
            this.PlaceholderReplacements = new Dictionary<string, string>()
            {
                { Constants.Placeholders.AddressId, Guid.NewGuid().ToString() }
            };
        }

        public TestScenarioManager TestScenarioManager
        {
            get
            {
                var managers = HttpContext.RequestServices.GetRequiredService<Dictionary<string, TestScenarioManager>>();
                return managers[this.testScenarioManagerName];
            }
        }

        protected Dictionary<string, string> PlaceholderReplacements { get; }

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

            var result = this.TestScenarioManager.GetResponseContent(apiName, testContext);
            var responseContent = ReplacePlaceholders(result.Content);

            HttpContext.Response.StatusCode = result.StatusCode;
            HttpContext.Response.ContentType = result.ContentType ?? "application/json";

            if (!string.IsNullOrEmpty(responseContent))
            {
                await HttpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(responseContent));
            }

            return new EmptyResult();
        }

        protected virtual HttpResponseMessage GetResponse(string apiName)
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
                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent("TestContext not found.")
                    };
                }
            }

            var result = this.TestScenarioManager.GetResponseContent(apiName, testContext);
            var responseContent = ReplacePlaceholders(result.Content);

            var message = new HttpResponseMessage((HttpStatusCode)result.StatusCode);
            if (!string.IsNullOrEmpty(responseContent))
            {
                message.Content = new StringContent(responseContent, Encoding.UTF8, result.ContentType ?? "application/json");
            }

            return message;
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

        protected HttpResponseMessage ReplacePlaceholders(HttpResponseMessage response)
        {
            if (response?.Content == null)
            {
                return response;
            }

            var content = response.Content.ReadAsStringAsync().Result;
            content = ReplacePlaceholders(content);
            response.Content = new StringContent(content, Encoding.UTF8, response.Content.Headers.ContentType?.MediaType ?? "application/json");
            return response;
        }
    }
}