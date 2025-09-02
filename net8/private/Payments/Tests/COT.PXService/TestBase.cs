// <copyright file="TestBase.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace COT.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Test.Common;
    using Tests.Common;
    using static Bond.Deserialize;
    using Common = Microsoft.Commerce.Payments.Common.Transaction;

    public class TestBase
    {
        private const string EnvironmentTypeName = "EnvironmentType";

        protected PXServiceClient ServiceClient { get; set; }

        public static EnvironmentType Current
        {
            get; private set;
        }

        public static void ReadRunSetting(TestContext testContext)
        {
            Current = (EnvironmentType)Enum.Parse(typeof(EnvironmentType), LoadParameter(EnvironmentTypeName, testContext), true);
        }

        private static string LoadParameter(string name, TestContext context, bool required = true)
        {
            string parameterValue = null;
            if (context.Properties.Contains(name))
            {
                parameterValue = context.Properties[name].ToString();
            }

            if (required && string.IsNullOrEmpty(parameterValue))
            {
                throw new MissingFieldException(string.Format("Parameter {0} is required in .runsettings and can't be empty ", name));
            }

            return parameterValue;
        }

        protected void Initialize()
        {
            string serviceEndpoint;
            string hostName = string.Empty;
            string resource = "https://paymentexperience-test.cp.microsoft-int.com/";
            string pmeAuthority = "https://login.windows.net/975f013f-7f24-47e8-a7d3-abc4752bf346/v2.0";
            string cotClientAppId = "a2b81e22-f9e9-436b-8bf8-f3d41fc2516e"; // application name: PX-MI-COT-INT

            switch (Current)
            {
                case EnvironmentType.OneBox:
                    cotClientAppId = "7033f9b1-b4e6-4d49-9bde-738d53c14ae9"; // application name: PX-COT-INT-PME
                    serviceEndpoint = @"http://localhost/pxservice/"; // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                    break;

                case EnvironmentType.IntWestUSPme:
                    serviceEndpoint = @"https://agw-px-int-wus-1.westus.cloudapp.azure.com/px/";
                    hostName = "paymentexperience-test.cp.microsoft-int.com";
                    break;

                case EnvironmentType.IntWestUS2Pme:
                    serviceEndpoint = @"https://agw-px-int-wus2-1.westus2.cloudapp.azure.com/px/";
                    hostName = "paymentexperience-test.cp.microsoft-int.com";
                    break;

                case EnvironmentType.PpeWestUSPme:
                    serviceEndpoint = @"https://paymentexperience-ppe-westus.azurewebsites.net/px/";
                    resource = "https://paymentexperience.cp.microsoft.com/";
                    break;

                case EnvironmentType.PpeEastUSPme:
                    serviceEndpoint = @"https://paymentexperience-ppe-eastus.azurewebsites.net/px/";
                    resource = "https://paymentexperience.cp.microsoft.com/";
                    break;

                case EnvironmentType.PpeEastUS2Pme:
                    serviceEndpoint = @"https://paymentexperience-ppe-eastus2.azurewebsites.net/px/";
                    resource = "https://paymentexperience.cp.microsoft.com/";
                    break;

                case EnvironmentType.PpeNorthCentralUSPme:
                    serviceEndpoint = @"https://paymentexperience-ppe-northcentralus.azurewebsites.net/px/";
                    resource = "https://paymentexperience.cp.microsoft.com/";
                    break;

                case EnvironmentType.PpeWestCentralUSPme:
                    serviceEndpoint = @"https://paymentexperience-ppe-westcentralus.azurewebsites.net/px/";
                    resource = "https://paymentexperience.cp.microsoft.com/";
                    break;

                case EnvironmentType.ProdCanaryCentralUSPme:
                    serviceEndpoint = @"https://paymentexperience-prod-centraluseuap.azurewebsites.net/px/";
                    resource = "https://paymentexperience.cp.microsoft.com/";
                    break;

                case EnvironmentType.ProdWestUS2Pme:
                    serviceEndpoint = @"https://paymentexperience-prod-westus2.azurewebsites.net/px/";
                    resource = "https://paymentexperience.cp.microsoft.com/";
                    break;

                case EnvironmentType.ProdWestUSPme:
                    serviceEndpoint = @"https://paymentexperience-prod-westus.azurewebsites.net/px/";
                    resource = "https://paymentexperience.cp.microsoft.com/";
                    break;

                case EnvironmentType.ProdEastUSPme:
                    serviceEndpoint = @"https://paymentexperience-prod-eastus.azurewebsites.net/px/";
                    resource = "https://paymentexperience.cp.microsoft.com/";
                    break;

                case EnvironmentType.ProdCentralUSPme:
                    serviceEndpoint = @"https://paymentexperience-prod-centralus.azurewebsites.net/px/";
                    resource = "https://paymentexperience.cp.microsoft.com/";
                    break;

                case EnvironmentType.ProdSouthCentralUSPme:
                    serviceEndpoint = @"https://paymentexperience-prod-southcentralus.azurewebsites.net/px/";
                    resource = "https://paymentexperience.cp.microsoft.com/";
                    break;

                case EnvironmentType.ProdNorthCentralUSPme:
                    serviceEndpoint = @"https://paymentexperience-prod-northcentralus.azurewebsites.net/px/";
                    resource = "https://paymentexperience.cp.microsoft.com/";
                    break;

                case EnvironmentType.ProdWestCentralUSPme:
                    serviceEndpoint = @"https://paymentexperience-prod-westcentralus.azurewebsites.net/px/";
                    resource = "https://paymentexperience.cp.microsoft.com/";
                    break;

                case EnvironmentType.ProdEastUS2Pme:
                    serviceEndpoint = @"https://paymentexperience-prod-eastus2.azurewebsites.net/px/";
                    resource = "https://paymentexperience.cp.microsoft.com/";
                    break;
                default:
                    throw new NotSupportedException(string.Format("Unsupported environment", Current));
            }

            ServiceClientSettings clientSettings = new ServiceClientSettings()
            {
                ServiceEndpoint = serviceEndpoint,
                HostName = hostName
            };

            if (Current != EnvironmentType.OneBox)
            {
                clientSettings.AadTokenProviders.Add(
                    Constants.AADClientType.PME,
                    new AadTokenProvider(
                        clientAppId: cotClientAppId,
                        resource: resource));
            }
            else
            {
                clientSettings.ClientCertificate = null;
                clientSettings.AadTokenProviders.Add(
                    Constants.AADClientType.PME,
                    new AadTokenProvider(
                        authority: pmeAuthority,
                        clientAppId: cotClientAppId,
                        resource: resource,
                        clientCert: clientSettings.ClientCertificate));
            }

            this.ServiceClient = new PXServiceClient(clientSettings);
        }

        /// <summary>
        /// Execute the service request
        /// </summary>
        /// <param name="url">The request url</param>
        /// <param name="responseVerification">The action to verify the response</param>
        protected void ExecuteRequest(string url, HttpMethod method, Common.TestContext context, object payload, Dictionary<string, string> requestHeaders, Action<HttpStatusCode, dynamic, WebHeaderCollection> responseVerification)
        {
            HttpStatusCode responseCode = HttpStatusCode.NotImplemented;
            dynamic result = null;
            WebHeaderCollection responseHeaders = null;
            responseCode = this.ExecuteRequest(url, method, context, payload, requestHeaders, out result, out responseHeaders);

            responseVerification(responseCode, result, responseHeaders);
        }

        /// <summary>
        /// Execute the service request
        /// </summary>
        /// <param name="url">The request url</param>
        /// <param name="responseVerification">The action to verify the response</param>
        protected void ExecuteRequest(
            string url,
            HttpMethod method,
            Common.TestContext context,
            object payload,
            Dictionary<string, string> requestHeaders,
            Action<HttpStatusCode, dynamic> responseVerification,
            string contentType = Constants.HeaderValues.JsonContent,
            Constants.AuthenticationType authType = Constants.AuthenticationType.AAD,
            Constants.AADClientType aadClientType = Constants.AADClientType.PME)
        {
            HttpStatusCode responseCode = HttpStatusCode.NotImplemented;
            dynamic result = null;
            WebHeaderCollection responseHeaders = null;
            responseCode = this.ExecuteRequest(
                url,
                method,
                context,
                payload,
                requestHeaders,
                out result,
                out responseHeaders,
                contentType,
                authType,
                aadClientType);
            try
            {
                responseVerification(responseCode, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred in TestBase.ExecuteRequest: {ex.Message}");
                Console.WriteLine($"Response in TestBase.ExecuteRequest is: {result}");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Execute the service request
        /// </summary>
        /// <param name="url">The request url</param>
        private HttpStatusCode ExecuteRequest(
            string url,
            HttpMethod method,
            Common.TestContext context,
            object payload,
            Dictionary<string, string> requestHeaders,
            out dynamic response,
            out WebHeaderCollection responseHeaders,
            string contentType = Constants.HeaderValues.JsonContent,
            Constants.AuthenticationType authType = Constants.AuthenticationType.AAD,
            Constants.AADClientType aadClientType = Constants.AADClientType.PME)
        {
            Assert.IsNotNull(this.ServiceClient);

            HttpStatusCode responseCode = HttpStatusCode.NotImplemented;
            dynamic outResponse = null;
            WebHeaderCollection outHeaders = null;
            string request = payload == null ? null : (contentType == Constants.HeaderValues.JsonContent ? JsonConvert.SerializeObject(payload) : payload.ToString());
            try
            {
                responseCode = this.ServiceClient.SendRequest(
                    url,
                    method.ToString(),
                    context,
                    request,
                    null,
                    requestHeaders,
                    new EventTraceActivity(Guid.NewGuid()),
                    out outResponse,
                    out outHeaders,
                    contentType,
                    authType,
                    aadClientType);
            }
            catch (Exception responseException)
            {
                Trace.TraceInformation("Response exception: {0}", responseException);
                Console.WriteLine($"responseException in TestBase.ExecuteRequest is: {responseException.ToString()}");
            }

            response = outResponse;
            responseHeaders = outHeaders;
            return responseCode;
        }
    }
}
