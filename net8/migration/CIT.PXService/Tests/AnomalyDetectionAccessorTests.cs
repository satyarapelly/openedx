// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AnomalyDetectionAccessorTests : TestBase
    {
        [TestMethod]
        public void AnomalyDetectionAccessor_VerifyBlockedIds()
        {
            // Arrange
            string expiryDate = DateTime.UtcNow.AddSeconds(2).ToString("O");
            string blockedAccountContent = $@"AccountId,ExpiryTimestamp
43cbbbf0-e6c9-4c82-83ea-ee791f984be5,{expiryDate}
da3f7f83-8623-4a2e-948b-12915dd89ba1,{expiryDate}
34d24404-d3d3-46e5-9ee1-c70541ce05ae,{expiryDate}
";

            string blockedClientIPContent = $@"ClientIP,ExpiryTimestamp
10.1.2.110,{expiryDate}
10.5.2.150,{expiryDate}
10.7.2.170,{expiryDate}
";
            
            bool isDataInitialized = PXSettings.AnomalyDetectionAccessor.InitializeAnomalyDetectionResults(
                Encoding.UTF8.GetBytes(blockedAccountContent),
                Encoding.UTF8.GetBytes(blockedClientIPContent));

            // Act
            // Check whether account or clientIP is malicious or not
            bool blockedAccountResult1 = PXSettings.AnomalyDetectionAccessor.IsMaliciousAccountId("43cbbbf0-e6c9-4c82-83ea-ee791f984be5", EventTraceActivity.Empty);
            bool blockedAccountResult2 = PXSettings.AnomalyDetectionAccessor.IsMaliciousAccountId("da3f7f83-8623-4a2e-948b-12915dd89ba1", EventTraceActivity.Empty);
            bool blockedAccountResult3 = PXSettings.AnomalyDetectionAccessor.IsMaliciousAccountId("34d24404-d3d3-46e5-9ee1-c70541ce05ae", EventTraceActivity.Empty);
            bool blockedClientIPResult1 = PXSettings.AnomalyDetectionAccessor.IsMaliciousClientIP("10.1.2.110", EventTraceActivity.Empty);
            bool blockedClientIPResult2 = PXSettings.AnomalyDetectionAccessor.IsMaliciousClientIP("10.5.2.150", EventTraceActivity.Empty);
            bool blockedClientIPResult3 = PXSettings.AnomalyDetectionAccessor.IsMaliciousClientIP("10.7.2.170", EventTraceActivity.Empty);
            bool nonBlockedAccountResult = PXSettings.AnomalyDetectionAccessor.IsMaliciousAccountId("99d24404-d3d3-46e5-9ee1-c70541ce05ae", EventTraceActivity.Empty);
            bool nonBlockedClientIPResult = PXSettings.AnomalyDetectionAccessor.IsMaliciousClientIP("11.7.2.170", EventTraceActivity.Empty);
            bool nonBlockedEmptyAccountResult = PXSettings.AnomalyDetectionAccessor.IsMaliciousAccountId(null, EventTraceActivity.Empty);
            bool nonBlockedEmptyClientIPResult = PXSettings.AnomalyDetectionAccessor.IsMaliciousClientIP(null, EventTraceActivity.Empty);

            // Assert
            Assert.IsTrue(isDataInitialized);

            // Verify blockedIDs
            Assert.IsTrue(blockedAccountResult1, "Expected account is not marked as malicious");
            Assert.IsTrue(blockedAccountResult2, "Expected account is not marked as malicious");
            Assert.IsTrue(blockedAccountResult3, "Expected account is not marked as malicious");
            Assert.IsTrue(blockedClientIPResult1, "Expected clientIP is not marked as malicious");
            Assert.IsTrue(blockedClientIPResult2, "Expected clientIP is not marked as malicious");
            Assert.IsTrue(blockedClientIPResult3, "Expected clientIP is not marked as malicious");

            // Verify non blockedIDs
            Assert.IsFalse(nonBlockedAccountResult, "Valid account is incorrectly marked as malicious");
            Assert.IsFalse(nonBlockedClientIPResult, "Valid clientIP is incorrectly marked as malicious");
            Assert.IsFalse(nonBlockedEmptyAccountResult, "Empty account is incorrectly marked as malicious");
            Assert.IsFalse(nonBlockedEmptyClientIPResult, "Empty clientIP is incorrectly marked as malicious");
        }

        [TestMethod]
        public async Task AnomalyDetectionAccessor_VerifyBlockedIdsAfterExpiry()
        {
            // Arrange
            string expiryDateTime = DateTime.UtcNow.AddSeconds(2).ToString("O");
            string blockedAccountContent = $@"AccountId,ExpiryTimestamp
43cbbbf0-e6c9-4c82-83ea-ee791f984be5,{expiryDateTime}
da3f7f83-8623-4a2e-948b-12915dd89ba1,{expiryDateTime}
34d24404-d3d3-46e5-9ee1-c70541ce05ae,{expiryDateTime}
";

            string blockedClientIPContent = $@"ClientIP,ExpiryTimestamp
10.1.2.110,{expiryDateTime}
10.5.2.150,{expiryDateTime}
10.7.2.170,{expiryDateTime}
";

            bool isDataInitialized = PXSettings.AnomalyDetectionAccessor.InitializeAnomalyDetectionResults(
                Encoding.UTF8.GetBytes(blockedAccountContent),
                Encoding.UTF8.GetBytes(blockedClientIPContent));

            // Assert
            Assert.IsTrue(isDataInitialized);

            // Wait for the expiry
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Act
            // Check whether account or clientIP is malicious or not
            bool blockedAccountResult1 = PXSettings.AnomalyDetectionAccessor.IsMaliciousAccountId("43cbbbf0-e6c9-4c82-83ea-ee791f984be5", EventTraceActivity.Empty);
            bool blockedAccountResult2 = PXSettings.AnomalyDetectionAccessor.IsMaliciousAccountId("da3f7f83-8623-4a2e-948b-12915dd89ba1", EventTraceActivity.Empty);
            bool blockedAccountResult3 = PXSettings.AnomalyDetectionAccessor.IsMaliciousAccountId("34d24404-d3d3-46e5-9ee1-c70541ce05ae", EventTraceActivity.Empty);
            bool blockedClientIPResult1 = PXSettings.AnomalyDetectionAccessor.IsMaliciousClientIP("10.1.2.110", EventTraceActivity.Empty);
            bool blockedClientIPResult2 = PXSettings.AnomalyDetectionAccessor.IsMaliciousClientIP("10.5.2.150", EventTraceActivity.Empty);
            bool blockedClientIPResult3 = PXSettings.AnomalyDetectionAccessor.IsMaliciousClientIP("10.7.2.170", EventTraceActivity.Empty);
            bool nonBlockedAccountResult = PXSettings.AnomalyDetectionAccessor.IsMaliciousAccountId("99d24404-d3d3-46e5-9ee1-c70541ce05ae", EventTraceActivity.Empty);
            bool nonBlockedClientIPResult = PXSettings.AnomalyDetectionAccessor.IsMaliciousClientIP("11.7.2.170", EventTraceActivity.Empty);

            // Verify blockedIDs
            Assert.IsFalse(blockedAccountResult1, "Account is returned as malicious even after expiry");
            Assert.IsFalse(blockedAccountResult2, "Account is returned as malicious even after expiry");
            Assert.IsFalse(blockedAccountResult3, "Account is returned as malicious even after expiry");
            Assert.IsFalse(blockedClientIPResult1, "ClientIP is returned as malicious even after expiry");
            Assert.IsFalse(blockedClientIPResult2, "ClientIP is returned as malicious even after expiry");
            Assert.IsFalse(blockedClientIPResult3, "ClientIP is returned as malicious even after expiry");

            // Verify non blockedIDs
            Assert.IsFalse(nonBlockedAccountResult, "Valid account is incorrectly marked as malicious");
            Assert.IsFalse(nonBlockedClientIPResult, "Valid clientIP is incorrectly marked as malicious");
        }
    }
}
