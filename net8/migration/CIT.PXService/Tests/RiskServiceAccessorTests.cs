// <copyright file="RiskServiceAccessorTests.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Accessors.RiskService;
    using Microsoft.Commerce.Payments.PXService.RiskService.V7;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.CommonSchema.Services.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    [TestClass]
    public class RiskServiceAccessorTests : TestBase
    {
        private readonly RiskServiceAccessor riskServiceAccessor;

        private readonly Mock<HttpMessageHandler> httpMessageHandlerMock;

        public RiskServiceAccessorTests()
        {
            this.httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            this.riskServiceAccessor = new RiskServiceAccessor("https://localhost/pxservice", "https://localhost/pxservice", "V6", this.httpMessageHandlerMock.Object);
        }

        [TestMethod]
        public async Task ApplyRiskEligibilityTest_Success()
        {
            string puid = string.Empty;
            string tid = Guid.NewGuid().ToString();
            string oid = Guid.NewGuid().ToString();
            string client = Guid.NewGuid().ToString();
            IList<PaymentMethod> paymementMethodList = GetPaymentMethodList();
            string ipAddress = "10.0.0.1";
            string locale = "en-US";
            string deviceType = "Laptop";
            EventTraceActivity traceActivityId = new EventTraceActivity
            {
                ActivityId = Guid.NewGuid(),
                CorrelationVectorV4 = new CorrelationVector()
            };

            IList<RiskServiceResponsePaymentInstrument> riskServicePaymentInformations = GetRiskServiceResponsePaymentInstrument();

            RiskEligibilityResponse riskEligibilityResponse = new RiskEligibilityResponse
            {
                Id = new Guid().ToString(),
                EventType = string.Empty,
                Decision = string.Empty,
                Reasons = new List<string>(),
                PaymentInstrumentTypes = riskServicePaymentInformations
            };

            string content = JsonConvert.SerializeObject(riskEligibilityResponse);

            this.httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = new StringContent(content) }).Verifiable();
               
            IList<PaymentMethod> paymentMethods = await this.riskServiceAccessor.FilterBasedOnRiskEvaluation(client, puid, tid, oid, paymementMethodList, ipAddress, locale, deviceType, traceActivityId);
            Assert.AreEqual(paymentMethods.Count, 1);
            object[] args = new object[] { ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>() };
            this.httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(), args);
        }

        [TestMethod]
        public async Task ApplyRiskEligibilityTest_BadRequest()
        {
            string puid = string.Empty;
            string tid = Guid.NewGuid().ToString();
            string oid = Guid.NewGuid().ToString();
            string client = Guid.NewGuid().ToString();
            string ipAddress = "10.0.0.1";
            string locale = "en-US";
            string deviceType = "Laptop";

            IList<PaymentMethod> paymementMethodList = GetPaymentMethodList();
            EventTraceActivity traceActivityId = new EventTraceActivity
            {
                ActivityId = Guid.NewGuid(),
                CorrelationVectorV4 = new CorrelationVector()
            };

            RiskServiceErrorResponse riskServiceErrorResponse = new RiskServiceErrorResponse()
            {
                Code = "BadRequest",
                Message = "Error while retriving payment Methods",
                Parameters = new List<string>()
            };

            string content = JsonConvert.SerializeObject(riskServiceErrorResponse);

            this.httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent(content) }).Verifiable();

            IList<PaymentMethod> paymentMethods = await this.riskServiceAccessor.FilterBasedOnRiskEvaluation(client, puid, tid, oid, paymementMethodList, ipAddress, locale, deviceType, traceActivityId);
            Assert.AreEqual(paymentMethods.Count, 2);
            object[] args = new object[] { ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>() };
            this.httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(), args);
        }

        [TestMethod]
        public async Task ApplyRiskEligibilityTest_InternalServerError()
        {
            string puid = string.Empty;
            string tid = Guid.NewGuid().ToString();
            string oid = Guid.NewGuid().ToString();
            string client = Guid.NewGuid().ToString();
            string sessionId = Guid.NewGuid().ToString();
            string ipAddress = "10.0.0.1";
            string locale = "en-US";
            string deviceType = "Laptop";
            IList<PaymentMethod> paymementMethodList = GetPaymentMethodList();
            EventTraceActivity traceActivityId = new EventTraceActivity
            {
                ActivityId = Guid.NewGuid(),
                CorrelationVectorV4 = new CorrelationVector()
            };
            
            RiskServiceErrorResponse riskServiceErrorResponse = new RiskServiceErrorResponse()
            {
                Code = "InternalServerError",
                Message = "Error while retriving payment Methods",
                Parameters = new List<string>()
            };

            string content = JsonConvert.SerializeObject(riskServiceErrorResponse);

            this.httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage() { StatusCode = HttpStatusCode.InternalServerError, Content = new StringContent(content) }).Verifiable();

            IList<PaymentMethod> paymentMethods = await this.riskServiceAccessor.FilterBasedOnRiskEvaluation(client, puid, tid, oid, paymementMethodList, ipAddress, locale, deviceType, traceActivityId);
            Assert.AreEqual(paymentMethods.Count, 2);
            object[] args = new object[] { ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>() };
            this.httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(), args);
        }

        [TestMethod]
        public async Task FilterPaymentMethodsTest_Success()
        {
            string puid = string.Empty;
            string client = Guid.NewGuid().ToString();
            string orderId = Guid.NewGuid().ToString();
            string sessionId = Guid.NewGuid().ToString();

            IList<PaymentMethod> paymementMethodList = GetPaymentMethodList();
            EventTraceActivity traceActivityId = new EventTraceActivity
            {
                ActivityId = Guid.NewGuid(),
                CorrelationVectorV4 = new CorrelationVector()
            };
            
            IList<RiskServicePaymentInformation> riskServicePaymentInformations = GetRiskServicePaymentInformations();

            RiskServicePISelectionResponse riskEligibilityResponse = new RiskServicePISelectionResponse
            {
                PaymentInfo = riskServicePaymentInformations
            };

            string content = JsonConvert.SerializeObject(riskEligibilityResponse);

            this.httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = new StringContent(content) }).Verifiable();

            IList<PaymentMethod> paymentMethods = await this.riskServiceAccessor.FilterPaymentMethods(puid, client, orderId, sessionId, paymementMethodList, traceActivityId);
            Assert.AreEqual(paymentMethods.Count, 1);
            object[] args = new object[] { ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>() };
            this.httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(), args);
        }

        [TestMethod]
        public async Task FilterPaymentMethodsTest_BadRequest()
        {
            string puid = string.Empty;
            string client = Guid.NewGuid().ToString();
            string orderId = Guid.NewGuid().ToString();
            string sessionId = Guid.NewGuid().ToString();

            IList<PaymentMethod> paymementMethodList = GetPaymentMethodList();
            EventTraceActivity traceActivityId = new EventTraceActivity
            {
                ActivityId = Guid.NewGuid(),
                CorrelationVectorV4 = new CorrelationVector()
            };

            RiskServiceErrorResponse riskServiceErrorResponse = new RiskServiceErrorResponse()
            {
                Code = "BadRequest",
                Message = "Error while retriving payment Methods",
                Parameters = new List<string>()
            };

            string content = JsonConvert.SerializeObject(riskServiceErrorResponse);

            this.httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage() { StatusCode = HttpStatusCode.BadRequest, Content = new StringContent(content) }).Verifiable();

            IList<PaymentMethod> paymentMethods = await this.riskServiceAccessor.FilterPaymentMethods(puid, client, orderId, sessionId, paymementMethodList, traceActivityId);
            Assert.AreEqual(paymentMethods.Count, 2);
            object[] args = new object[] { ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>() };
            this.httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(), args);
        }

        [TestMethod]
        public async Task FilterPaymentMethodsTest_InternalServerError()
        {
            string puid = string.Empty;
            string client = Guid.NewGuid().ToString();
            string orderId = Guid.NewGuid().ToString();
            string sessionId = Guid.NewGuid().ToString();

            IList<PaymentMethod> paymementMethodList = GetPaymentMethodList();
            EventTraceActivity traceActivityId = new EventTraceActivity
            {
                ActivityId = Guid.NewGuid(),
                CorrelationVectorV4 = new CorrelationVector()
            };

            RiskServiceErrorResponse riskServiceErrorResponse = new RiskServiceErrorResponse()
            {
                Code = "InternalServerError",
                Message = "Error while retriving payment Methods",
                Parameters = new List<string>()
            };

            string content = JsonConvert.SerializeObject(riskServiceErrorResponse);

            this.httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage() { StatusCode = HttpStatusCode.InternalServerError, Content = new StringContent(content) }).Verifiable();

            IList<PaymentMethod> paymentMethods = await this.riskServiceAccessor.FilterPaymentMethods(puid, client, orderId, sessionId, paymementMethodList, traceActivityId);
            Assert.AreEqual(paymentMethods.Count, 2);
            object[] args = new object[] { ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>() };
            this.httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(), args);
        }

        [TestMethod]
        public async Task FilterPaymentInstrumentsTest_Success()
        {
            string puid = string.Empty;
            string client = Guid.NewGuid().ToString();
            string orderId = Guid.NewGuid().ToString();
            string sessionId = Guid.NewGuid().ToString();

            EventTraceActivity traceActivityId = new EventTraceActivity
            {
                ActivityId = Guid.NewGuid(),
                CorrelationVectorV4 = new CorrelationVector()
            };
            IList<PaymentInstrument> paymentInstruments = GetPaymentInstrumentList();
            IList<RiskServicePaymentInformation> riskServicePaymentInformations = GetRiskServicePaymentInformations();

            RiskServicePISelectionResponse riskEligibilityResponse = new RiskServicePISelectionResponse
            {
                PaymentInfo = riskServicePaymentInformations
            };

            string content = JsonConvert.SerializeObject(riskEligibilityResponse);

            this.httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = new StringContent(content) }).Verifiable();

            IList<PaymentInstrument> paymentInstrumentsrResponse = await this.riskServiceAccessor.FilterPaymentInstruments(puid, client, orderId, sessionId, paymentInstruments, paymentInstruments.ToList(), traceActivityId);
            Assert.AreEqual(paymentInstrumentsrResponse.Count, 1);
            
            object[] args = new object[] { ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>() };
            this.httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(), args);
        }

        [TestMethod]
        public async Task FilterPaymentInstrumentsTest_BadRequest()
        {
            string puid = string.Empty;
            string client = Guid.NewGuid().ToString();
            string orderId = Guid.NewGuid().ToString();
            string sessionId = Guid.NewGuid().ToString();

            EventTraceActivity traceActivityId = new EventTraceActivity
            {
                ActivityId = Guid.NewGuid(),
                CorrelationVectorV4 = new CorrelationVector()
            };
            IList<PaymentInstrument> paymentInstruments = GetPaymentInstrumentList();
            IList<RiskServicePaymentInformation> riskServicePaymentInformations = GetRiskServicePaymentInformations();

            RiskServiceErrorResponse riskServiceErrorResponse = new RiskServiceErrorResponse()
            {
                Code = "BadRequest",
                Message = "Error while retriving payment Methods",
                Parameters = new List<string>()
            };

            string content = JsonConvert.SerializeObject(riskServiceErrorResponse);

            this.httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = new StringContent(content) }).Verifiable();

            IList<PaymentInstrument> paymentInstrumentsrResponse = await this.riskServiceAccessor.FilterPaymentInstruments(puid, client, orderId, sessionId, paymentInstruments, paymentInstruments.ToList(), traceActivityId);

            Assert.AreEqual(paymentInstrumentsrResponse.Count, 1);
            object[] args = new object[] { ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>() };
            this.httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(), args);
        }

        [TestMethod]
        public async Task FilterPaymentInstrumentsTest_InternalServerError()
        {
            string puid = string.Empty;
            string client = Guid.NewGuid().ToString();
            string orderId = Guid.NewGuid().ToString();
            string sessionId = Guid.NewGuid().ToString();

            EventTraceActivity traceActivityId = new EventTraceActivity
            {
                ActivityId = Guid.NewGuid(),
                CorrelationVectorV4 = new CorrelationVector()
            };
            IList<PaymentInstrument> paymentInstruments = GetPaymentInstrumentList();
            IList<RiskServicePaymentInformation> riskServicePaymentInformations = GetRiskServicePaymentInformations();

            RiskServiceErrorResponse riskServiceErrorResponse = new RiskServiceErrorResponse()
            {
                Code = "InternalServerError",
                Message = "Error while retriving payment Methods",
                Parameters = new List<string>()
            };

            string content = JsonConvert.SerializeObject(riskServiceErrorResponse);

            this.httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, Content = new StringContent(content) }).Verifiable();

            IList<PaymentInstrument> paymentInstrumentsrResponse = await this.riskServiceAccessor.FilterPaymentInstruments(puid, client, orderId, sessionId, paymentInstruments, paymentInstruments.ToList(), traceActivityId);

            Assert.AreEqual(paymentInstrumentsrResponse.Count, 1);
            object[] args = new object[] { ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>() };
            this.httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(), args);
        }

        private static IList<RiskServicePaymentInformation> GetRiskServicePaymentInformations()
        {
            RiskServicePaymentInformation riskServicePaymentInformation1 = new RiskServicePaymentInformation("direct_debit", "sepa")
            {
                Id = new Guid().ToString(),
                PaymentMethodFamily = "direct_debit",
                PaymentMethodType = "sepa",
                Allowed = true
            };
            RiskServicePaymentInformation riskServicePaymentInformation2 = new RiskServicePaymentInformation("direct_debit", "ach")
            {
                Id = new Guid().ToString(),
                PaymentMethodFamily = "direct_debit",
                PaymentMethodType = "ach",
                Allowed = false
            };
            IList<RiskServicePaymentInformation> riskServicePaymentInformations = new List<RiskServicePaymentInformation>
            {
                riskServicePaymentInformation1,
                riskServicePaymentInformation2
            };
            return riskServicePaymentInformations;
        }

        private static IList<RiskServiceResponsePaymentInstrument> GetRiskServiceResponsePaymentInstrument()
        {
            RiskServiceResponsePaymentInstrument riskServiceResponsePaymentInstrument1 = new RiskServiceResponsePaymentInstrument()
            {
                PaymentInstrumentFamily = "direct_debit",
                PaymentInstrumentType = "sepa",
                Allowed = true
            };
            RiskServiceResponsePaymentInstrument riskServiceResponsePaymentInstrument2 = new RiskServiceResponsePaymentInstrument()
            {
                PaymentInstrumentFamily = "direct_debit",
                PaymentInstrumentType = "ach",
                Allowed = false
            };
            IList<RiskServiceResponsePaymentInstrument> riskServiceResponsePaymentInstruments = new List<RiskServiceResponsePaymentInstrument>
            {
                riskServiceResponsePaymentInstrument1,
                riskServiceResponsePaymentInstrument2
            };
            return riskServiceResponsePaymentInstruments;
        }

        private static IList<PaymentMethod> GetPaymentMethodList()
        {
            PaymentMethod paymentMethod1 = new PaymentMethod
            {
                PaymentMethodType = "sepa",
                Properties = new PaymentMethodCapabilities(),
                PaymentMethodGroup = string.Empty,
                PaymentMethodFamily = "direct_debit",
                ExclusionTags = new List<string>()
            };
            PaymentMethod paymentMethod2 = new PaymentMethod
            {
                PaymentMethodType = "ach",
                Properties = new PaymentMethodCapabilities(),
                PaymentMethodGroup = string.Empty,
                PaymentMethodFamily = "direct_debit",
                ExclusionTags = new List<string>()
            };
            IList<PaymentMethod> paymementMethodList = new List<PaymentMethod>
            {
                paymentMethod1,
                paymentMethod2
            };
            return paymementMethodList;
        }

        private static IList<PaymentInstrument> GetPaymentInstrumentList()
        {
            PaymentInstrument paymentInstrument = new PaymentInstrument
            {
                PaymentInstrumentId = string.Empty,
                PaymentInstrumentAccountId = string.Empty,
                PaymentMethod = GetPaymentMethodList().FirstOrDefault(),
                Status = PaymentInstrumentStatus.Active,
                CreationTime = DateTime.Now.AddDays(-1),
                LastUpdatedTime = DateTime.Now,
                PaymentInstrumentDetails = new PaymentInstrumentDetails(),
                ClientAction = new ClientAction(ClientActionType.Wait)
            };
            IList<PaymentInstrument> paymentInstruments = new List<PaymentInstrument>() 
            {
                paymentInstrument
            };
            return paymentInstruments;
        }
    }
}
