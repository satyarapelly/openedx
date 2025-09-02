// <copyright file="PXServiceCorsHandlerTest.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2020. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PXServiceCorsHandlerTest : TestBase
    {
        [DataRow("http://localhost:3000", HttpStatusCode.OK)] // DevSkim: ignore DS137138
        [DataRow("https://pidlsdktestportal.azurewebsites.net", HttpStatusCode.OK)]
        [DataRow("http://localhost:12345", HttpStatusCode.Forbidden)] // DevSkim: ignore DS137138
        [DataRow("https://somehttps.net", HttpStatusCode.Forbidden)]
        [DataRow("http://somehttp.dev", HttpStatusCode.Forbidden)] // DevSkim: ignore DS137138
        [DataTestMethod]
        public async Task PXServiceCorsHandler_TestCorsValidationWithOriginHeader(string origin, HttpStatusCode expectedStatusCode)
        {
            string urlToTest = "v7.0/Account001/addressDescriptions?country=us&partner=webblends";
            List<HttpMethod> methods = new List<HttpMethod> { HttpMethod.Options, HttpMethod.Get };

            foreach (HttpMethod method in methods)
            {
                // Prepare
                var req = new HttpRequestMessage(method, urlToTest);
                req.Headers.Add("Origin", origin);

                var response = await PXClient.SendAsync(req);

                // Assert
                Assert.AreEqual(expectedStatusCode, response.StatusCode);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Assert.IsTrue(response.Headers.Contains("Access-Control-Allow-Origin"));
                    Assert.AreEqual(origin, response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault());
                }
                else
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
                    Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Origin"));
                }
            }
        }

        [TestMethod]
        public async Task PXServiceCorsHandler_TestCorsValidationNoOriginHeader()
        {
            string urlToTest = "v7.0/Account001/addressDescriptions?country=us&partner=webblends";

            var req = new HttpRequestMessage(HttpMethod.Get, urlToTest);

            var response = await PXClient.SendAsync(req);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Origin"));
        }
    }
}
