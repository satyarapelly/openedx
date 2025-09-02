// <copyright file="FeatureDiffTestGenerator.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.ApiSurface
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using Microsoft.Commerce.Payments.PXService.ApiSurface.Diff;
    using Newtonsoft.Json;

    /// <summary>
    /// FeatureDiffTestGenerator generates the diff tests for PSS features.
    /// </summary>
    public class FeatureDiffTestGenerator
    {
        private readonly TestGeneratorConfig config;

        public FeatureDiffTestGenerator(TestGeneratorConfig config)
        {
            this.config = config;
        }

        public static List<FeatureDiffTest> PSSFeatures
        {
            get
            {
                // Path of file in PidlTest project
                var pssFeaturesTestsJson = File.ReadAllText(".\\PSSFeatureDiffTest\\pss_feature_diff_tests.json");
                var pssFeaturesTests = JsonConvert.DeserializeObject<List<FeatureDiffTest>>(pssFeaturesTestsJson);

                return pssFeaturesTests;
            }
        }

        public List<Test> GeneratePSSFeatureDiffTests()
        {
            List<Test> set = new List<Test>();
            List<FeatureDiffTest> pssTestFeatures = PSSFeatures.Where(f => !f.DisableTesting).ToList();

            // If config.PaymentInstrumentEx is disabled then disable all the tests related to PaymentInstrumentEx
            if (!this.config.PaymentInstrumentEx)
            {
                pssTestFeatures = SkipDiffTestsByResourceName(pssTestFeatures, "paymentInstrumentsEx");
            }

            foreach (var pssFeature in pssTestFeatures)
            {
                // If countries are not specified, then use common Countries collection from DiffTest
                pssFeature.TestCountries = pssFeature.TestCountries != null && pssFeature.TestCountries.Count > 0 ? pssFeature.TestCountries : Constants.CountriesTest;

                foreach (var test in pssFeature.Tests)
                {
                    Dictionary<string, string> headers = CreateRequestHeaders(test);

                    foreach (string partner in pssFeature.TestPartners)
                    {
                        foreach (string country in pssFeature.TestCountries)
                        {
                            // If operations or types are not specified, default to empty string to continue for loop
                            test.Operations = test.Operations != null && test.Operations.Count > 0 ? test.Operations : new List<string>() { string.Empty };
                            test.Types = test.Types != null && test.Types.Count > 0 ? test.Types : new List<string>() { string.Empty };

                            foreach (string operation in test.Operations)
                            {
                                foreach (string type in test.Types)
                                {
                                    if (!SkipPSSFeatureDiffTestCombination(test.SkipCombinations, partner, operation, type, country))
                                    {
                                        set.Add(CreateTest(pssFeature.FeatureName, test, headers, partner, country, operation, type));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return set;
        }

        private static Dictionary<string, string> CreateRequestHeaders(FeatureDiffTestConfig test)
        {
            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-test", string.Format("{{scenarios: \"{0}\", contact: \"{1}\"}}", test.TestSceanrioHeaders, "DiffTest")
                },
                {
                    "x-ms-flight", test.Flights != null && test.Flights.Count() > 0 ? $"{test.Flights},PXDisablePSSCache" : "PXDisablePSSCache"
                }
            };

            if (test.AdditionalHeaders != null && test.AdditionalHeaders.Count > 0)
            {
                foreach (var additionalHeader in test.AdditionalHeaders)
                {
                    headers.Add(additionalHeader.Key, additionalHeader.Value);
                }
            }

            return headers;
        }

        private static Test CreateTest(string featureName, FeatureDiffTestConfig featureTest, Dictionary<string, string> headers, string partner, string country, string operation, string type)
        {
            var url = featureTest.Url.Split('?');
            var query = HttpUtility.ParseQueryString(url.LastOrDefault().ToLower());
            string resourceName = url.FirstOrDefault();

            query.Set("partner", partner);
            query.Set("country", country);

            if (!string.IsNullOrEmpty(operation))
            {
                query.Set("operation", operation);
            }

            if (!string.IsNullOrEmpty(type))
            {
                query.Set("type", type);
            }

            // If user type is not specified, default to UserMe
            featureTest.UserType = featureTest.UserType ?? Constants.UserTypes.UserMe;

            return new Test(new TestRequestRelativePath(featureTest.UserType, resourceName, query), null, null, additionalHeaders: headers, testName: $"PSS: {featureName}");
        }

        private static bool SkipPSSFeatureDiffTestCombination(List<FeatureTestSkipCombination> testSkipCombinations, string partner, string operation, string type, string country)
        {
            if (testSkipCombinations == null || testSkipCombinations.Count == 0)
            {
                return false;
            }

            return testSkipCombinations.Any(featureSkipCombination =>
                (string.Equals(featureSkipCombination.Partner, partner, StringComparison.OrdinalIgnoreCase) || string.Equals(featureSkipCombination.Partner, Constants.DiffTest.Any, StringComparison.OrdinalIgnoreCase))
                && (string.Equals(featureSkipCombination.Country, country, StringComparison.OrdinalIgnoreCase) || string.Equals(featureSkipCombination.Country, Constants.DiffTest.Any, StringComparison.OrdinalIgnoreCase))
                && (string.Equals(featureSkipCombination.ResourceType, type, StringComparison.OrdinalIgnoreCase) || string.Equals(featureSkipCombination.ResourceType, Constants.DiffTest.Any, StringComparison.OrdinalIgnoreCase))
                && (string.Equals(featureSkipCombination.Operation, operation, StringComparison.OrdinalIgnoreCase) || string.Equals(featureSkipCombination.Operation, Constants.DiffTest.Any, StringComparison.OrdinalIgnoreCase)));
        }

        private static List<FeatureDiffTest> SkipDiffTestsByResourceName(List<FeatureDiffTest> pssTestFeatures, string resourceName)
        {
            pssTestFeatures.ForEach(f => f.Tests.RemoveAll(t => string.Equals(t.Url.Split('?')[0], resourceName, StringComparison.OrdinalIgnoreCase)));

            return pssTestFeatures;
        }
    }
}
