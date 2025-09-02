using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Commerce.Payments.PidlFactory.V7;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CIT.PidlFactory.V7
{
    [TestClass]
    public class ConfigsTests
    {
        [TestMethod]
        public void DomainDictionariesYearsExpiryTest()
        {
            Dictionary<string, string> yearsExpiry = PIDLResourceFactory.GetDictionaryFromConfigString("{}YearsExpiry");
            Dictionary<string, string> yearsExpiry4 = PIDLResourceFactory.GetDictionaryFromConfigString("{}YearsExpiry4");

            int expectedNumberOfYears = 25;
            int currentYear = DateTime.Now.Year;

            int gracePeriodMonths = 0; // TODO:revert to 3 once expiry years are updated. Banks provide a grace period of couple of months for an expired credit card
            int gracePeriodYearIncrement = DateTime.Now.Month < gracePeriodMonths ? 0 : 1;
            int startingExpirationYear = currentYear - 2 + gracePeriodYearIncrement;

            Assert.AreEqual(yearsExpiry.Count, expectedNumberOfYears, expectedNumberOfYears + " expiry years defined");

            bool sorted = true;
            int previousYear = startingExpirationYear;
            foreach (KeyValuePair<string, string> year in yearsExpiry)
            {
                int thisYear = int.Parse(year.Key);
                sorted = sorted && thisYear - 1 == previousYear;
                Assert.AreEqual(year.Key.Substring(2), year.Value, "The shortened 2 digit year value \"" + year.Value + "\" matches defined key \"" + year.Key + "\"");
                previousYear = thisYear;
            }

            Assert.IsTrue(sorted, "Expiry years sorted");

            Assert.AreEqual(yearsExpiry4.Count, 25, "25 expiry years 4 digits defined");
            sorted = true;
            previousYear = startingExpirationYear;
            foreach (KeyValuePair<string, string> year in yearsExpiry4)
            {
                int thisYear = int.Parse(year.Key);
                sorted = sorted && thisYear - 1 == previousYear;
                Assert.AreEqual(year.Key, year.Value, "The 4 digit year value \"" + year.Value + "\" matches defined key \"" + year.Key + "\"");
                previousYear = thisYear;
            }

            Assert.IsTrue(sorted, "Expiry years 4 digits sorted");
        }

        [TestMethod]
        public void DomainDictionariesJapaneseStatesCollectionOrderTest()
        {
            Dictionary<string, string> japaneseStates = PIDLResourceFactory.GetDictionaryFromConfigString("{}JPStates");

            Assert.AreEqual(japaneseStates.Count, TestConstants.OrderedJPStateValues.Length);

            int index = 0;

            foreach (string key in japaneseStates.Keys)
            {
                Assert.AreEqual(japaneseStates[key], TestConstants.OrderedJPStateValues[index]);
                index++;
            }
        }
    }
}
