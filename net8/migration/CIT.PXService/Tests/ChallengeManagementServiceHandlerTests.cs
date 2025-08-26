// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2023. All rights reserved.</copyright>
namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Accessors.ChallengeManagementService;
    using Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService;
    using Microsoft.Commerce.Payments.PXService.V7.PXChallengeManagement;
    using Microsoft.Commerce.Tracing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;    

    [TestClass]
    public class ChallengeManagementServiceHandlerTests : TestBase
    {
        private const string PXSessionId = "pxSessionId";
        private const string AccountId = "accountId";
        private Mock<IChallengeManagementServiceAccessor> mockChallengeManagementServiceAccessor = new Mock<IChallengeManagementServiceAccessor>();

        [TestInitialize]
        public void Startup()
        {
            PXSettings.ChallengeManagementService.Responses.Clear();
            PXSettings.ChallengeManagementService.ResetToDefaults();
        }

        [TestMethod]
        public async Task CreateChallenge_Succeeded()
        {
            PXChallengeManagementHandler challengeHandler = new PXChallengeManagementHandler(mockChallengeManagementServiceAccessor.Object);

            var resultString = File.ReadAllText(
                        Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory,
                            string.Format(@"TestData\challengepidl.json")));

            mockChallengeManagementServiceAccessor
                .Setup(x => x.CreateChallenge(It.IsAny<string>(), It.IsAny<EventTraceActivity>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.FromResult(resultString));

            var activeSessionData = new SessionBusinessModel()
            {
                Status = "Active",
                SessionData = "{\"challengeRequired\":true,\"challengeCompleted\":\"false\",\"challengeRetries\":0, \"accountId\":\"accountId\"}",
            };

            mockChallengeManagementServiceAccessor
                .Setup(x => x.GetChallengeSession(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(activeSessionData));

            var sessionStatusUpdated = new SessionBusinessModel()
            {
                Status = "Active",
                SessionData = "{\"challengeRequired\":true,\"challengeCompleted\":\"false\",\"challengeRetries\":0, \"accountId\":\"accountId\"}",
            };

            mockChallengeManagementServiceAccessor
                .Setup(x => x.UpdateChallengeSession(It.IsAny<SessionBusinessModel>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(sessionStatusUpdated));

            List<PIDLResource> pidl = new List<PIDLResource>();

            await challengeHandler.AddChallenge(pidl, new EventTraceActivity(), "test", PXSessionId, "en");

            Assert.IsNotNull(pidl);
        }

        [TestMethod]
        public async Task CreateChallenge_MultiPage_Succeeded()
        {
            PXChallengeManagementHandler challengeHandler = new PXChallengeManagementHandler(mockChallengeManagementServiceAccessor.Object);

            var resultString = File.ReadAllText(
                        Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory,
                            string.Format(@"TestData\challengepidl.json")));

            mockChallengeManagementServiceAccessor
                .Setup(x => x.CreateChallenge(It.IsAny<string>(), It.IsAny<EventTraceActivity>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.FromResult(resultString));

            var activeSessionData = new SessionBusinessModel()
            {
                Status = "Active",
                SessionData = "{\"challengeRequired\":true,\"challengeCompleted\":\"false\",\"challengeRetries\":0, \"accountId\":\"accountId\"}",
            };

            mockChallengeManagementServiceAccessor
                .Setup(x => x.GetChallengeSession(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(activeSessionData));

            var sessionStatusUpdated = new SessionBusinessModel()
            {
                Status = "Active",
                SessionData = "{\"challengeRequired\":true,\"challengeCompleted\":\"false\",\"challengeRetries\":0, \"accountId\":\"accountId\"}",
            };

            mockChallengeManagementServiceAccessor
                .Setup(x => x.UpdateChallengeSession(It.IsAny<SessionBusinessModel>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(sessionStatusUpdated));

            List<PIDLResource> pidl = new List<PIDLResource>();
            pidl.Add(new PIDLResource(new Dictionary<string, string>()
            {
                { "description_type", "paymentMethod" }
            }));

            await challengeHandler.AddChallenge(pidl, new EventTraceActivity(), "test", PXSessionId, "en", new List<string> { Flighting.Features.PXChallengeMultipageChallenge });

            Assert.IsNotNull(pidl);
            Assert.IsNull(pidl.FirstOrDefault()?.LinkedPidls);
            Assert.AreEqual(1, pidl.FirstOrDefault()?.DisplayPages.Count);
        }

        [TestMethod]
        public async Task GetChallengeStatus_Succeeded()
        {
            PXChallengeManagementHandler challengeHandler = new PXChallengeManagementHandler(mockChallengeManagementServiceAccessor.Object);

            var challengeResult = new ChallengeStatusResult()
            {
                Passed = true
            };

            mockChallengeManagementServiceAccessor
                .Setup(x => x.GetChallengeStatus(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(challengeResult));

            var result = await challengeHandler.GetPXChallengeStatus(pxChallengeSessionId: PXSessionId, traceActivityId: new EventTraceActivity());

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CreateChallengeSession_Succeeded()
        {
            PXChallengeManagementHandler challengeHandler = new PXChallengeManagementHandler(mockChallengeManagementServiceAccessor.Object);

            var sessionId = new SessionBusinessModel()
            {
                SessionId = "sessionId"
            };

            mockChallengeManagementServiceAccessor
                .Setup(x => x.CreateChallengeSession(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(sessionId));

            var sessionDataModel = new Dictionary<string, string>();
            sessionDataModel.Add("Language", "en-us");
            sessionDataModel.Add("Partner", "test");
            sessionDataModel.Add("Country", "country");
            sessionDataModel.Add("Operation", "add");
            sessionDataModel.Add("Family", "card");
            sessionDataModel.Add("CardType", "amex");
            
            var result = await challengeHandler.CreatePXChallengeSessionId(sessionDataModel, AccountId, new EventTraceActivity());

            Assert.AreEqual(result, "sessionId");
        }

        [TestMethod]
        public async Task GetChallengeSession_Succeeded()
        {
            PXChallengeManagementHandler challengeHandler = new PXChallengeManagementHandler(mockChallengeManagementServiceAccessor.Object);

            var activeSessionData = new SessionBusinessModel()
            {
                Status = "Active",
                SessionData = "{\"challengeRequired\":true,\"challengeCompleted\":\"false\",\"challengeRetries\":0, \"accountId\":\"accountId\"}",
            };

            mockChallengeManagementServiceAccessor
                .Setup(x => x.GetChallengeSession(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(activeSessionData));

            var result = await challengeHandler.GetPXChallengeSession(PXSessionId, AccountId, new EventTraceActivity());

            Assert.AreEqual(result["isPXChallengeSessionActive"], true);
            Assert.AreEqual(result["isPXChallengeSessionAccountValid"], true);
        }

        [TestMethod]
        public async Task UpdateChallengeSession_Succeeded()
        {
            PXChallengeManagementHandler challengeHandler = new PXChallengeManagementHandler(mockChallengeManagementServiceAccessor.Object);

            var sessionStatusUpdated = new SessionBusinessModel()
            {
                Status = "Completed"
            };

            mockChallengeManagementServiceAccessor
                .Setup(x => x.UpdateChallengeSession(It.IsAny<SessionBusinessModel>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(sessionStatusUpdated));

            var result = await challengeHandler.UpdatePXSessionCompletedStatus(PXSessionId, new EventTraceActivity());

            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public async Task UpdateChallengeRequired_Test()
        {
            PXChallengeManagementHandler challengeHandler = new PXChallengeManagementHandler(mockChallengeManagementServiceAccessor.Object);

            var activeSessionData = new SessionBusinessModel()
            {
                Status = "Active",
                SessionData = "{\"challengeRequired\":false,\"challengeCompleted\":\"false\",\"challengeRetries\":2}",
            };

            var updatedSessionData = new SessionBusinessModel()
            {
                Status = "Active",
                SessionData = "{\"challengeRequired\":true,\"challengeCompleted\":\"false\",\"challengeRetries\":2}",
            };

            mockChallengeManagementServiceAccessor
                .Setup(x => x.GetChallengeSession(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(activeSessionData));

            mockChallengeManagementServiceAccessor
                .Setup(x => x.UpdateChallengeSession(It.IsAny<SessionBusinessModel>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(updatedSessionData));

            var result = await challengeHandler.UpdatePXSessionChallengeRequired(PXSessionId, true, new EventTraceActivity());

            Assert.AreEqual(result, true);   
        }
    }
}
