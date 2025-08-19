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
    using System.Web.Http;
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
            // Test header validation
            List<string> matchedTestScenarios = testContext.ScenarioList.Where(s => this.testScenarios.ContainsKey(s)).ToList();

            if (matchedTestScenarios.Count == 0)
            {
                if (string.IsNullOrWhiteSpace(this.defaultTestScenario))
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(string.Format("There is no matched scenario. Test header: [{0}]", testContext.Scenarios))
                    });
                }
                else
                {
                    // Add default header implictly 
                    matchedTestScenarios.Add(this.defaultTestScenario);
                }
            }

            if (matchedTestScenarios.Count > 1)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(string.Format("There are more than one matched scenario. Test header: [{0}]", testContext.Scenarios))
                });
            }

            // Create Response
            string testScenarioName = matchedTestScenarios[0];
            ApiResponse response = null;
            TestScenario ts = this.testScenarios[testScenarioName];
            if (ts.ResponsesPerApiCall.ContainsKey(apiName))
            {
                response = ts.ResponsesPerApiCall[apiName];
            }

            if (response.StatusCode == HttpStatusCode.BadRequest 
                || response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new HttpResponseException(new HttpResponseMessage(response.StatusCode)
                {
                    Content = new StringContent(response.Content.ToString(), Encoding.UTF8, Constants.HeaderValues.JsonContent)
                });
            }

            if (response.StatusCode.Equals(HttpStatusCode.NotFound))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(response.Content.ToString(), Encoding.UTF8, Constants.HeaderValues.JsonContent)
                };
            }

            if (response.StatusCode.Equals(HttpStatusCode.NoContent))
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            if (response.StatusCode.Equals(HttpStatusCode.ServiceUnavailable))
            {
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response.Content.ToString(), Encoding.UTF8, Constants.HeaderValues.JsonContent)
            };
        }
    }
}
