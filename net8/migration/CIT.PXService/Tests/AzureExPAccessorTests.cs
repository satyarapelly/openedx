// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Tracing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static Microsoft.Commerce.Payments.PXCommon.Flighting;

    [TestClass]
    public class AzureExPAccessorTests : TestBase
    {
        [TestInitialize]
        public void Startup()
        {
            PXSettings.AzureExPService.Responses.Clear();
        }

        [TestMethod]
        public async Task AzureExPAccessor_VerifyFeatureConfig()
        {
            // Arrange
            bool isVAProviderInitialized = PXSettings.AzureExPAccessor.InitializeVariantAssignmentProvider(File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestData\paymentexpprd_flight.blob")));

            // Act
            // Get feature config for two requests with differnt flight context
            FeatureConfig featureConfigForRequestOne = await PXSettings.AzureExPAccessor.GetExposableFeatures(null, EventTraceActivity.Empty);
            FeatureConfig featureConfigForRequestTwo = await PXSettings.AzureExPAccessor.GetExposableFeatures(new System.Collections.Generic.Dictionary<string, string> { { "country", "us" } }, EventTraceActivity.Empty);

            // Assert
            Assert.IsTrue(isVAProviderInitialized);

            // Verify feature config for first request
            Assert.IsNotNull(featureConfigForRequestOne);
            Assert.AreEqual(string.Empty, featureConfigForRequestOne.AssignmentContext);
            Assert.AreEqual(0, featureConfigForRequestOne.EnabledFeatures.Count);

            // Verify feature config for second request
            Assert.IsNotNull(featureConfigForRequestTwo);
            Assert.AreEqual("73077f2a-8f45:28524;", featureConfigForRequestTwo.AssignmentContext);
            Assert.AreEqual(1, featureConfigForRequestTwo.EnabledFeatures.Count);
            Assert.AreEqual("FeatureFlightOne", featureConfigForRequestTwo.EnabledFeatures[0]);
        }
    }
}
