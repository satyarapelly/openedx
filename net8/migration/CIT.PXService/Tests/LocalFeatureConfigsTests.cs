// <copyright file="LocalFeatureConfigsTests.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PXService.Settings.FeatureConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LocalFeatureConfigsTests : TestBase
    {
        public const string TestAccount1 = "testAccount1";
        public const string TestAccount1LowerCase = "testaccount1";
        public const string TestAccount2 = "testAccount2";
        public const string Feature1 = "feature1";
        public const string Feature2 = "feature2";
        public const string Feature3 = "feature3";
        public const string Partners = "partners";
        public const string AvailableToAll = "available_to_all";
        public const string Countries = "countries";

        [TestMethod]
        public void TestEmptyFeatureConfigsAndTestAccountIds()
        {
            var featureConfigs = new List<Dictionary<string, Dictionary<string, string>>>() 
            { 
                null, 
                null,
                new Dictionary<string, Dictionary<string, string>>(),
                new Dictionary<string, Dictionary<string, string>>()
            };

            var testAccountIds = new List<List<string>>()
            {
                null,
                new List<string>(),
                null,
                new List<string>()
            };

            for (int i = 0; i < featureConfigs.Count; i++)
            {
                LocalFeatureConfigs config = new LocalFeatureConfigs(featureConfigs[i], testAccountIds[i]);
                Assert.AreEqual(0, config.FeaturesEnabledForAll.Count);
                Assert.AreEqual(0, config.TestAccountIds.Count);
                Assert.AreEqual(0, config.FeaturesPerCountry.Count);
                Assert.AreEqual(0, config.FeaturesPerPartner.Count);
                Assert.AreEqual(0, config.LiveFeatures.Count);
                Assert.AreEqual(0, config.GetEligibleFeatures("dummyaccountId", "xbox", "us").Count);
            }
        }

        [TestMethod]
        public void TestDuplicatedTestAccountIds()
        {
            var featureConfigs = new List<Dictionary<string, Dictionary<string, string>>>()
            {
                new Dictionary<string, Dictionary<string, string>>(),
            };

            var testAccountIds = new List<List<string>>()
            {
                new List<string>()
                {
                   TestAccount1, TestAccount1, TestAccount2
                }
            };

            for (int i = 0; i < featureConfigs.Count; i++)
            {
                LocalFeatureConfigs config = new LocalFeatureConfigs(featureConfigs[i], testAccountIds[i]);
                Assert.AreEqual(0, config.FeaturesEnabledForAll.Count);
                Assert.AreEqual(0, config.FeaturesPerCountry.Count);
                Assert.AreEqual(0, config.FeaturesPerPartner.Count);
                Assert.AreEqual(0, config.LiveFeatures.Count);
                Assert.AreEqual(0, config.GetEligibleFeatures("dummyaccountId", "xbox", "us").Count);
                Assert.AreEqual(2, config.TestAccountIds.Count);
                Assert.IsTrue(config.TestAccountIds.Contains(TestAccount1));
                Assert.IsTrue(config.TestAccountIds.Contains(TestAccount1));
            }
        }

        [TestMethod]
        public void TestFeaturesPerPartner_PerCountry_AvailableToAll_WithOrWithoutSpace()
        {
            var featureConfigs = new List<Dictionary<string, Dictionary<string, string>>>()
            {
                new Dictionary<string, Dictionary<string, string>>()
                {
                    { Feature1,  
                        new Dictionary<string, string>() 
                        {
                            { 
                                Partners, "xbox,amcweb,xbox"
                            },
                        }
                    },
                    { Feature2,
                        new Dictionary<string, string>() 
                        {
                            {
                                Countries, "us,cn,cn"
                            },
                        }
                    },
                    { Feature3,
                        new Dictionary<string, string>() 
                        {
                            {
                                AvailableToAll, "true"
                            },
                        }
                    }
                },
                new Dictionary<string, Dictionary<string, string>>()
                {
                    { Feature1,
                        new Dictionary<string, string>()
                        {
                            {
                                Partners, "xbox , amcweb , xbox"
                            },
                        }
                    },
                    { Feature2,
                        new Dictionary<string, string>()
                        {
                            {
                                Countries, " us, cn , cn "
                            },
                        }
                    },
                    { Feature3,
                        new Dictionary<string, string>()
                        {
                            {
                                AvailableToAll, " true"
                            },
                        }
                    }
                }
            };

            var testAccountIds = new List<List<string>>()
            {
                new List<string>()
                {
                   TestAccount1, TestAccount1, TestAccount2
                },
                new List<string>()
                {
                   TestAccount1, TestAccount1, TestAccount2
                }
            };

            for (int i = 0; i < featureConfigs.Count; i++)
            {
                LocalFeatureConfigs config = new LocalFeatureConfigs(featureConfigs[i], testAccountIds[i]);
                
                // verify features enabled for all is correctly setup
                Assert.AreEqual(1, config.FeaturesEnabledForAll.Count);
                Assert.IsTrue(config.FeaturesEnabledForAll.Contains(Feature3));

                // verify features enabled for features per country
                Assert.AreEqual(2, config.FeaturesPerCountry.Count);
                Assert.IsTrue(config.FeaturesPerCountry.ContainsKey("cn"));
                Assert.IsTrue(config.FeaturesPerCountry.ContainsKey("us"));
                Assert.IsTrue(config.FeaturesPerCountry["cn"].Contains(Feature2));
                Assert.IsTrue(config.FeaturesPerCountry["us"].Contains(Feature2));

                // verify features enabled for features per partner
                Assert.AreEqual(2, config.FeaturesPerPartner.Count);
                Assert.IsTrue(config.FeaturesPerPartner.ContainsKey("xbox"));
                Assert.IsTrue(config.FeaturesPerPartner.ContainsKey("amcweb"));
                Assert.IsTrue(config.FeaturesPerPartner["xbox"].Contains(Feature1));
                Assert.IsTrue(config.FeaturesPerPartner["amcweb"].Contains(Feature1));

                // verify the live feature
                Assert.AreEqual(3, config.LiveFeatures.Count);
                Assert.IsTrue(config.LiveFeatures.Contains(Feature1));
                Assert.IsTrue(config.LiveFeatures.Contains(Feature2));
                Assert.IsTrue(config.LiveFeatures.Contains(Feature3));

                // validate fetch by partner and country
                var features = config.GetEligibleFeatures(null, "xbox", "us");
                Assert.AreEqual(3, features.Count);
                Assert.IsTrue(features.Contains(Feature1)); // add by xbox
                Assert.IsTrue(features.Contains(Feature2)); // add by us
                Assert.IsTrue(features.Contains(Feature3)); // add by available to all

                // validate test account has all the flights with out country and partner 
                features = config.GetEligibleFeatures(TestAccount1, null, null);
                Assert.AreEqual(3, features.Count);
                Assert.IsTrue(features.Contains(Feature1)); // TestAccount1
                Assert.IsTrue(features.Contains(Feature2)); // TestAccount1
                Assert.IsTrue(features.Contains(Feature3)); // TestAccount1

                // validate test account has all the flights with parter and country but not hit any other flight
                features = config.GetEligibleFeatures(TestAccount2, "cart", "gb");
                Assert.AreEqual(3, features.Count);
                Assert.IsTrue(features.Contains(Feature1)); // TestAccount2
                Assert.IsTrue(features.Contains(Feature2)); // TestAccount2
                Assert.IsTrue(features.Contains(Feature3)); // TestAccount2

                // validate test account has all the flights and both parter and counry matche flights and the flights has been deduped
                features = config.GetEligibleFeatures(TestAccount2, "xbox", "us");
                Assert.AreEqual(3, features.Count);
                Assert.IsTrue(features.Contains(Feature1)); // TestAccount2 or xbox
                Assert.IsTrue(features.Contains(Feature2)); // TestAccount2 or us
                Assert.IsTrue(features.Contains(Feature3)); // TestAccount2 or all available

                // validate the flights available to 100% is added correctly
                features = config.GetEligibleFeatures(null, "cart", "gb");
                Assert.AreEqual(1, features.Count);
                Assert.IsTrue(features.Contains(Feature3)); // add by available to all

                // validate match by partner only 
                features = config.GetEligibleFeatures(null, "amcweb", "gb");
                Assert.AreEqual(2, features.Count);
                Assert.IsTrue(features.Contains(Feature1)); // add by amcweb
                Assert.IsTrue(features.Contains(Feature3)); // add by available to all

                // validate match by country only 
                features = config.GetEligibleFeatures(null, "cart", "cn");
                Assert.AreEqual(2, features.Count);
                Assert.IsTrue(features.Contains(Feature2)); // add by cn
                Assert.IsTrue(features.Contains(Feature3)); // add by available to all

                // validate case insensitive when filter out features
                features = config.GetEligibleFeatures(null, "XBOX", "US");
                Assert.AreEqual(3, features.Count);
                Assert.IsTrue(features.Contains(Feature1)); // add by xbox
                Assert.IsTrue(features.Contains(Feature2)); // add by us
                Assert.IsTrue(features.Contains(Feature3)); // add by available to all

                features = config.GetEligibleFeatures(TestAccount1LowerCase, null, null);
                Assert.AreEqual(3, features.Count);
                Assert.IsTrue(features.Contains(Feature1)); // add by test account 1
                Assert.IsTrue(features.Contains(Feature2)); // add by test account 1
                Assert.IsTrue(features.Contains(Feature3)); // add by test account 1
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Only single condition is allowed")]
        public void TestInvalidFeatureFlagConfig_MultipleConditionsPerFeature()
        {
            var featureConfigs = new Dictionary<string, Dictionary<string, string>>()
            {
                { Feature1,
                    new Dictionary<string, string>()
                    {
                        {
                            Partners, "xbox,amcweb,xbox"
                        },
                        {
                            Countries, "us,cn,cn"
                        }
                    }
                }
            };

            var testAccountIds = new List<string>()
            {
                TestAccount1, TestAccount1, TestAccount2
            };

            LocalFeatureConfigs config = new LocalFeatureConfigs(featureConfigs, testAccountIds);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Only countries, partners and available_to_all are allowed")]
        public void TestInvalidFeatureFlagConfig_NotAllowedConditionTag()
        {
            var featureConfigs = new Dictionary<string, Dictionary<string, string>>()
            {
                { Feature1,
                    new Dictionary<string, string>()
                    {
                        {
                            "notallowconditiontag", "xbox,amcweb,xbox"
                        },
                        {
                            Countries, "us,cn,cn"
                        }
                    }
                }
            };

            var testAccountIds = new List<string>()
            {
                TestAccount1, TestAccount1, TestAccount2
            };

            LocalFeatureConfigs config = new LocalFeatureConfigs(featureConfigs, testAccountIds);
        }

        [TestMethod]
        public void TestMergeRemoteLocalFeatureConfigs()
        {
            var featureConfigs = new Dictionary<string, Dictionary<string, string>>()
            {
                { Feature1,
                    new Dictionary<string, string>()
                    {
                        {
                            Partners, "xbox,amcweb,xbox"
                        }
                    }
                }
            };

            var testAccountIds = new List<string>()
            {
                TestAccount1, TestAccount1, TestAccount2
            };

            var remoteFeatures = new List<string>()
            {
                Feature1, Feature2
            };

            var mergedFeatures = LocalFeatureConfigs.MergeMatchedEligibleLocalAndRemoteFeatures(
                accountId: "dummy_test_account",
                partner: "xbox",
                country: "us",
                new LocalFeatureConfigs(featureConfigs, testAccountIds),
                remoteFeatures,
                new EventTraceActivity());
            Assert.AreEqual(3, mergedFeatures.Count);
            Assert.AreEqual(mergedFeatures[0], Feature1);
            Assert.AreEqual(mergedFeatures[1], Feature2);
            Assert.AreEqual(mergedFeatures[2], Feature1);
        }

        [TestMethod]
        public void TestMergeRemoteLocalFeatureConfigs_AllRemoteFeaturesInLocal()
        {
            var featureConfigs = new Dictionary<string, Dictionary<string, string>>()
            {
                { Feature1,
                    new Dictionary<string, string>()
                    {
                        {
                            Partners, "xbox,amcweb,xbox"
                        }
                    }
                },
                { Feature2,
                    new Dictionary<string, string>()
                    {
                        {
                            Partners, "xbox,amcweb"
                        }
                    }
                }
            };

            var testAccountIds = new List<string>()
            {
                TestAccount1, TestAccount1, TestAccount2
            };

            var remoteFeatures = new List<string>()
            {
                Feature1, Feature2
            };

            var mergedFeatures = LocalFeatureConfigs.MergeMatchedEligibleLocalAndRemoteFeatures(
                accountId: "dummy_test_account",
                partner: "xbox",
                country: "us",
                new LocalFeatureConfigs(featureConfigs, testAccountIds),
                remoteFeatures,
                new EventTraceActivity());

            // enable to verify the local config, we didn't dedupe when we merge to remote
            // we only append the local config to the end of remote
            // once all remote features are removed. The duplications should be gone.
            Assert.AreEqual(4, mergedFeatures.Count);
            Assert.AreEqual(mergedFeatures[0], Feature1);
            Assert.AreEqual(mergedFeatures[1], Feature2);
            Assert.AreEqual(mergedFeatures[2], Feature1);
            Assert.AreEqual(mergedFeatures[3], Feature2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Only single condition is allowed")]
        public void TestMergeRemoteLocalFeatureConfigs_WithMultipleConditions()
        {
            var featureConfigs = new Dictionary<string, Dictionary<string, string>>()
            {
                { Feature1,
                    new Dictionary<string, string>()
                    {
                        {
                            Partners, "xbox,amcweb,xbox"
                        },
                        {
                            Countries, "us"
                        }
                    }
                }
            };

            var testAccountIds = new List<string>()
            {
                TestAccount1, TestAccount1, TestAccount2
            };

            var remoteFeatures = new List<string>()
            {
                Feature1, Feature2
            };

            var mergedFeatures = LocalFeatureConfigs.MergeMatchedEligibleLocalAndRemoteFeatures(
                accountId: "dummy_test_account",
                partner: "xbox",
                country: "us",
                new LocalFeatureConfigs(featureConfigs, testAccountIds),
                remoteFeatures,
                new EventTraceActivity());
            Assert.AreEqual(3, mergedFeatures.Count);
            Assert.AreEqual(mergedFeatures[0], Feature1);
            Assert.AreEqual(mergedFeatures[1], Feature2);
            Assert.AreEqual(mergedFeatures[2], Feature1);
        }

        [TestMethod]
        public void TestMergeRemoteLocalFeatureConfigs_EmptyLocalFeatureConfigsReturnRemoteConfig()
        {
            Dictionary<string, Dictionary<string, string>> featureConfigs = null;

            List<string> testAccountIds = null;

            var configs = new List<LocalFeatureConfigs>()
            {
                null,
                new LocalFeatureConfigs(featureConfigs, testAccountIds)
            };
 
            var remoteFeatures = new List<string>()
            {
                Feature1, Feature2
            };

            foreach (var localConfig in configs)
            {
                var mergedFeatures = LocalFeatureConfigs.MergeMatchedEligibleLocalAndRemoteFeatures(
                    accountId: "dummy_test_account",
                    partner: "xbox",
                    country: "us",
                    localConfig,
                    remoteFeatures,
                    new EventTraceActivity());

                Assert.AreEqual(2, mergedFeatures.Count);
                Assert.AreEqual(mergedFeatures[0], Feature1);
                Assert.AreEqual(mergedFeatures[1], Feature2);
            }
        }

        [TestMethod]
        public void TestFeatureConfigsCount()
        {
            // "62dc8681-6753-484a-981a-128f82a43d25" is a test account in int_test_user_group
            int totalFeatureConfigs = 20;
            Assert.AreEqual(totalFeatureConfigs, PXSettings.LocalFeatureConfigs.GetEligibleFeatures("62dc8681-6753-484a-981a-128f82a43d25", "webblends", "us").Count);
        }
    }
}
