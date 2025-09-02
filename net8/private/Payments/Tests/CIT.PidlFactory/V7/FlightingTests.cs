// <copyright company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System.Collections.Generic;
    using System.Linq;
    using Helpers;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FlightingTests
    {
        public TestContext TestContext { get; set; }

        [DataRow("", true, false)]
        [DataRow("swapNameAndNumberFlight", false, false)]
        [DataRow("addExtraFieldFlight", true, true)]
        [DataRow("swapNameAndNumberFlight,addExtraFieldFlight", false, true)]
        [DataRow("TestFlightNA", true, false)]
        [DataTestMethod]
        public void FlightTest_SwapOrderAndAddNewItem(string flightNames, bool defaultOrderForCardNumberAndName, bool hasTestMessage)
        {
            const string Country = "us";
            const string Family = TestConstants.PaymentMethodFamilyNames.CreditCard;
            const string Type = TestConstants.PaymentMethodTypeNames.Visa;
            const string Operation = TestConstants.PidlOperationTypes.Add;
            const string Language = "en-us";
            const string Partner = "Test";

            PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = Family, PaymentMethodType = Type };
            HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
            testPIs.Add(pi);
            List<string> flightNamesList = flightNames.Split(',').ToList();

            this.TestContext.WriteLine("Start testing csv flight scenario: FlightList \"{0}\"", flightNames);
            List<PIDLResource> flightPidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(testPIs, Country, Family, Type, Operation, Language, Partner, exposedFlightFeatures: flightNamesList);

            PidlAssert.IsValid(flightPidls, 1);

            int cardNumberIndex = flightPidls[0].DisplayPages[0].Members.IndexOf(flightPidls[0].GetDisplayHintById(TestConstants.DisplayHintIds.CardNumber) as PropertyDisplayHint);
            int cardholderNameIndex = flightPidls[0].DisplayPages[0].Members.IndexOf(flightPidls[0].GetDisplayHintById(TestConstants.DisplayHintIds.CardholderName) as PropertyDisplayHint);
            Assert.AreEqual(defaultOrderForCardNumberAndName, cardNumberIndex < cardholderNameIndex, "Order of cardNumber and cardholderName is incorrect");

            TextDisplayHint testFlightMessage = flightPidls[0].GetDisplayHintById(TestConstants.DisplayHintIds.TestMessageForAddCC) as TextDisplayHint;
            Assert.AreEqual(hasTestMessage, testFlightMessage != null, "TestFlightMessage does not apply to flight setting");
        }
    }
}
