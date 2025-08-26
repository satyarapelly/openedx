// <copyright file="HttpRequestHelper.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LegacyAccountHelperTests : TestBase
    {
        [DataRow("ABC121", null)] // invalid PIID
        [DataRow("9igMnQAAAAAqAACA", "9igMnQAAAAAAAAAA")] // Valid Billable
        [TestMethod]
        public void GetBillableAccountIdValidTest(string piid, string expectedBillableAccountId)
        {
            string billableAccountId = LegacyAccountHelper.GetBillableAccountId(piid, null);
            Assert.AreEqual(billableAccountId, expectedBillableAccountId);
        }
    }
}