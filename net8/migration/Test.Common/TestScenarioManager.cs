// <copyright file="TestScenarioManager.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Commerce.Payments.Common.Transaction;

namespace Test.Common
{
    public class TestScenarioManager
    {
        private readonly Dictionary<string, TestScenario> testScenarios;
        private readonly string defaultTestScenario;

        public TestScenarioManager(string scenariosPath, string defaultTestScenario)
        {
            this.defaultTestScenario = defaultTestScenario;

            this.testScenarios = Directory
                .EnumerateFiles(scenariosPath)
                .Select(File.ReadAllText)
                .Select(content => JsonConvert.DeserializeObject<TestScenario>(content)!)
                .ToDictionary(ts => ts.Name, StringComparer.OrdinalIgnoreCase);
        }

        public HttpResponseData GetResponseContent(string apiName, TestContext testContext)
        {
            var matchedScenarios = testContext.ScenarioList
                .Where(s => testScenarios.ContainsKey(s))
                .ToList();

            if (matchedScenarios.Count == 0)
            {
                if (string.IsNullOrWhiteSpace(defaultTestScenario))
                {
                    return new HttpResponseData
                    {
                        StatusCode = 400,
                        Content = $"No matching scenario found. Provided: [{testContext.Scenarios}]"
                    };
                }

                matchedScenarios.Add(defaultTestScenario);
            }

            if (matchedScenarios.Count > 1)
            {
                return new HttpResponseData
                {
                    StatusCode = 400,
                    Content = $"Multiple matching scenarios found. Provided: [{testContext.Scenarios}]"
                };
            }

            var scenarioName = matchedScenarios[0];
            if (!testScenarios.TryGetValue(scenarioName, out var scenario) ||
                !scenario.ResponsesPerApiCall.TryGetValue(apiName, out var response))
            {
                return new HttpResponseData
                {
                    StatusCode = 404,
                    Content = $"No response found for API '{apiName}' in scenario '{scenarioName}'."
                };
            }

            return new HttpResponseData
            {
                StatusCode = (int)response.StatusCode,
                Content = response.Content?.ToString(),
                ContentType = "application/json"
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
