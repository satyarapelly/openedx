// <copyright file="UnitTestBase.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Helpers;
    using Microsoft.Commerce.Payments.PidlFactory;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using PXCommon = Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Test.Common;

    [TestClass]
    public abstract class UnitTestBase
    {
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Expects partner subsets to be disjoint and the union of all partner subsets to equal the full set of partners
        /// </summary>
        /// <param name="partnerSubsets"></param>
        protected static void TestPartnerSetCoverage(IEnumerable<string[]> partnerSubsets, string subsetsDescription)
        {
            if (partnerSubsets == null)
            {
                throw new ArgumentNullException();
            }

            if (partnerSubsets.Count() < 2)
            {
                throw new ArgumentException();
            }

            Dictionary<string, int> partnerSubsetIndexMap = new Dictionary<string, int>();
            int index = 0;

            foreach (string[] partnerSubset in partnerSubsets)
            {
                foreach (string partner in partnerSubset)
                {
                    bool isPartnerAlreadyAdded = partnerSubsetIndexMap.ContainsKey(partner);
                    int partnerPreviousIndex = isPartnerAlreadyAdded ? partnerSubsetIndexMap[partner] : -1;
                    Assert.IsFalse(isPartnerAlreadyAdded, string.Format("CIT for {0} is expected to be disjoint for all partner subsets. {1} was found in partner subsets {2} and {3}", subsetsDescription, partner, partnerPreviousIndex, index));
                    partnerSubsetIndexMap[partner] = index;
                }

                index++;
            }

            CollectionAssert.AreEquivalent(TestConstants.AllPartners.ToList(), partnerSubsetIndexMap.Keys.ToList(), string.Format("CIT for {0} is expected to cover all partners", subsetsDescription));
        }

        /// <summary>
        /// Expects at least one pidl has a valid submit hint
        /// </summary>
        /// <param name="pidls">The pidls to validate</param>
        /// <param name="allowedSubmitButtonHintIds">The id values to look for when checking for submit hints</param>
        /// <param name="submitType">The type of what is being submitted</param>
        /// <param name="submitActionContext">The submit hint action context, exposed for convenience so we don't have to find it again after validating it exists</param>
        protected static void AssertSubmitHintExists(List<PIDLResource> pidls, IEnumerable<string> allowedSubmitButtonHintIds, string submitType, out PXCommon.RestLink submitActionContext)
        {
            ContentDisplayHint submitDisplayHint = null;

            PidlAssert.IsValid(pidls);

            foreach (PIDLResource pidl in pidls)
            {
                foreach (string submitButtonId in allowedSubmitButtonHintIds)
                {
                    submitDisplayHint = pidl.DisplayHints().Where(dh => dh.HintId == submitButtonId).FirstOrDefault() as ContentDisplayHint;

                    if (submitDisplayHint != null)
                    {
                        break;
                    }
                }

                if (submitDisplayHint != null)
                {
                    break;
                }
            }

            Assert.IsNotNull(submitDisplayHint, string.Format("{0} is expected to have a submit display hint", submitType));
            Assert.IsNotNull(submitDisplayHint.Action, string.Format("{0} submit display hint is expected to have an action", submitType));

            submitActionContext = submitDisplayHint.Action.Context as PXCommon.RestLink;

            Assert.IsNotNull(submitActionContext, string.Format("{0} submit display hint action is expected to have a context", submitType));
        }
    }
}
