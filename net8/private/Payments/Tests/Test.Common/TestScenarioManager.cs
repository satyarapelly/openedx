// <copyright file="TestScenarioManager.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Test.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using Microsoft.Commerce.Payments.Common.Transaction;
    using Newtonsoft.Json;

    /// <summary>
    /// TestScenarioManager manages all the supported test scenarios
    /// The name of the test scenarios follows the rule px.pi_type.expected_result_of_the_test_scenario
    /// eg:"px.pims.nsm.resume.error.invalidchallengecode" means add a nonsimmobi pi and the add pi flow fails at resume with server error code "invalidchallengecode"
    /// A test scenario object stores the responses of all api calls in one test scenario. 
    /// eg: nsm add flow may includes add, get, resume pi call responses, while credit card add flow may only contains add pi call response.
    /// more details can be refered to the files in TestScenarios folder. Each file matches to a test scenario.
    /// </summary>
    public class TestScenarioManager
    {
        // The key is the name of test scenario
        private readonly Dictionary<string, TestScenario> testScenarios = new Dictionary<string, TestScenario>(StringComparer.OrdinalIgnoreCase);

        private string defaultTestScenario;

        public TestScenarioManager(string scenariosPath, string defaultTestScenario)
        {
            this.defaultTestScenario = defaultTestScenario;

            DirectoryInfo dir = new DirectoryInfo(scenariosPath);
            IEnumerable<FileInfo> files = dir.GetFiles();
            this.testScenarios = Directory.EnumerateFiles(scenariosPath)
                .Select(f => File.ReadAllText(f))
                .Select(content => JsonConvert.DeserializeObject<TestScenario>(content))
                .ToDictionary(ts => ts.Name);
        }

        public HttpResponseMessage GetResponse(string apiName, TestContext testContext)
        {
            var result = this.GetResponseContent(apiName, testContext);

            var message = new HttpResponseMessage((HttpStatusCode)result.StatusCode);

            if (!string.IsNullOrEmpty(result.Content))
            {
                message.Content = new StringContent(result.Content, Encoding.UTF8, result.ContentType ?? Constants.HeaderValues.JsonContent);
            }

            return message;
        }

        public HttpResponseData GetResponseContent(string apiName, TestContext testContext)
        {
            // Test header validation
            List<string> matchedTestScenarios = testContext.ScenarioList.Where(s => this.testScenarios.ContainsKey(s)).ToList();

            if (matchedTestScenarios.Count == 0)
            {
                if (string.IsNullOrWhiteSpace(this.defaultTestScenario))
                {
                    return new HttpResponseData
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Content = string.Format("There is no matched scenario. Test header: [{0}]", testContext.Scenarios),
                        ContentType = Constants.HeaderValues.JsonContent,
                    };
                }
                else
                {
                    // Add default header implictly
                    matchedTestScenarios.Add(this.defaultTestScenario);
                }
            }

            if (matchedTestScenarios.Count > 1)
            {
                return new HttpResponseData
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Content = string.Format("There are more than one matched scenario. Test header: [{0}]", testContext.Scenarios),
                    ContentType = Constants.HeaderValues.JsonContent,
                };
            }

            // Create Response
            string testScenarioName = matchedTestScenarios[0];
            TestScenario ts = this.testScenarios[testScenarioName];

            if (!ts.ResponsesPerApiCall.TryGetValue(apiName, out ApiResponse response))
            {
                return new HttpResponseData
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    ContentType = Constants.HeaderValues.JsonContent,
                };
            }

            return new HttpResponseData
            {
                StatusCode = (int)response.StatusCode,
                Content = response.Content?.ToString(),
                ContentType = Constants.HeaderValues.JsonContent,
            };
        }
    }

    public class HttpResponseData
    {
        public int StatusCode { get; set; }
        public string? Content { get; set; }
        public string? ContentType { get; set; } = "application/json";
    }
}
