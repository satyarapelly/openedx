// <copyright file="GetPropertyDescriptionByPropertyName.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using Helpers;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class PropertyValidationOverrideTests
    {
        private const string Partner = "test";

        [DataRow("us", "postal_code", "^\\d{1}$")]
        [DataRow("tw", "postal_code", "^[0-9]{3,3}((-|\\s)?[0-9]{2,3})?$")]
        [DataRow("us", "address_line2", "^\\s{1,3}$")]
        [TestMethod]
        public void GetAddressDescriptions_OverrideValidationRegex(string country, string propertyName, string regex)
        {
            const string AddressType = "billing";
            const string AddressLanguage = "en-us";
            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetAddressDescriptions(country, AddressType, AddressLanguage, Partner);

            foreach (PIDLResource pidl in pidls)
            {
                Assert.IsTrue(pidl.DataDescription.ContainsKey(propertyName));
                PropertyDescription propertyDescription = pidl.DataDescription[propertyName] as PropertyDescription;
                Assert.AreEqual(propertyDescription.Validation.Regex, regex, "Regex doesn't match");
            }
        }
    }
}
