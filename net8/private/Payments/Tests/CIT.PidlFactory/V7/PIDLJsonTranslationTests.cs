// <copyright file="PIDLJsonTranslationTests.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using PidlTest.JsonDiff;

    [TestClass]
    public class PIDLJsonTranslationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void PidlResourceJSONTranslationPaymentMethodDescriptions()
        {
            using (StreamReader sw = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestData\GetPaymentMethodDescriptionsPositiveTestCases.csv")))
            {
                // Skip headers
                sw.ReadLine();
                string line;

                while ((line = sw.ReadLine()) != null)
                {
                    // Column order is Country, Family, Type, Operation, Language
                    string[] rowValues = line.Split(',');

                    PaymentMethod pi = new PaymentMethod() { PaymentMethodId = "1a2b3c4d", PaymentMethodFamily = rowValues[(int)GetPaymentMethodDescriptionTestColumns.Family], PaymentMethodType = rowValues[(int)GetPaymentMethodDescriptionTestColumns.Type] };
                    HashSet<PaymentMethod> testPIs = new HashSet<PaymentMethod>();
                    testPIs.Add(pi);

                    List<PIDLResource> pidls = null;
                    pidls = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(
                        testPIs,
                        rowValues[(int)GetPaymentMethodDescriptionTestColumns.Country],
                        rowValues[(int)GetPaymentMethodDescriptionTestColumns.Family],
                        rowValues[(int)GetPaymentMethodDescriptionTestColumns.Type],
                        rowValues[(int)GetPaymentMethodDescriptionTestColumns.Operation],
                        rowValues[(int)GetPaymentMethodDescriptionTestColumns.Language]);

                    string basePidls = JsonConvert.SerializeObject(pidls);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(basePidls));

                    List<PIDLResource> translatedPidls = new List<PIDLResource>();
                    PIDLResource.PopulatePIDLResource(basePidls, translatedPidls);
                    var results = DiffFinder.GetPidlDiffs(basePidls, JsonConvert.SerializeObject(translatedPidls));

                    Assert.AreEqual(0, results.Count);
                }
            }
        }
    }
}
