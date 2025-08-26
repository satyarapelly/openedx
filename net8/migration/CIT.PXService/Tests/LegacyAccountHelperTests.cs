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

        [DataRow(TaxExemptionType.BrazilCPFID)]
        [DataRow(TaxExemptionType.BrazilCNPJID)]
        [TestMethod]
        public void TestTaxExemptionBrazil(TaxExemptionType taxExemptionType)
        {
            // Shims can be used only in a ShimsContext
            using (Microsoft.QualityTools.Testing.Fakes.ShimsContext.Create())
            {
                // Arrange
                Microsoft.Commerce.Payments.PXService.Fakes.ShimLegacyAccountHelper.GetLegacyBillableAccountFromIdPXServiceSettingsStringEventTraceActivityStringStringString =
                (a, b, c, d, e, f) =>
                {
                    return new PayinPayoutAccount
                    {
                        PayinAccount = new PayinAccount
                        {
                            TaxExemptionSet = new List<TaxExemption>
                            {
                                new TaxExemption { TaxExemptionType = taxExemptionType }
                            }
                        }
                    };
                };
                Microsoft.Commerce.Payments.PXService.Fakes.ShimLegacyAccountHelper.GetFraudDetectionContextPropertiesStringStringString =
                    (a, b, c) => { return null; };
                Microsoft.Commerce.Payments.PXService.Fakes.ShimLegacyAccountHelper.UpdateLegacyBillableAccountPXServiceSettingsStringStringStringPayinPayoutAccountStringListOfPropertyEventTraceActivityString =
                (a, b, c, d, e, f, g, h, i) =>
                {
                    PayinPayoutAccount account = e as PayinPayoutAccount;

                    // Assert
                    if (taxExemptionType == TaxExemptionType.BrazilCPFID)
                    {
                        Assert.IsNull(account.PayinAccount.TaxExemptionSet);
                    }
                    else
                    {
                        Assert.IsNotNull(account.PayinAccount.TaxExemptionSet);
                    }
                };

                // Act
                LegacyAccountHelper.UpdateLegacyBillableAccountAddress(null, "asdf", new Microsoft.Commerce.Payments.PidlFactory.V7.PIDLData(), null, "altSecId", "orgPuid", "ipAddress", "language"); // lgtm[cs/local-secret-data] lgtm[cs/secret-data-in-code] -Suppressing because of a false positive from Semmle
            }
        }
    }
}