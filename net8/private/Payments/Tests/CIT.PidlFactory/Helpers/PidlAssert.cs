// <copyright file="PidlAssert.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace CIT.PidlFactory.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using V7;

    internal static class PidlAssert
    {
        public static void IsValid(
            List<PIDLResource> pidls,
            int? count = null,
            bool identity = true,
            bool dataDescription = true,
            bool displayDescription = true,
            string descriptionType = null,
            string operation = null,
            string country = null,
            bool clientSidePrefill = false)
        {
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            if (count.HasValue)
            {
                Assert.AreEqual(count, pidls.Count(), "Pidl count is not as expected");
            }

            foreach (var pidl in pidls)
            {
                if (identity)
                {
                    Assert.IsNotNull(pidl.Identity, "Pidl identity is expected to be not null");

                    if (descriptionType != null && operation != null && country != null)
                    {
                        var expectedIdentity = new Dictionary<string, string>
                        {
                            { TestConstants.DescriptionIdentityFields.DescriptionType, descriptionType },
                            { TestConstants.DescriptionIdentityFields.Operation, operation },
                            { TestConstants.DescriptionIdentityFields.Country, country }
                        };

                        CollectionAssert.AreEquivalent(expectedIdentity, pidl.Identity, "Pidl identity is expected to have correct keys and values");
                    }
                }

                if (dataDescription)
                {
                    Assert.IsNotNull(pidl.DataDescription, "Pidl data description is expected to be not null");
                    Assert.IsTrue(pidl.DataDescription.Count > 0, "Pidl data description is expected to have atleast one property description");
                }

                if (displayDescription)
                {
                    Assert.IsNotNull(pidl.DisplayPages, "Pidl DisplayPages is expected to be not null");
                    Assert.IsTrue(pidl.DisplayPages.Count > 0, "Pidl is expected to have atleast one display page");
                }

                if (clientSidePrefill)
                {
                    Assert.IsNotNull(pidl.DataSources, "For client side prefilling to work, Pidl DataSources is expected to be not null");
                    Assert.IsTrue(pidl.DataSources.Count > 0, "For client side prefilling to work, Pidl is expected to have atleast one DataSource");
                }
            }
        }

        public static void HasLinkedPidl(
            List<PIDLResource> pidls,
            PIDLResource linkedPidl,
            PidlContainerDisplayHint.SubmissionOrder submitOrder,
            int pidlIndex = 0)
        {
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            foreach (var pidl in pidls)
            {
                Assert.IsNotNull(pidl, "Pidl is expected to be not null");
                var actualLinkedPidl = pidl.LinkedPidls.Where(lp => lp.Identity == linkedPidl.Identity).FirstOrDefault();
                Assert.IsNotNull(linkedPidl, "Linked Pidl is expected to be not null");
                Assert.AreEqual(actualLinkedPidl, linkedPidl, "Linked Pidl is different from what was expected");

                var expectedPidlLinkHintId = "pidlContainer" + pidlIndex;
                var pidlLink = pidl.DisplayHints().OfType<PidlContainerDisplayHint>().Where(dh => dh.HintId == expectedPidlLinkHintId).FirstOrDefault();
                Assert.IsNotNull(pidlLink, "Pidl link with the id \"{0}\" is expected", expectedPidlLinkHintId);
                Assert.AreEqual(submitOrder, pidlLink.SubmitOrder, "Submit order is different from what was expected");
            }
        }
    }
}
