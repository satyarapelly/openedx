// <copyright file="GuestAccountHelperTests.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System.Net.Http;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static Microsoft.Commerce.Payments.PXService.V7.Contexts.Constants;

    [TestClass]
    public class GuestAccountHelperTests
    {
        [TestMethod]
        public void Test_IsGuestAccount()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Add(
                "x-ms-customer",
                CustomerHeaderTests.CustomerHeaderTestToken);

            bool isGuestAccount = GuestAccountHelper.IsGuestAccount(request);
            Assert.IsTrue(isGuestAccount);
            Assert.AreEqual(request.GetProperties()["x-ms-customer_customerType"], CustomerType.AnonymousUser);
        }

        [TestMethod]
        public void Test_IsGuestAccount_FromRequestProperties()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.GetProperties()["x-ms-customer_customerType"] = CustomerType.AnonymousUser;

            bool isGuestAccount = GuestAccountHelper.IsGuestAccount(request);
            Assert.IsTrue(isGuestAccount);
            Assert.AreEqual(request.GetProperties()["x-ms-customer_customerType"], CustomerType.AnonymousUser);
        }
    }
}
