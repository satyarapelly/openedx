// <copyright file="RiskServiceMockResponseProvider.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Test.Common;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class RiskServiceMockResponseProvider : IMockResponseProvider
    {
        static RiskServiceMockResponseProvider()
        {
            var riskEligibilityCheckApprovedJson = File.ReadAllText(
                 Path.Combine(
                     AppDomain.CurrentDomain.BaseDirectory,
                     "TestScenarios",
                     "Risk",
                     "px.risk.approved.success.json"));

            RiskEligibilityCheckApprovedResponse = JsonConvert.DeserializeObject<TestScenario>(riskEligibilityCheckApprovedJson);

            var riskEligibilityCheckRejectedJson = File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "TestScenarios",
                    "Risk",
                    "px.risk.rejected.success.json"));

            RiskEligibilityCheckRejectedResponse = JsonConvert.DeserializeObject<TestScenario>(riskEligibilityCheckRejectedJson);

            var riskEligibilityCheckFailedJson = File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "TestScenarios",
                    "Risk",
                    "px.risk.badrequest.failed.json"));

            RiskEligibilityCheckFailedResponse = JsonConvert.DeserializeObject<TestScenario>(riskEligibilityCheckFailedJson);

            var riskEligibilityCheckServerErrorJson = File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "TestScenarios",
                    "Risk",
                    "px.risk.servererror.failed.json"));

            RiskEligibilityCheckServerErrorResponse = JsonConvert.DeserializeObject<TestScenario>(riskEligibilityCheckServerErrorJson);
        }

        public static TestScenario RiskEligibilityCheckApprovedResponse { get; private set; }

        public static TestScenario RiskEligibilityCheckRejectedResponse { get; private set; }

        public static TestScenario RiskEligibilityCheckFailedResponse { get; private set; }

        public static TestScenario RiskEligibilityCheckServerErrorResponse { get; private set; }

        public string TestScenario { get; set; }

        public void ResetDefaults()
        {
            this.TestScenario = null;
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            string responseContent = "{\"payment_info\":[{\"paymentMethodType\":\"visa\",\"paymentMethodFamily\":\"credit_card\",\"allowed\":true},{\"paymentMethodType\":\"amex\",\"paymentMethodFamily\":\"credit_card\",\"allowed\":true},{\"paymentMethodType\":\"mc\",\"paymentMethodFamily\":\"credit_card\",\"allowed\":true},{\"paymentMethodType\":\"paypal\",\"paymentMethodFamily\":\"ewallet\",\"allowed\":true},{\"paymentMethodType\":\"alipay_billing_agreement\",\"paymentMethodFamily\":\"ewallet\",\"allowed\":true},{\"paymentMethodType\":\"stored_value\",\"paymentMethodFamily\":\"ewallet\",\"allowed\":true},{\"paymentMethodType\":\"monetary_commitment\",\"paymentMethodFamily\":\"ewallet\",\"allowed\":true},{\"paymentMethodType\":\"commercial_monetary_commit\",\"paymentMethodFamily\":\"ewallet\",\"allowed\":true},{\"paymentMethodType\":\"check\",\"paymentMethodFamily\":\"offline_bank_transfer\",\"allowed\":true},{\"paymentMethodType\":\"ea_check\",\"paymentMethodFamily\":\"offline_bank_transfer\",\"allowed\":true},{\"paymentMethodType\":\"sandbox_check\",\"paymentMethodFamily\":\"offline_bank_transfer\",\"allowed\":true},{\"paymentMethodType\":\"enterprise_agreement\",\"paymentMethodFamily\":\"virtual\",\"allowed\":true},{\"paymentMethodType\":\"invoice\",\"paymentMethodFamily\":\"virtual\",\"allowed\":true}]}";

            if (request.RequestUri.ToString().Contains("risk-evaluation"))
            {
                responseContent = JsonConvert.SerializeObject(RiskEligibilityCheckApprovedResponse.ResponsesPerApiCall[Constants.RiskApiName.RiskEvaluation].Content);

                if (!string.IsNullOrEmpty(this.TestScenario))
                {
                    if (this.TestScenario.Contains(Constants.DefaultTestScenarios.RiskEmulator))
                    {
                        responseContent = JsonConvert.SerializeObject(RiskEligibilityCheckApprovedResponse.ResponsesPerApiCall[Constants.RiskApiName.RiskEvaluation].Content);
                    }
                    else if (this.TestScenario.Contains(Constants.TestScenarios.PXRiskRejectedSuccess))
                    {
                        responseContent = JsonConvert.SerializeObject(RiskEligibilityCheckRejectedResponse.ResponsesPerApiCall[Constants.RiskApiName.RiskEvaluation].Content);
                    }
                    else if (this.TestScenario.Contains(Constants.TestScenarios.PXRiskBadRequestFailed))
                    {
                        statusCode = HttpStatusCode.BadRequest;
                        responseContent = JsonConvert.SerializeObject(RiskEligibilityCheckFailedResponse.ResponsesPerApiCall[Constants.RiskApiName.RiskEvaluation].Content);
                    }
                    else if (this.TestScenario.Contains(Constants.TestScenarios.PXRiskServerErrorFailed))
                    {
                        statusCode = HttpStatusCode.InternalServerError;
                        responseContent = JsonConvert.SerializeObject(RiskEligibilityCheckServerErrorResponse.ResponsesPerApiCall[Constants.RiskApiName.RiskEvaluation].Content);
                    }
                }
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