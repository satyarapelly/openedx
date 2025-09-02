// <copyright file="TestRequestContent.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService.ApiSurface.Diff
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Loads and stores important data from the Mock PIMS scenario files
    /// </summary>
    public class TestRequestContent
    {
        private static JObject instance;

        public TestRequestContent(string pathToscenarioDetails)
        {
            this.ExpectedStatus = new List<string>() { null, null, null, null, null, null, null };
            string scenarioText = File.ReadAllText(pathToscenarioDetails);
            JObject scenario = JObject.Parse(scenarioText);

            JToken addPI = scenario.SelectToken("$.responsesPerApiCall.addPI");
            JToken getPI = scenario.SelectToken("$.responsesPerApiCall.getPI");
            JToken resumePI = scenario.SelectToken("$.responsesPerApiCall.resumePI");
            
            // IssuerService 
            JToken apply = scenario.SelectToken("$.responsesPerApiCall.apply");
            JToken applyEligibility = scenario.SelectToken("$.responsesPerApiCall.applyEligibility");
            JToken initialize = scenario.SelectToken("$.responsesPerApiCall.initialize");

            this.Name = scenario["testScenarioName"].Value<string>();
            this.Countries = Instance[this.Name]["countries"].Values<string>().ToList();
            this.Partners = Instance[this.Name]["partners"].Values<string>().ToList();
            this.Body = Instance[this.Name]["body"].ToString(Formatting.None);

            this.ExpectedStatus[(int)PIState.Add] = (addPI == null) ? null : addPI["statusCode"].Value<string>();
            this.ExpectedStatus[(int)PIState.Get] = (getPI == null) ? null : getPI["statusCode"].Value<string>();
            this.ExpectedStatus[(int)PIState.Resume] = (resumePI == null) ? null : resumePI["statusCode"].Value<string>();

            // IssuerService 
            this.ExpectedStatus[(int)PIState.IssuerServiceApply] = (apply == null) ? null : apply["statusCode"].Value<string>();
            this.ExpectedStatus[(int)PIState.IssuerServiceApplyEligibility] = (applyEligibility == null) ? null : applyEligibility["statusCode"].Value<string>();
            this.ExpectedStatus[(int)PIState.IssuerServiceInitialize] = (initialize == null) ? null : initialize["statusCode"].Value<string>();
        }

        public string Body { get; private set; }

        public List<string> Countries { get; private set; }

        public List<string> ExpectedStatus { get; private set; }

        public string Name { get; private set; }

        public List<string> Partners { get; private set; }

        private static JObject Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = JObject.Parse(File.ReadAllText(".\\DiffTest\\ConfigFiles\\scenarios.json"));
                }

                return instance;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "{{name: \"{0}\", addPI: {1}, getPI: {2}, resumePI: {3}}}",
                this.Name,
                this.ExpectedStatus[(int)PIState.Add],
                this.ExpectedStatus[(int)PIState.Get],
                this.ExpectedStatus[(int)PIState.Resume]);
        }
    }
}
