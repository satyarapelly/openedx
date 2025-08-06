// <copyright file="CustomerHeaderTests.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Commerce.Payments.PXService.V7.Contexts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    [TestClass]
    public class CustomerHeaderTests
    {
        internal const string CustomerHeaderTestToken = "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJyZXF1ZXN0ZXIiOiJ7fSIsInRhcmdldCI6IntcImN1c3RvbWVyVHlwZVwiOlwiYW5vbnltb3VzdXNyXCIsXCJjdXN0b21lcklkXCI6XCI4MDEwNmEwYS00NDQ5LTQ5YzMtOTJmNC1iNWU2ZWRhNDQ3NDNcIixcIm9uQmVoYWxmT2ZcIjpmYWxzZX0iLCJjYWxsZXIiOiJPbXMiLCJhdXRoVHlwZSI6Ilg1MDkiLCJ2ZXJzaW9uIjoiMS4wIiwibmJmIjoxNjkyMzg3MDMwLCJleHAiOjE2OTI0MzAyMzAsImlzcyI6InVybjptaWNyb3NvZnQ6b21zIn0.";

        [TestMethod]
        public void Test_ParseHeader()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add(
                "x-ms-customer",
                CustomerHeaderTestToken);

            CustomerHeader customerHeader = CustomerHeader.Parse(request);
            Assert.IsNotNull(customerHeader);
            Assert.AreEqual(customerHeader.TargetCustomer.CustomerType, Constants.CustomerType.AnonymousUser);
        }

        [TestMethod]
        public void Test_ParseHeader_InvalidHeader()
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Headers.Add("x-ms-customer", "invalid");

            CustomerHeader customerHeader = CustomerHeader.Parse(request);
            Assert.IsNull(customerHeader);
        }
    }
}
