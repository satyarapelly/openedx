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
    public class PropertyDescriptionOverrideTests
    {
        private const string Partner = "test";

        [DataRow("us", "address_line2", false, "test")]
        [DataRow("us", "postal_code", true, "test")]
        [DataRow("us", "region", true, "test")]
        [DataRow("us", "address_line2", true, "default")]
        [DataRow("us", "postal_code", false, "default")]
        [DataRow("us", "region", false, "default")]
        [TestMethod]
        public void GetAddressDescriptions_OverrideIsOptional(string country, string propertyName, bool isOptional, string partner)
        {
            const string AddressType = "billing";
            const string AddressLanguage = "en-us";
            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetAddressDescriptions(country, AddressType, AddressLanguage, partner);

            foreach (PIDLResource pidl in pidls)
            {
                Assert.IsTrue(pidl.DataDescription.ContainsKey(propertyName));
                PropertyDescription propertyDescription = pidl.DataDescription[propertyName] as PropertyDescription;
                Assert.IsTrue(propertyDescription.IsOptional.HasValue, "'IsOptional' is missing");
                Assert.AreEqual(propertyDescription.IsOptional.Value, isOptional, $"{propertyName} is not {isOptional} for {country}");
            }
        }

        [DataRow("us", "address_line2", "default_address_line2")]
        [DataRow("us", "postal_code", "000000")]
        [TestMethod]
        public void GetAddressDescriptions_OverrideDefaultValue(string country, string propertyName, string defaultValue)
        {
            const string AddressType = "billing";
            const string AddressLanguage = "en-us";
            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetAddressDescriptions(country, AddressType, AddressLanguage, Partner);

            foreach (PIDLResource pidl in pidls)
            {
                Assert.IsTrue(pidl.DataDescription.ContainsKey(propertyName));
                PropertyDescription propertyDescription = pidl.DataDescription[propertyName] as PropertyDescription;
                Assert.AreEqual(propertyDescription.DefaultValue, defaultValue, "Default value doesn't match");
            }
        }

        [DataRow("us", null, "city", false)]
        [DataRow("us", "us_optional_city_scenario", "city", true)]
        [TestMethod]
        public void GetAddressDescriptions_OverrideIsOptionalWithScenario(string country, string scenario, string propertyName, bool isOptional)
        {
            const string AddressType = "billing";
            const string AddressLanguage = "en-us";
            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetAddressDescriptions(country, AddressType, AddressLanguage, Partner, scenario: scenario);

            foreach (PIDLResource pidl in pidls)
            {
                Assert.IsTrue(pidl.DataDescription.ContainsKey(propertyName));
                PropertyDescription propertyDescription = pidl.DataDescription[propertyName] as PropertyDescription;
                Assert.IsTrue(propertyDescription.IsOptional.HasValue, "'IsOptional' is missing");
                Assert.AreEqual(propertyDescription.IsOptional.Value, isOptional, $"{propertyName} is not {isOptional} for {country}");
            }
        }
    }
}
