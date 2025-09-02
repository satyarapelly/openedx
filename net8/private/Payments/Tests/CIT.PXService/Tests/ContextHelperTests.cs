// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ContextHelperTests : TestBase
    {
        /// <summary>
        /// Verify Pifd context group
        /// </summary>
        [TestMethod]
        public void ContextHelperAddsPifdBaseUrlToServerContext()
        {
            // Act
            Dictionary<string, object> actualContext = null;
            ContextHelper.GetServiceContext(new SelfHostedPXServiceCore.Mocks.PXServiceSettings(), ref actualContext);

            // Assert
            var expectedContextGroup = new Dictionary<string, string>()
            {
                { "baseUrl", "https://pifd.cp.microsoft-int.com/V6.0" }
            };

            this.AssertContextGroup(expectedContextGroup, actualContext, "Pifd");
        }

        /// <summary>
        /// Verify DeviceInfo context group
        /// </summary>
        [TestMethod]
        public void ContextHelperAddsDeviceInfoToClientContext()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl("/someUrl"));
            request.Headers.Add("x-ms-clientcontext-encoding", "base64");

            // Base64 encoded values of: ipAddress=131.107.147.243,deviceId=1234
            request.Headers.Add("x-ms-deviceinfo", "ipAddress=MTMxLjEwNy4xNDcuMjQz,deviceId=MTIzNA==");

            // Act
            Dictionary<string, object> actualContext = null;
            ContextHelper.GetClientContext(request, ref actualContext);

            // Assert
            var expectedContextGroup = new Dictionary<string, string>()
            {
                { "ipAddress", "131.107.147.243" },
                { "deviceId", "1234" }
            };

            this.AssertContextGroup(expectedContextGroup, actualContext, "DeviceInfo");
        }

        /// <summary>
        /// Verify AuthInfo group
        /// </summary>
        [TestMethod]
        public void ContextHelperAddsAuthInfoToContext()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl("/someUrl"));
            request.Headers.Add("x-ms-clientcontext-encoding", "base64");

            // Base64 encoded values of: type=AAD,context=me
            request.Headers.Add("x-ms-authinfo", "type=QUFE,context=bWU=");

            // Act
            Dictionary<string, object> actualContext = null;
            ContextHelper.GetClientContext(request, ref actualContext);

            // Assert
            var expectedContextGroup = new Dictionary<string, string>()
            {
                { "type", "AAD" },
                { "context", "me" }
            };

            this.AssertContextGroup(expectedContextGroup, actualContext, "AuthInfo");
        }

        /// <summary>
        /// Verify MsaProfile group
        /// </summary>
        [TestMethod]
        public void ContextHelperAddsMsaProfileToContext()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl("/someUrl"));
            request.Headers.Add("x-ms-clientcontext-encoding", "base64");

            // Base64 encoded values of: PUID=1234,emailAddress=foo@bar.com,firstName=foo,lastName=bar
            request.Headers.Add("x-ms-msaprofile", "PUID=MTIzNA==,emailAddress=Zm9vQGJhci5jb20=,firstName=Zm9v,lastName=YmFy");

            // Act
            Dictionary<string, object> actualContext = null;
            ContextHelper.GetClientContext(request, ref actualContext);

            // Assert
            var expectedContextGroup = new Dictionary<string, string>()
            {
                { "PUID", "1234" },
                { "emailAddress", "foo@bar.com" },
                { "firstName", "foo" },
                { "lastName", "bar" }
            };

            this.AssertContextGroup(expectedContextGroup, actualContext, "MsaProfile");
        }

        [TestMethod]
        public void ContextHelperWorksWithoutBase64Encoding()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl("/someUrl"));
            request.Headers.Add("x-ms-authinfo", "type=AAD,context=me");

            // Act
            Dictionary<string, object> actualContext = null;
            ContextHelper.GetClientContext(request, ref actualContext);

            // Assert
            var expectedContextGroup = new Dictionary<string, string>()
            {
                { "type", "AAD" },
                { "context", "me" }
            };

            this.AssertContextGroup(expectedContextGroup, actualContext, "AuthInfo");
        }

        [TestMethod]
        public void ContextHelperHashesXBoxDeviceIdWhenAddingToClientContext()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl("/someUrl"));
            request.Headers.Add("x-ms-clientcontext-encoding", "base64");

            // Base64 encoded values of: ipAddress=131.107.147.243,xboxLiveDeviceId=1234567890123456
            request.Headers.Add("x-ms-deviceinfo", "ipAddress=MTMxLjEwNy4xNDcuMjQz,xboxLiveDeviceId=MTIzNDU2Nzg5MDEyMzQ1Ng==");

            // Act
            Dictionary<string, object> actualContext = null;
            ContextHelper.GetClientContext(request, ref actualContext);

            // Assert
            var expectedContextGroup = new Dictionary<string, string>()
            {
                { "ipAddress", "131.107.147.243" },
                { "xboxLiveDeviceId", "5034FF5DD37CF15F748DD055D39FA886" } // Hash of 1234567890123456 
            };

            this.AssertContextGroup(expectedContextGroup, actualContext, "DeviceInfo");
        }

        [TestMethod]
        public void ContextHelperAddsToExistingContext()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl("/someUrl"));
            request.Headers.Add("x-ms-authinfo", "type=AAD,context=me");

            // Act
            Dictionary<string, object> actualContext = null;
            ContextHelper.GetServiceContext(new SelfHostedPXServiceCore.Mocks.PXServiceSettings(), ref actualContext);
            ContextHelper.GetClientContext(request, ref actualContext);

            // Assert
            var expectedAuthInfoContextGroup = new Dictionary<string, string>()
            {
                { "type", "AAD" },
                { "context", "me" }
            };

            var expectedPifdContextGroup = new Dictionary<string, string>()
            {
                { "baseUrl", "https://pifd.cp.microsoft-int.com/V6.0" }
            };

            this.AssertContextGroup(expectedAuthInfoContextGroup, actualContext, "AuthInfo");
            this.AssertContextGroup(expectedPifdContextGroup, actualContext, "Pifd");
        }

        [TestMethod]
        public void ContextHelperAddsToExistingContextGroup()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl("/someUrl"));
            request.Headers.Add("x-ms-authinfo", "type=AAD,context=me");

            // Act
            Dictionary<string, object> actualContext = new Dictionary<string, object>()
            {
                { "AuthInfo", new Dictionary<string, string>()
                    {
                        { "SomeExistingKey", "SomeExistingValue" }
                    }
                }
            };

            ContextHelper.GetClientContext(request, ref actualContext);

            // Assert
            var expectedContextGroup = new Dictionary<string, string>()
            {
                { "SomeExistingKey", "SomeExistingValue" },
                { "type", "AAD" },
                { "context", "me" }
            };

            this.AssertContextGroup(expectedContextGroup, actualContext, "AuthInfo");
        }

        [TestMethod]
        public void ContextHelperTryGetWorksWithValidPath()
        {
            // Arrange
            var request = GetRequestWithAllTypicalContextHeaders();

            // Act
            Dictionary<string, object> actualContext = null;
            ContextHelper.GetServiceContext(new SelfHostedPXServiceCore.Mocks.PXServiceSettings(), ref actualContext);
            ContextHelper.GetClientContext(request, ref actualContext);

            // Assert
            Assert.AreEqual("https://pifd.cp.microsoft-int.com/V6.0", ContextHelper.TryGetContextValue(actualContext, "Pifd.baseUrl"));
            Assert.AreEqual("AAD", ContextHelper.TryGetContextValue(actualContext, "AuthInfo.type"));
            Assert.AreEqual("131.107.147.243", ContextHelper.TryGetContextValue(actualContext, "DeviceInfo.ipAddress"));
            Assert.AreEqual("foo@bar.com", ContextHelper.TryGetContextValue(actualContext, "MsaProfile.emailAddress"));
        }

        [TestMethod]
        public void ContextHelperTryGetIsCaseInsensitive()
        {
            // Arrange
            var request = GetRequestWithAllTypicalContextHeaders();

            // Act
            Dictionary<string, object> actualContext = null;
            ContextHelper.GetServiceContext(new SelfHostedPXServiceCore.Mocks.PXServiceSettings(), ref actualContext);
            ContextHelper.GetClientContext(request, ref actualContext);

            // Assert
            Assert.AreEqual("https://pifd.cp.microsoft-int.com/V6.0", ContextHelper.TryGetContextValue(actualContext, "PiFD.BASEUrl"));
        }

        [TestMethod]
        public void ContextHelperTryGetReturnsNullWhenContextValueIsNotFound()
        {
            // Arrange
            var request = GetRequestWithAllTypicalContextHeaders();

            // Act
            Dictionary<string, object> actualContext = null;
            ContextHelper.GetServiceContext(new SelfHostedPXServiceCore.Mocks.PXServiceSettings(), ref actualContext);
            ContextHelper.GetClientContext(request, ref actualContext);

            // Assert
            Assert.IsNull(ContextHelper.TryGetContextValue(actualContext, "Pifd.nonExistentValue"));
        }

        [TestMethod]
        public void ContextHelperTryGetReturnsNullWhenContextGroupIsNotFound()
        {
            // Arrange
            var request = GetRequestWithAllTypicalContextHeaders();

            // Act
            Dictionary<string, object> actualContext = null;
            ContextHelper.GetServiceContext(new SelfHostedPXServiceCore.Mocks.PXServiceSettings(), ref actualContext);
            ContextHelper.GetClientContext(request, ref actualContext);

            // Assert
            Assert.IsNull(ContextHelper.TryGetContextValue(actualContext, "NonExistentGroup.baseUrl"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ContextHelperTryGetThrowsArgumentExceptionForPathLength1()
        {
            // Arrange
            var request = GetRequestWithAllTypicalContextHeaders();

            // Act
            Dictionary<string, object> actualContext = null;
            ContextHelper.GetServiceContext(new SelfHostedPXServiceCore.Mocks.PXServiceSettings(), ref actualContext);
            ContextHelper.GetClientContext(request, ref actualContext);

            // Assert
            Assert.IsNull(ContextHelper.TryGetContextValue(actualContext, "Pifd"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ContextHelperTryGetThrowsArgumentExceptionForPathLength3()
        {
            // Arrange
            var request = GetRequestWithAllTypicalContextHeaders();

            // Act
            Dictionary<string, object> actualContext = null;
            ContextHelper.GetServiceContext(new SelfHostedPXServiceCore.Mocks.PXServiceSettings(), ref actualContext);
            ContextHelper.GetClientContext(request, ref actualContext);

            // Assert
            Assert.IsNull(ContextHelper.TryGetContextValue(actualContext, "Pifd.baseUrl.extraPath"));
        }

        private void AssertContextGroup(Dictionary<string, string> expectedGroup, Dictionary<string, object> actualContext, string groupName)
        {
            Assert.IsNotNull(actualContext, "Context dictionary is null");

            object groupObject = null;
            actualContext.TryGetValue(groupName, out groupObject);
            Assert.IsNotNull(groupObject, "{0} group was not found in the context dictionary", groupName);

            Dictionary<string, string> actualGroup = groupObject as Dictionary<string, string>;
            Assert.IsNotNull(actualGroup, "{0} group in context is not of type Dictionary<string, string>", groupName);

            CollectionAssert.AreEquivalent(expectedGroup.Keys, actualGroup.Keys);
            foreach (string key in expectedGroup.Keys)
            {
                Assert.AreEqual(expectedGroup[key], actualGroup[key], "{0} in context group {1} are not equal", key, groupName);
            }
        }

        private HttpRequestMessage GetRequestWithAllTypicalContextHeaders()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl("/someUrl"));
            request.Headers.Add("x-ms-clientcontext-encoding", "base64");

            // Base64 encoded values of: ipAddress=131.107.147.243,deviceId=1234
            request.Headers.Add("x-ms-deviceinfo", "ipAddress=MTMxLjEwNy4xNDcuMjQz,deviceId=MTIzNA==");

            // Base64 encoded values of: type=AAD,context=me
            request.Headers.Add("x-ms-authinfo", "type=QUFE,context=bWU=");

            // Base64 encoded values of: PUID=1234,emailAddress=foo@bar.com,firstName=foo,lastName=bar
            request.Headers.Add("x-ms-msaprofile", "PUID=MTIzNA==,emailAddress=Zm9vQGJhci5jb20=,firstName=Zm9v,lastName=YmFy");

            return request;
        }
    }
}
