// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System.Linq;
    using System.Threading.Tasks;
    using global::Tests.Common.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DefaultParamsTests : TestBase
    {
        [DataRow("/v7.0/Account001/paymentMethodDescriptions?country=us&family=credit_card", "Cardholder Name", "add")]
        [DataRow("/v7.0/Account001/paymentMethodDescriptions?country=us&family=credit_card&completePrerequisites=true", "Cardholder Name", "add")]
        [DataRow("/v7.0/Account001/paymentMethodDescriptions?country=us&operation=select", "Payment Methods", "select")]
        [DataRow("/v7.0/paymentMethodDescriptions?country=us&family=credit_card&partner=marketplace", "Cardholder Name", "add")]
        [DataRow("/v7.0/Account001/addressDescriptions?country=us&type=billing", "Address line 1", null)]
        [DataRow("/v7.0/Account001/addressDescriptions?country=us&partner=webblends", "AddressGroup", "selectinstance")]
        [DataRow("/v7.0/Account001/addressDescriptions?country=us&partner=oxowebdirect", "AddressGroup", "selectinstance")]
        [DataRow("/v7.0/Account001/profileDescriptions?country=us&partner=xbox&type=consumer", "First name", "add")]
        [DataRow("/v7.0/Account001/taxIdDescriptions?country=br&type=consumer_tax_id&partner=webblends", "CPF (optional)", null)]
        [DataRow("/v7.0/Account001/taxIdDescriptions?country=br&type=consumer_tax_id&partner=oxowebdirect", "CPF (optional)", null)]
        [DataRow("/v7.0/OrgAccount001/billingGroupDescriptions?country=us&partner=commercialstores", "Pay with", "selectinstance")]
        [TestMethod]
        public async Task DefaultParams_AreAsExpected(string pidlUrl, string displayText, string expOperation)
        {
            var pidls = await GetPidlFromPXService(pidlUrl);

            foreach (var pidl in pidls)
            {
                if (displayText != null)
                {
                    var foundDisplayHint = pidl.DisplayHints().FirstOrDefault(dh =>
                    {
                        return dh.DisplayText() != null && dh.DisplayText().Contains(displayText);
                    });

                    Assert.IsNotNull(foundDisplayHint, "Default language is expected to be English");
                }

                if (expOperation != null)
                {
                    Assert.AreEqual(expOperation, pidl.Identity["operation"], "Default operation is expected to be add");
                }
            }
        }
    }
}
