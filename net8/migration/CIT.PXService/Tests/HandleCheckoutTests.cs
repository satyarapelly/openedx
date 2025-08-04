// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2022. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class HandleCheckoutTests : TestBase
    {
        /// <summary>
        /// 
        /// </summary>
        [Ignore]
        [TestMethod]
        public async Task HandleCheckout()
        {
            // Todo: enable the test case after add mock.
            string checkoutId = "d5553094-7b64-4267-8e33-56fe07f59597";
            string partner = "teams";
            var emptyRequestBody = new
            {
                paymentMethodFamily = "paymentMethodFamily",
                paymentMethodType = "paymentMethodType"
            };

            // Step 1: PIDLSDK HandleCheckout will call PX to show PI form
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl($"/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId={checkoutId}&partner={partner}"));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidls = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());

            // Credit card pidl should returns with 4 pidls
            Assert.AreEqual(4, pidls.Count);

            // Verify the identity, the credit card form is shown
            Assert.AreEqual("checkout", pidls[0].Identity["description_type"]);
            Assert.AreEqual("RenderPidlPage", pidls[0].Identity["operation"]);
            Assert.AreEqual("credit_card", pidls[0].Identity["family"]);

            // Step 2: After user enters the credit card information, post card information to 
            pxResponse = await PXClient.PostAsync(
               GetPXServiceUrl($"/v7.0/checkoutsEx/{checkoutId}/charge?partner={partner}"),
                new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, "application/json"));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);
            pidls = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual(1, pidls.Count);
            Assert.AreEqual("ReturnContext", pidls[0].ClientAction.ActionType.ToString());
            dynamic paymentRequest = pidls[0].ClientAction.Context;
            Assert.AreEqual(checkoutId, paymentRequest.id.ToString());
        }
    }
}