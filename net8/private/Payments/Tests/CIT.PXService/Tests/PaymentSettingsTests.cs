// <copyright company="Microsoft Corporation">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class PaymentSettingsTests : TestBase
    {
        [DataRow("chrome", true)]
        [DataRow("edge", false)]
        [DataTestMethod]
        public async Task PaymentSettings_GetSettings(string browserType, bool usePidlUI)
        {
            // Arrange
            var requestBody = new ClientConfigData
            {
                BrowserType = browserType
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/settings?partner=northstarweb", new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert
            string contentStr = await result.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(contentStr);
            JToken usePidlUIToken = json.SelectToken("usePidlUI");
            Assert.AreEqual(usePidlUI, usePidlUIToken.Value<bool>());
        }
    }
}