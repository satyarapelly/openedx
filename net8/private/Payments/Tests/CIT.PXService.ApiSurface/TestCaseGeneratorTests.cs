namespace CIT.PXService.ApiSurface
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Commerce.Payments.PXService.ApiSurface.Diff;
    using Test.Common;
    using System.Data;
    using Microsoft.Commerce.Payments.PXService.ApiSurface;
    using System.Collections.Generic;

    [TestClass]
    public class TestCaseGeneratorTests
    {
        private TestContext testContext;

        public TestContext TestContext
        {
            get { return testContext; }
            set { testContext = value; }
        }

        /// <summary>
        /// As we try to improve diff test coverage (add missing tests, remove unnecessary or repeated tests), 
        /// below CIT helps ensure that we dont make unexpected changes to diff test coverage.
        /// </summary>
        [Ignore] //// TODO: Task 35521761: Update the test case count for each type of diff tests
        [TestMethod]
        public void VerifyTestCaseCounts()
        {
            var testCaseGenerator = new TestGenerator(
                config: new TestGeneratorConfig()
                {
                    AddressDescription = true,
                    BillingGroupDescription = true,
                    ChallengeDescription = true,
                    CheckoutDescriptions = true,
                    PaymentMethodDescription = true,
                    ProfileDescriptionWithEmulator = true,
                    ProfileDescriptionWithoutEmulator = true,
                    TaxIdDescription = true,
                    RewardsDescriptions = true,
                    PaymentInstrumentEx = true
                });

            testCaseGenerator.GenerateTestSet();

            // Verify total number of diff tests generated
            Assert.AreEqual(28737, testCaseGenerator.Set.Count());

            // Verify number of diff tests grouped by Partner, ResourceName and Operation
            CsvDataTable numTestCases = new CsvDataTable("TestData/ExpectedTestCounts.csv");

            // Use this block of code to update ExpectedTestCounts.csv (copy test output to ExpectedTestCounts.csv)
            TestContext.WriteLine("Partner,ResourceName,Family,Operation,ExpectedTestCount");

            var groups = testCaseGenerator.Set
                .GroupBy(tc => new { tc.Path.Partner, tc.Path.ResourceName, tc.Path.PIFamily, tc.Path.Operation })
                .OrderBy(g => g.Key.Partner)
                .ThenBy(g => g.Key.ResourceName)
                .ThenBy(g => g.Key.PIFamily)
                .ThenBy(g => g.Key.Operation);

            foreach (var group in groups)
            {
                TestContext.WriteLine("{0},{1},{2},{3},{4}", group.Key.Partner, group.Key.ResourceName, group.Key.PIFamily, group.Key.Operation, group.Count());
            }

            bool testCountsAreAsExpected = true;
            foreach (DataRow row in numTestCases.Rows)
            {
                string partner = row["Partner"].ToString();
                string resourceName = row["ResourceName"].ToString();
                string family = row["Family"].ToString();
                string operation = row["Operation"].ToString();
                int expectedCount = int.Parse(row["ExpectedTestCount"].ToString());

                int actualCount = testCaseGenerator.Set.Where(tc =>
                {
                    return string.Equals(tc.Path.Partner ?? string.Empty, partner, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(tc.Path.ResourceName, resourceName, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(tc.Path.PIFamily ?? string.Empty, family, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(tc.Path.Operation ?? string.Empty, operation, StringComparison.OrdinalIgnoreCase);
                }).Count();

                if (expectedCount != actualCount)
                {
                    if (testCountsAreAsExpected)
                    {
                        testCountsAreAsExpected = false;
                        TestContext.WriteLine("Below groups of tests have an unexpected number of tests:");
                        TestContext.WriteLine("---------------------------------------------------------------------------------------");
                        TestContext.WriteLine("Partner,ResourceName,Operation,Family,ExpectedCount,ActualCount");
                    }

                    TestContext.WriteLine("{0},{1},{2},{3},{4},{5}", partner, resourceName, operation, family, expectedCount, actualCount);
                }
            }

            Assert.IsTrue(testCountsAreAsExpected, "Atleast one group of tests have an unexpected number of tests.  See test output for details.");
        }

        /// <summary>
        /// This test verifies that there is atleast one diff test case to cover each scenario that we consider as
        /// being in the "in prod" status.  We consider a scenario to be "in prod" if PX service is currently receiving 
        /// production traffic for that scenario or if we (PX team) have communicated to partners that a scenario as being 
        /// ready for production traffic (even though we may not be receiving traffic just yet).
        /// Status of each scenario is being tracked by the PM team in an excel sheet.  The below test case runs 
        /// off of a bare-bones csv version of that excel sheet.
        /// </summary>
        [TestMethod]
        public void VerifyTestCaseCoverage()
        {
            var testCaseGenerator = new TestGenerator(
                config: new TestGeneratorConfig()
                {
                    AddressDescription = true,
                    BillingGroupDescription = true,
                    ChallengeDescription = true,
                    CheckoutDescriptions = true,
                    PaymentMethodDescription = true,
                    ProfileDescriptionWithEmulator = true,
                    ProfileDescriptionWithoutEmulator = true,
                    TaxIdDescription = true,
                    RewardsDescriptions = true,
                    PaymentInstrumentEx = true
                });

            testCaseGenerator.GenerateTestSet();
            CsvDataTable expectedTestCoverage = new CsvDataTable("TestData/ExpectedTestCoverage.csv");

            bool diffTestsCoverAllScenariosInProd = true;
            foreach (DataRow row in expectedTestCoverage.Rows)
            {
                for (int partnerIndex = 5; partnerIndex < expectedTestCoverage.Columns.Count; partnerIndex++)
                {
                    // We only want to verify coverage for scenarios that are considered "in prod"
                    if (!string.Equals(row[partnerIndex].ToString(), "in prod", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string partner = expectedTestCoverage.Columns[partnerIndex].ColumnName;
                    string resourceName = row[0].ToString();
                    string family = row[1].ToString();
                    string type = row[2].ToString();
                    string country = row[3].ToString();
                    string[] operations = row[4].ToString().Split(new char[] { ',' });

                    foreach (string operation in operations)
                    {
                        if (null == testCaseGenerator.Set.FirstOrDefault(tc =>
                        {
                            return string.Equals(tc.Path.Partner, partner, StringComparison.OrdinalIgnoreCase)
                                && string.Equals(tc.Path.ResourceName, resourceName, StringComparison.OrdinalIgnoreCase)
                                && (string.IsNullOrEmpty(family) || string.Equals(tc.Path.PIFamily, family, StringComparison.OrdinalIgnoreCase))
                                && (string.IsNullOrEmpty(type) || string.Equals(tc.Path.PIType ?? string.Empty, type, StringComparison.OrdinalIgnoreCase))
                                && (string.IsNullOrEmpty(operation) || string.Equals(tc.Path.Operation ?? string.Empty, operation, StringComparison.OrdinalIgnoreCase))
                                && (string.IsNullOrEmpty(country) || string.Equals(tc.Path.Country ?? string.Empty, country, StringComparison.OrdinalIgnoreCase));
                        }))
                        {
                            if (diffTestsCoverAllScenariosInProd == true)
                            {
                                diffTestsCoverAllScenariosInProd = false;

                                TestContext.WriteLine("Below \"in prod\" scenarios are not covered by diff tests:");
                                TestContext.WriteLine("---------------------------------------------------------------------------------------");
                                TestContext.WriteLine("ResourceName,Family,Type,Operation,Country,Partner");
                            }

                            TestContext.WriteLine("{0},{1},{2},{3},{4},{5}", resourceName, family, type, operation, country, partner);
                        }
                    }
                }
            }

            Assert.IsTrue(diffTestsCoverAllScenariosInProd, "Atleast one \"in prod\" scenario is not being covered by diff tests.  See test output for details.");
        }
        
        /// <summary>
        /// Test to validate PSS feature diff test are generated.
        /// </summary>
        [TestMethod]
        public void VerifyPSSFeatureTestGenerate()
        {
            var testCaseGenerator = new TestGenerator(
                config: new TestGeneratorConfig()
                {
                    RunDiffTestsForPSSFeatures = true
                });

            testCaseGenerator.GenerateTestSet();

            Assert.IsTrue(testCaseGenerator.Set.Count > 0, "PSS feature Diff tests are expected when RunDiffTestsForPSSFeatures is enabled");
        }
        
        /// <summary>
        /// Test to validate PSS featurename of difftests are unique.
        /// </summary>
        [TestMethod]
        public void VerifyUniqueFeatureNameForPSSDiffTests()
        {
            // Group all by featurename and filter names where count is more than 1
            List<string> duplicateFeatureNames = FeatureDiffTestGenerator.PSSFeatures
                .GroupBy(f => f.FeatureName.ToLower())
                .Where(g => g.Count() > 1)
                .Select(f => f.Key)
                .ToList();

            Assert.IsTrue(duplicateFeatureNames.Count == 0, $"There are features with duplicate FeatureName: {string.Join(",", duplicateFeatureNames)}");
        }
    }
}
