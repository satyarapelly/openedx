// <copyright company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Helpers;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using PXCommon = Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Test.Common;
    using Newtonsoft.Json;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;

    [TestClass]
    public class GetProfileDescriptionsTests : UnitTestBase
    {
        readonly Dictionary<string, Dictionary<string, string[]>> namedPartnerLists = new Dictionary<string, Dictionary<string, string[]>>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "Profile",
                new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
                {
                    {
                        "Partners-Employee",
                        new string[] { "commercialstores", "defaulttemplate" }
                    },
                    {
                        "Partners-Organization",
                        new string[] { "commercialstores", "defaulttemplate" }
                    },
                    {
                        "Partners-Organization-Update-Partial",
                        new string[] { "smboobe" }
                    },
                    {
                        "Partners-LegalEntity",
                        new string[] { "commercialstores", "defaulttemplate" }
                    },
                    {
                        // Standalone profile update is delivered to webblends only, code is in production but SFPex team hasn't taken it yet
                        "Partners-Consumer",
                        new string[] { "oxodime", "oxowebdirect", "webblends", "storify", "xboxsubs", "xboxsettings", "saturn" }
                    },
                    {
                        "Partners-NotApplicableForProfile",
                        new string[] { "amc", "amcweb", "amcxbox", "appsource", "azure", "azuresignup", "azureibiza", "bing", "bingtravel", "cart", "commercialsupport", "commercialwebblends", "consumersupport", "default", "ggpdeds", "marketplace", "mseg", "northstarweb", "office", "officeoobe", "oxooobe", "officeoobeinapp", "onedrive", "oxowebdirect", "payin", "setupoffice", "setupofficesdx", "storeoffice", "storify", "xboxnative", "xboxsubs", "xboxsettings", "saturn", "test", "webblends_inline", "webpay", "xbox", "xboxweb", "windowsnative", "windowssubs", "windowssettings", "windowsstore", "selectpmdropdown", "selectpmbuttonlist", "selectpmradiobuttonlist", "listpidropdown", "listpiradiobutton", "listpibuttonlist" }
                    },
                    {
                        // Skip profile tests for wallet since wallet converts identity to identity + "wallet" format
                        "Partners-Skip",
                        new string[] { "wallet", "msteams", "onepage", "twopage", "secondscreentemplate", "consoletemplate" }
                    },
                }
            },
            {
                "SubmitLinks",
                new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
                {
                    {
                        "Partners-Skip",
                        new string[] { "msteams", "selectpmbuttonlist", "selectpmradiobuttonlist", "selectpmdropdown", "listpidropdown", "listpiradiobutton", "listpibuttonlist", "onepage", "twopage", "secondscreentemplate", "consoletemplate" }
                    },
                    {
                        "Partners-ProfileEmployeeV2WithSubmitLink",
                        new string[] { }
                    },
                    {
                        "Partners-ProfileEmployeeV3WithSubmitLink",
                        new string[] { "commercialstores", "azure", "azuresignup", "azureibiza", "defaulttemplate" }
                    },
                    {
                        "Partners-ProfileEmployeeWithSubmitLinkDoesNotExist",
                        new string[] { "amc", "amcweb", "amcxbox", "appsource", "bing", "bingtravel", "cart", "commercialsupport", "commercialwebblends", "consumersupport", "default", "ggpdeds", "marketplace", "mseg", "northstarweb", "office", "officeoobe", "oxooobe", "officeoobeinapp", "onedrive", "oxodime", "oxowebdirect", "payin", "setupoffice", "setupofficesdx", "smboobe", "storeoffice", "storify", "xboxnative", "xboxsubs", "xboxsettings", "saturn", "test", "wallet", "webblends", "webblends_inline", "webpay", "xbox", "xboxweb", "windowssettings", "windowsstore", "windowsnative", "windowssubs" }
                    },
                    //// new subset group
                    {
                        "Partners-ProfileOrganizationAddV2WithSubmitLink",
                        new string[] { }
                    },
                    {
                        "Partners-ProfileOrganizationAddV3WithSubmitLink",
                        new string[] { "commercialstores", "defaulttemplate" }
                    },
                    {
                        "Partners-ProfileOrganizationAddWithSubmitLinkDoesNotExist",
                        new string[] { "amc", "amcweb", "amcxbox", "appsource", "azure", "azuresignup", "azureibiza", "bing", "bingtravel", "cart", "commercialsupport", "commercialwebblends", "consumersupport", "default", "ggpdeds", "marketplace", "mseg", "northstarweb", "office", "officeoobe", "oxooobe", "officeoobeinapp", "onedrive", "oxodime", "oxowebdirect", "payin", "setupoffice", "setupofficesdx", "smboobe", "storeoffice", "storify", "xboxnative", "xboxsubs", "xboxsettings", "saturn", "test", "wallet", "webblends", "webblends_inline", "webpay", "xbox", "xboxweb", "windowssettings", "windowsstore", "windowsnative", "windowssubs" }
                    },
                    //// new subset group
                    {
                        "Partners-ProfileOrganizationUpdateV2WithSubmitLink",
                        new string[] { }
                    },
                    {
                        "Partners-ProfileOrganizationUpdateV3WithSubmitLink",
                        new string[] { "commercialstores", "defaulttemplate" }
                    },
                    {
                        "Partners-ProfileOrganizationUpdateWithSubmitLinkDoesNotExist",
                        new string[] { "amc", "amcweb", "amcxbox", "appsource", "azure", "azuresignup", "azureibiza", "bing", "bingtravel", "cart", "commercialsupport", "commercialwebblends", "consumersupport", "default", "ggpdeds", "marketplace", "mseg", "northstarweb", "office", "officeoobe", "oxooobe", "officeoobeinapp", "onedrive", "oxodime", "oxowebdirect", "payin", "setupoffice", "setupofficesdx", "smboobe", "storeoffice", "storify", "xboxnative", "xboxsubs", "xboxsettings", "saturn", "test", "wallet", "webblends", "webblends_inline", "webpay", "xbox", "xboxweb", "windowssettings", "windowsstore", "windowsnative", "windowssubs" }
                    },
                    //// new subset group
                    {
                        "Partners-ProfileOrganizationUpdatePatchV2WithSubmitLink",
                        new string[] { }
                    },
                    {
                        "Partners-ProfileOrganizationUpdatePatchV3WithSubmitLink",
                        new string[] { }
                    },
                    {
                        "Partners-ProfileOrganizationUpdatePatchWithSubmitLinkDoesNotExist",
                        new string[] { "amc", "amcweb", "amcxbox", "appsource", "azure", "azuresignup", "azureibiza", "bing", "bingtravel", "cart", "commercialsupport", "commercialstores", "commercialwebblends", "consumersupport", "default", "ggpdeds", "marketplace", "mseg", "northstarweb", "office", "officeoobe", "oxooobe", "officeoobeinapp", "onedrive", "oxodime", "oxowebdirect", "payin", "setupoffice", "setupofficesdx", "smboobe", "storeoffice", "storify", "xboxnative", "xboxsubs", "xboxsettings", "saturn", "test", "wallet", "webblends", "webblends_inline", "webpay", "xbox", "xboxweb", "windowssettings", "windowsnative", "windowsstore", "windowssubs", "defaulttemplate" }
                    },
                    //// new subset group
                    {
                        "Partners-ProfileOrganizationDisableTaxUpdateV2WithSubmitLink",
                        new string[] { }
                    },
                    {
                        "Partners-ProfileOrganizationDisableTaxUpdateV3WithSubmitLink",
                        new string[] { "commercialstores", "defaulttemplate" }
                    },
                    {
                        "Partners-ProfileOrganizationDisableTaxUpdateWithSubmitLinkDoesNotExist",
                        new string[] { "amc", "amcweb", "amcxbox", "appsource", "azure", "azuresignup", "azureibiza", "bing", "bingtravel", "cart", "commercialsupport", "commercialwebblends", "consumersupport", "default", "ggpdeds", "marketplace", "mseg", "northstarweb", "office", "officeoobe", "oxooobe", "officeoobeinapp", "onedrive", "oxodime", "oxowebdirect", "payin", "setupoffice", "setupofficesdx", "smboobe", "storeoffice", "storify", "xboxnative", "xboxsubs", "xboxsettings", "saturn", "test", "wallet", "webblends", "webblends_inline", "webpay", "xbox", "xboxweb", "windowssettings", "windowsstore", "windowsnative", "windowssubs" }
                    },
                    //// new subset group
                    {
                        "Partners-ProfileOrganizationDisableTaxUpdatePatchV2WithSubmitLink",
                        new string[] { }
                    },
                    {
                        "Partners-ProfileOrganizationDisableTaxUpdatePatchV3WithSubmitLink",
                        new string[] { }
                    },
                    {
                        "Partners-ProfileOrganizationDisableTaxUpdatePatchWithSubmitLinkDoesNotExist",
                        new string[] { "amc", "amcweb", "amcxbox", "appsource", "azure", "azuresignup", "azureibiza", "bing", "bingtravel", "cart", "commercialstores", "commercialsupport", "commercialwebblends", "consumersupport", "default", "ggpdeds", "marketplace", "mseg", "northstarweb", "office", "officeoobe", "oxooobe", "officeoobeinapp", "onedrive", "oxodime", "oxowebdirect", "payin", "setupoffice", "setupofficesdx", "smboobe", "storeoffice", "storify", "xboxnative", "xboxsubs", "xboxsettings", "saturn", "test", "wallet", "webblends", "webblends_inline", "webpay", "xbox", "xboxweb", "windowssettings", "windowsnative", "windowsstore", "windowssubs", "defaulttemplate" }
                    },
                    //// new subset group
                    {
                        "Partners-ProfileLegalEntityV2WithSubmitLink",
                        new string[] { "test" }
                    },
                    {
                        "Partners-ProfileLegalEntityV3WithSubmitLink",
                        new string[] { "commercialstores", "defaulttemplate" }
                    },
                    {
                        "Partners-ProfileLegalEntityWithSubmitLinkDoesNotExist",
                        new string[] { "amc", "amcweb", "amcxbox", "appsource", "azure", "azuresignup", "azureibiza", "bing", "bingtravel", "cart", "commercialsupport", "commercialwebblends", "consumersupport", "default", "ggpdeds", "marketplace", "mseg", "northstarweb", "office", "officeoobe", "oxooobe", "officeoobeinapp", "onedrive", "oxodime", "oxowebdirect", "payin", "setupoffice", "setupofficesdx", "storeoffice", "smboobe", "storify", "xboxnative", "xboxsubs", "xboxsettings", "saturn", "wallet", "webblends", "webblends_inline", "webpay", "xbox", "xboxweb", "windowssettings", "windowsnative", "windowsstore", "windowssubs" }
                    },
                    //// new subset group
                    {
                        "Partners-ProfileConsumerV2WithSubmitLink",
                        new string[] { "webblends_inline", "xbox" }
                    },
                    {
                        "Partners-ProfileConsumerV3WithSubmitLink",
                        new string[] { }
                    },
                    {
                        "Partners-ProfileConsumerV3PatchWithSubmitLink",
                        new string[] { "oxodime", "oxowebdirect", "webblends" }
                    },
                    {
                        "Partners-ProfileConsumerWithSubmitLinkDoesNotExist",
                        new string[] { "amc", "amcweb", "amcxbox", "appsource", "azure", "azuresignup", "azureibiza", "bing", "bingtravel", "cart", "commercialstores", "commercialsupport", "commercialwebblends", "consumersupport", "default", "ggpdeds", "marketplace", "mseg", "northstarweb", "office", "officeoobe", "oxooobe", "officeoobeinapp", "onedrive", "payin", "setupoffice", "setupofficesdx", "smboobe", "storeoffice", "storify", "xboxnative", "xboxsubs", "xboxsettings", "saturn", "test", "wallet", "webpay", "xboxweb", "windowssettings", "windowsstore", "windowsnative", "windowssubs", "defaulttemplate" }
                    },
                    //// new subset group
                    {
                        "Partners-ProfileConsumerPrerequisitesV2WithSubmitLink",
                        new string[] { "amc", "amcweb", "amcxbox", "appsource", "cart", "consumersupport", "default", "ggpdeds", "mseg", "northstarweb", "officeoobe", "oxooobe", "officeoobeinapp", "onedrive", "oxodime", "oxowebdirect", "setupoffice", "setupofficesdx", "storeoffice", "storify", "xboxnative", "xboxsubs", "xboxsettings", "saturn", "test", "webblends", "webblends_inline", "xbox", "xboxweb", "windowssettings", "windowsnative", "windowsstore", "windowssubs" }
                    },
                    {
                        "Partners-ProfileConsumerPrerequisitesV3WithSubmitLink",
                        new string[] { "commercialstores", "smboobe", "azure", "azuresignup", "azureibiza", "defaulttemplate" /* bug with no address shipping pidl "webpay" */ }
                    },
                    {
                        "Partners-ProfileConsumerPrerequisitesWithSubmitLinkDoesNotExist",
                        new string[] { "bing", "bingtravel", "commercialsupport", "commercialwebblends", "marketplace", "office", "payin", "wallet", "webpay" }
                    },
                    //// new subset group
                    {
                        "Partners-ProfileConsumerPrerequisitesV2OverridesToV3UpdateWithSubmitLink",
                        new string[] { "amc", "amcweb", "amcxbox", "appsource", "azure", "azuresignup", "azureibiza", "cart", "consumersupport", "default", "ggpdeds", "mseg", "northstarweb", "officeoobe", "oxooobe", "officeoobeinapp", "onedrive", "setupoffice", "setupofficesdx", "smboobe", "storeoffice", "test", "webblends_inline", "xbox", "xboxweb", "windowssettings", "windowsnative", "windowsstore", "windowssubs", "xboxnative" }
                    },
                    {
                        "Partners-ProfileConsumerPrerequisitesV2OverridesToV3UpdatePatchWithSubmitLink",
                        new string[] { "oxodime", "oxowebdirect", "webblends", "storify", "xboxsubs", "xboxsettings", "saturn" }
                    },
                    {
                        "Partners-ProfileConsumerPrerequisitesV2OverrideDoesNotExist",
                        new string[] { "bing", "bingtravel", "commercialstores", "commercialsupport", "commercialwebblends", "marketplace", "office", "payin", "wallet", "webpay", "defaulttemplate" }
                    },
                    {
                        "Partners-ProfileEmployeePrerequisitesV3WithSubmitLink",
                        new string[] { "commercialstores", "azure", "azuresignup", "azureibiza", "defaulttemplate" }
                    },
                }
            }
        };

        readonly List<string> profileSubmitButtonHintIds = new List<string>() { "submitButton", "saveButton", "saveButtonHidden", "saveNextButton" };

        /// <summary>
        /// This test ensures that all partners are covered by the CITs for the profile subsets.
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.TestCoverage)]
        public void GetProfileDescriptions_EnsureCITsCoverAllProfileSubsets()
        {
            List<string[]> subsets = new List<string[]>() { namedPartnerLists["SubmitLinks"]["Partners-ProfileEmployeeV3WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileEmployeeV2WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileEmployeeWithSubmitLinkDoesNotExist"], namedPartnerLists["SubmitLinks"]["Partners-Skip"] };
            UnitTestBase.TestPartnerSetCoverage(subsets, "Partners with V3 Employee profile, V2, and all others");

            subsets = new List<string[]>() { namedPartnerLists["SubmitLinks"]["Partners-ProfileOrganizationAddV2WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileOrganizationAddV3WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileOrganizationAddWithSubmitLinkDoesNotExist"], namedPartnerLists["SubmitLinks"]["Partners-Skip"] };
            UnitTestBase.TestPartnerSetCoverage(subsets, "Partners with Add V3 Organization profile, V2, and all others");

            subsets = new List<string[]>() { namedPartnerLists["SubmitLinks"]["Partners-ProfileOrganizationUpdateV2WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileOrganizationUpdateV3WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileOrganizationUpdateWithSubmitLinkDoesNotExist"], namedPartnerLists["SubmitLinks"]["Partners-Skip"] };
            UnitTestBase.TestPartnerSetCoverage(subsets, "Partners with Update V3 Organization profile, V2, and all others");

            subsets = new List<string[]>() { namedPartnerLists["SubmitLinks"]["Partners-ProfileOrganizationUpdatePatchV2WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileOrganizationUpdatePatchV3WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileOrganizationUpdatePatchWithSubmitLinkDoesNotExist"], namedPartnerLists["SubmitLinks"]["Partners-Skip"] };
            UnitTestBase.TestPartnerSetCoverage(subsets, "Partners with UpdatePatch V3 Organization profile, V2, and all others");

            subsets = new List<string[]>() { namedPartnerLists["SubmitLinks"]["Partners-ProfileOrganizationDisableTaxUpdateV2WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileOrganizationDisableTaxUpdateV3WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileOrganizationDisableTaxUpdateWithSubmitLinkDoesNotExist"], namedPartnerLists["SubmitLinks"]["Partners-Skip"] };
            UnitTestBase.TestPartnerSetCoverage(subsets, "Partners with Update V3 OrganizationDisableTax profile, V2, and all others");

            subsets = new List<string[]>() { namedPartnerLists["SubmitLinks"]["Partners-ProfileOrganizationDisableTaxUpdatePatchV2WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileOrganizationDisableTaxUpdatePatchV3WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileOrganizationDisableTaxUpdatePatchWithSubmitLinkDoesNotExist"], namedPartnerLists["SubmitLinks"]["Partners-Skip"] };
            UnitTestBase.TestPartnerSetCoverage(subsets, "Partners with UpdatePatch V3 OrganizationDisableTax profile, V2, and all others");

            subsets = new List<string[]>() { namedPartnerLists["SubmitLinks"]["Partners-ProfileLegalEntityV2WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileLegalEntityV3WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileLegalEntityWithSubmitLinkDoesNotExist"], namedPartnerLists["SubmitLinks"]["Partners-Skip"] };
            UnitTestBase.TestPartnerSetCoverage(subsets, "Partners with V3 LegalEntity profile, V2, and all others");

            subsets = new List<string[]>() { namedPartnerLists["SubmitLinks"]["Partners-ProfileConsumerV2WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileConsumerV3WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileConsumerV3PatchWithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileConsumerWithSubmitLinkDoesNotExist"], namedPartnerLists["SubmitLinks"]["Partners-Skip"] };
            UnitTestBase.TestPartnerSetCoverage(subsets, "Partners with V3 Consumer profile, V2, and all others");

            subsets = new List<string[]>() { namedPartnerLists["SubmitLinks"]["Partners-ProfileConsumerPrerequisitesV2WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileConsumerPrerequisitesV3WithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileConsumerPrerequisitesWithSubmitLinkDoesNotExist"], namedPartnerLists["SubmitLinks"]["Partners-Skip"] };
            UnitTestBase.TestPartnerSetCoverage(subsets, "Partners with V3 ConsumerPrerequisites profile, V2, and all others");

            subsets = new List<string[]>() { namedPartnerLists["SubmitLinks"]["Partners-ProfileConsumerPrerequisitesV2OverridesToV3UpdateWithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileConsumerPrerequisitesV2OverridesToV3UpdatePatchWithSubmitLink"], namedPartnerLists["SubmitLinks"]["Partners-ProfileConsumerPrerequisitesV2OverrideDoesNotExist"], namedPartnerLists["SubmitLinks"]["Partners-Skip"] };
            UnitTestBase.TestPartnerSetCoverage(subsets, "Partners with V2 ConsumerPrerequisites profile that overrides to V3 Update, UpdatePatch, and all others");
        }

        /// <summary>
        /// This test ensures that all partners are covered by the CITs for the profile.
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.TestCoverage)]
        public void GetProfileDescriptions_EnsureCITsCoverAllPartners()
        {
            // Arrange
            var allProfilePartners = namedPartnerLists["Profile"]["Partners-Employee"]
                .Concat(namedPartnerLists["Profile"]["Partners-Organization"])
                .Concat(namedPartnerLists["Profile"]["Partners-Organization-Update-Partial"])
                .Concat(namedPartnerLists["Profile"]["Partners-LegalEntity"])
                .Concat(namedPartnerLists["Profile"]["Partners-Consumer"])
                .Concat(namedPartnerLists["Profile"]["Partners-NotApplicableForProfile"])
                .Concat(namedPartnerLists["Profile"]["Partners-Skip"]);

            // Assert
            CollectionAssert.AreEquivalent(TestConstants.AllPartners.ToList(), allProfilePartners.Distinct().ToList(), "CIT to verify standalone profile is expected to cover all partners");
        }

        [DataRow("Partners-ProfileEmployeeV2WithSubmitLink", "employee", "add", "us", null, false, "https://{pifd-endpoint}/users/{userId}/profiles", "POST", false)]
        [DataRow("Partners-ProfileEmployeeV3WithSubmitLink", "employee", "add", "us", null, false, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles", "POST", false)]
        [DataRow("Partners-ProfileEmployeeV2WithSubmitLink", "employee", "update", "us", null, false, "https://{pifd-endpoint}/users/{userId}/profiles/testprofileid/update", "POST", false)]
        [DataRow("Partners-ProfileEmployeeV3WithSubmitLink", "employee", "update", "us", null, true, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/testprofileid", "PUT", false)]
        [DataRow("Partners-ProfileEmployeeV3WithSubmitLink", "employee", "update_partial", "us", null, true, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/testprofileid", "PATCH", false)]
        [DataRow("Partners-ProfileEmployeeV3WithSubmitLink", "employee", "update_partial", "us", null, true, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/testprofileid", "PATCH", true)]
        [DataRow("Partners-ProfileOrganizationUpdateV2WithSubmitLink", "organization", "update", "us", null, false, "https://{pifd-endpoint}/users/{userId}/profiles/testprofileid/update", "POST", false)]
        [DataRow("Partners-ProfileOrganizationUpdateV3WithSubmitLink", "organization", "update", "us", null, true, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/testprofileid", "PUT", false)]
        [DataRow("Partners-ProfileOrganizationDisableTaxUpdateV2WithSubmitLink", "organizationDisableTax", "update", "tr", null, false, "https://{pifd-endpoint}/users/{userId}/profiles/testprofileid/update", "POST", false)]
        [DataRow("Partners-ProfileOrganizationDisableTaxUpdateV3WithSubmitLink", "organizationDisableTax", "update", "tr", null, true, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/testprofileid", "PUT", false)]
        /*
         * Bug 20397346(commercial stores doesn't define updateLegalEntityCompanyName) causes the following test rows to fail for reasons not related to submit links
         * [DataRow("Partners-ProfileLegalEntityV2WithSubmitLink", "legalEntity", "add", "us", null, false, "https://{hapi-endpoint}/{userId}/customerService/{id}/updateSoldToAddress", "PUT")]
         * [DataRow("Partners-ProfileLegalEntityV3WithSubmitLink", "legalEntity", "add", "us", null, false, "https://{hapi-endpoint}/{userId}/customerService/{id}/updateSoldToAddress", "PUT")]
         * [DataRow("Partners-ProfileLegalEntityV2WithSubmitLink", "legalEntity", "update", "us", null, false, "https://{hapi-endpoint}/{userId}/customerService/{id}/updateSoldToAddress", "PUT")]
         * [DataRow("Partners-ProfileLegalEntityV3WithSubmitLink", "legalEntity", "update", "us", null, false, "https://{hapi-endpoint}/{userId}/customerService/{id}/updateSoldToAddress", "PUT")]
         */
        [DataRow("Partners-ProfileConsumerV2WithSubmitLink", "consumer", "add", "us", null, false, "https://{pifd-endpoint}/users/{userId}/profiles", "POST", false)]
        [DataRow("Partners-ProfileConsumerV3WithSubmitLink", "consumer", "add", "us", null, false, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles", "POST", false)]
        [DataRow("Partners-ProfileConsumerV2WithSubmitLink", "consumer", "update", "us", null, true, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/testprofileid", "PUT", false)]
        [DataRow("Partners-ProfileConsumerV3WithSubmitLink", "consumer", "update", "us", null, true, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/testprofileid", "PUT", false)]
        [DataRow("Partners-ProfileConsumerV3PatchWithSubmitLink", "consumer", "update", "us", null, true, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/{id}", "PATCH", false)]
        [DataRow("Partners-ProfileConsumerPrerequisitesV2WithSubmitLink", "consumerPrerequisites", "add", "us", null, false, "https://{pifd-endpoint}/users/{userId}/profiles", "POST", false)]
        [DataRow("Partners-ProfileConsumerPrerequisitesV3WithSubmitLink", "consumerPrerequisites", "add", "us", null, false, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles", "POST", false)]
        [DataRow("Partners-ProfileConsumerPrerequisitesV2WithSubmitLink", "consumerPrerequisites", "update", "us", null, false, "https://{pifd-endpoint}/users/{userId}/profiles/testprofileid/update", "POST", false)]
        [DataRow("Partners-ProfileConsumerPrerequisitesV3WithSubmitLink", "consumerPrerequisites", "update", "us", null, false, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/testprofileid", "PUT", false)]
        [DataRow("Partners-ProfileConsumerPrerequisitesV2OverridesToV3UpdateWithSubmitLink", "consumerPrerequisites", "add", "us", null, true, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles", "POST", false)]
        [DataRow("Partners-ProfileConsumerPrerequisitesV2OverridesToV3UpdateWithSubmitLink", "consumerPrerequisites", "update", "us", null, true, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/testprofileid", "PUT", false)]
        [DataRow("Partners-ProfileConsumerPrerequisitesV2OverridesToV3UpdatePatchWithSubmitLink", "consumerPrerequisites", "add", "us", null, true, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles", "POST", false)]
        [DataRow("Partners-ProfileConsumerPrerequisitesV2OverridesToV3UpdatePatchWithSubmitLink", "consumerPrerequisites", "update", "us", null, true, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/{id}", "PATCH", false)]
        /*
         * Task 20658227: [PxService] Remove old "shipping_v3", "emp profile", "org profile" PIDL
           After removing all PATCH flight code, change the following two rows' operatial (remove "_partial")
         */
        [DataRow("Partners-ProfileEmployeePrerequisitesV3WithSubmitLink", "employee", "add_partial", "us", null, false, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles", "POST", false)]
        [DataRow("Partners-ProfileEmployeePrerequisitesV3WithSubmitLink", "employee", "update_partial", "us", null, false, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/testprofileid", "PATCH", false)]
        [DataRow("Partners-ProfileEmployeePrerequisitesV3WithSubmitLink", "employee", "update_partial", "us", null, false, "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/testprofileid", "PATCH", true)]

        [DataTestMethod]
        public void GetProfileDescriptions_ProfileSubmitLink(string partnerGroup, string identity, string operation, string country, string scenario, bool overrideJarvisVersionToV3, string expectedHref, string expectedMethod, bool useHapiProfileSubmit = false)
        {
            // Arrange
            foreach (string partner in namedPartnerLists["SubmitLinks"][partnerGroup])
            {
                PaymentExperienceSetting setting = null;
                this.TestContext.WriteLine("Start testing: Partner \"{0}\"", partner);

                if (useHapiProfileSubmit == true
                    && string.Equals(partner, TestConstants.TemplateNames.DefaultTemplate, StringComparison.OrdinalIgnoreCase))
                {
                    string featureName = identity == "employee" ? "useEmployeeProfileUpdateToHapi" : "useProfileUpdateToHapi";
                    string settingJsonString = "{\"template\":\"defaulttemplate\",\"features\":{\"" + featureName + "\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}";
                    setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(settingJsonString);
                }

                // Act
                List<PIDLResource> profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(country, identity, operation, "en-US", partner, null, profileId: (operation == "add" || operation == "add_partial") ? null : "testprofileid", overrideJarvisVersionToV3: overrideJarvisVersionToV3, scenario: scenario, partnerFlights: null, setting: setting);
                PXCommon.RestLink submitActionContext = null;

                // Assert
                UnitTestBase.AssertSubmitHintExists(profilePidls, profileSubmitButtonHintIds, "Profile", out submitActionContext);

                // Replace the {jarvis-endpoint} with {hapi-endpoint} for the DefaultTemplate partner. We use the jarvis endpoint when flighting is not needed.
                // However, if flighting is enabled or a template partner is available, the hapi-endpoint is utilized instead of jarvis.
                if (useHapiProfileSubmit == true
                    && string.Equals(partner, TestConstants.TemplateNames.DefaultTemplate, StringComparison.OrdinalIgnoreCase))
                {
                    expectedHref = expectedHref.Replace("https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/testprofileid", "https://{hapi-endpoint}/me/profiles/{id}");
                }

                Assert.AreEqual(expectedHref, submitActionContext.Href, "Profile submit display hint action context is expected to match the href provided");
                Assert.AreEqual(expectedMethod, submitActionContext.Method, "Profile submit display hint action context is expected to match the method provided");
                this.TestContext.WriteLine("...done");
            }
        }

        [DataRow("profile", "Partners-LegalEntity", "legalentity", true)]
        [DataRow("profile", "Partners-Consumer", "legalentity", false)]
        [DataRow("profile", "Partners-NotApplicableForProfile", "legalentity", false)]
        [DataTestMethod]
        public void GetProfileDescriptions_ShowProfileTest(string partnerListIdentity, string partnerListName, string type, bool hasDisplayDescription)
        {
            // Arrange
            // Test show profile for type organization and legalentity
            // Not available for other types
            const string Language = "en-us";
            const string Country = "us";
            const string Operation = TestConstants.PidlOperationTypes.Show;

            foreach (var partner in namedPartnerLists[partnerListIdentity][partnerListName])
            {
                PaymentExperienceSetting setting = null;
                this.TestContext.WriteLine("Start testing profile show scenario: Country \"{0}\", Partner \"{1}\", Type \"{2}\"", Country, partner, type);

                if (string.Equals(partner, TestConstants.TemplateNames.DefaultTemplate, StringComparison.OrdinalIgnoreCase))
                {
                    string settingJsonString = "{\"template\":\"defaulttemplate\",\"features\":{\"addUpdatePartnerActionToEditProfileHyperlink\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}";
                    setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(settingJsonString);
                }

                // Act
                List<PIDLResource> profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(Country, type, Operation, Language, partner, setting: setting);

                // Assert
                if (hasDisplayDescription)
                {
                    // Show Profile pidl uses client side prefill, dataSource is expected
                    PidlAssert.IsValid(profilePidls, 1, clientSidePrefill: true);

                    // Validate action in edit profile link
                    ValidatePidlActionForProfile(profilePidls[0]);
                }
                else
                {
                    // For not supported partners, display description should be null
                    PidlAssert.IsValid(profilePidls, 1, displayDescription: false, clientSidePrefill: true);
                }
            }
        }

        [DataRow("profile", "add", "Partners-Employee", "employee", true)]
        [DataRow("profile", "add", "Partners-NotApplicableForProfile", "employee", false)]
        [DataRow("profile", "add", "Partners-NotApplicableForProfile", "consumer", false)]
        [DataRow("profile", "add_partial", "Partners-Employee", "employee", true)]
        [DataTestMethod]
        public void GetProfileDescriptions_AddProfileTest(string partnerListIdentity, string operation, string partnerListName, string type, bool hasDisplayDescription)
        {
            // Arrange
            // Test show profile for type employee and consumer
            // Not available for other types
            const string Language = "en-us";
            const string Country = "us";

            foreach (var partner in namedPartnerLists[partnerListIdentity][partnerListName])
            {
                this.TestContext.WriteLine("Start testing profile add scenario: Country \"{0}\", Partner \"{1}\", Type \"{2}\"", Country, partner, type);

                // Act
                List<PIDLResource> profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(Country, type, operation, Language, partner);

                // Assert
                if (hasDisplayDescription)
                {
                    // DataSource is not expected for add scenario
                    PidlAssert.IsValid(profilePidls, 1, clientSidePrefill: false);

                    // Validate action in submit button
                    // Profile add uses POST method
                    ValidateSubmitButtonForProfile(profilePidls[0], TestConstants.HTTPVerbs.Post);
                }
                else
                {
                    // For not supported partners, display description should be null
                    PidlAssert.IsValid(profilePidls, 1, displayDescription: false, clientSidePrefill: false);
                }
            }
        }

        [DataRow("Profile", "update", "Partners-Organization", "organization", true)]
        [DataRow("Profile", "update", "Partners-Employee", "employee", true)]
        [DataRow("Profile", "update", "Partners-Consumer", "consumer", true)]
        [DataRow("Profile", "update", "Partners-NotApplicableForProfile", "organization", false)]
        [DataRow("Profile", "update", "Partners-NotApplicableForProfile", "employee", false)]
        [DataRow("Profile", "update", "Partners-NotApplicableForProfile", "consumer", false)]
        [DataTestMethod]
        public void GetProfileDescriptions_UpdateProfileServicePrefillTest(string partnerListIdentity, string operation, string partnerListName, string type, bool hasDisplayDescription)
        {
            // Arrange
            // Server prefill update profile is currently in prod and has heavy prod traffic
            // Later this approach will be merged to client prefill
            const string Language = "en-us";
            const string Country = "us";

            foreach (var partner in namedPartnerLists[partnerListIdentity][partnerListName])
            {
                this.TestContext.WriteLine("Start testing profile update with server side prefill scenario: Country \"{0}\", Partner \"{1}\", Type \"{2}\"", Country, partner, type);

                // Add custom headers to GetProfileDescriptions, custom headers should be append to PIDL action in the return pidl
                Dictionary<string, string> profileV3Headers = new Dictionary<string, string>();
                string sourceEtag = "61a12cf1-a1d8-40df-ad55-b619d2572179";
                profileV3Headers.Add(TestConstants.AccountV3ExtendedHttpHeaders.Etag, sourceEtag);
                profileV3Headers.Add(TestConstants.AccountV3ExtendedHttpHeaders.IfMatch, sourceEtag);

                // Act
                List<PIDLResource> profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(Country, type, operation, Language, partner, null, "mockid", profileV3Headers, true, null, null);

                // Assert
                if (hasDisplayDescription)
                {
                    // DataSource is not expected for server side prefill
                    PidlAssert.IsValid(profilePidls, 1, clientSidePrefill: false);

                    // Validate action and header info in submit button
                    // Consumer profile update uses PATCH method, others use PUT method.
                    string jarvisOperation = (string.Equals(partner, "oxowebdirect", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partner, "oxodime", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partner, "webblends", StringComparison.OrdinalIgnoreCase)
                        || Microsoft.Commerce.Payments.PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner)
                        || string.Equals(operation, "update_partial", StringComparison.OrdinalIgnoreCase))
                        ? TestConstants.HTTPVerbs.Patch : TestConstants.HTTPVerbs.Put;
                    ValidateSubmitButtonForProfile(profilePidls[0], jarvisOperation, sourceEtag);
                }
                else
                {
                    // For not supported partners, display description should be null
                    PidlAssert.IsValid(profilePidls, 1, displayDescription: false, clientSidePrefill: false);
                }
            }
        }

        [DataRow("Profile", "update_partial", "Partners-Organization", "organization", null, true)]
        [DataRow("Profile", "update_partial", "Partners-Organization", "organization", "twoColumns", true)]
        [DataRow("Profile", "update_partial", "Partners-Employee", "employee", null, true)]
        [DataRow("Profile", "update", "Partners-Consumer", "consumer", null, true)]
        [DataTestMethod]
        public void GetProfileDescriptions_UpdateProfileClientPrefillTest(string partnerListIdentity, string operation, string partnerListName, string type, string scenario, bool hasDisplayDescription)
        {
            // Arrange
            // Client prefill update profile is currently under flight for emp/org, it will be the final solution for all profiles
            const string Language = "en-us";
            const string Country = "us";

            foreach (var partner in namedPartnerLists[partnerListIdentity][partnerListName])
            {
                this.TestContext.WriteLine("Start testing profile update with server side prefill scenario: Country \"{0}\", Partner \"{1}\", Type \"{2}\"", Country, partner, type);

                // Add custom headers to GetProfileDescriptions, custom headers should be append to PIDL action in the return pidl
                Dictionary<string, string> profileV3Headers = new Dictionary<string, string>();
                string sourceEtag = "template data";
                profileV3Headers.Add(TestConstants.AccountV3ExtendedHttpHeaders.Etag, sourceEtag);
                profileV3Headers.Add(TestConstants.AccountV3ExtendedHttpHeaders.IfMatch, sourceEtag);

                // Act
                List<PIDLResource> profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(Country, type, operation, Language, partner, null, null, profileV3Headers, true, null, scenario);

                // Assert
                if (hasDisplayDescription)
                {
                    // DataSource is expected for server side prefill
                    PidlAssert.IsValid(profilePidls, 1, clientSidePrefill: true);

                    // Validate action and header info in submit button
                    string jarvisOperation = TestConstants.HTTPVerbs.Patch;
                    ValidateSubmitButtonForProfile(profilePidls[0], jarvisOperation, sourceEtag);
                }
            }
        }

        [DataRow("Profile", "Partners-LegalEntity", true)]
        [DataRow("Profile", "Partners-Consumer", false)]
        [DataRow("Profile", "Partners-NotApplicableForProfile", false)]
        [DataTestMethod]
        public void GetProfileDescriptions_UpdateLegalEntityProfileTest(string partnerListIdentity, string partnerListName, bool hasDisplayDescription)
        {
            // Arrange
            // Update legal entity profile talks to HAPI endpoint
            // No extra test is needed for submit button
            const string Language = "en-us";
            const string Country = "us";
            const string Type = "legalentity";
            const string Operation = TestConstants.PidlOperationTypes.Update;

            foreach (var partner in namedPartnerLists[partnerListIdentity][partnerListName])
            {
                this.TestContext.WriteLine("Start testing: Country \"{0}\", Partner \"{1}\", Type \"{2}\"", Country, partner, Type);

                // Act
                List<PIDLResource> profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(Country, Type, Operation, Language, partner, null, "mockid", null, true, null);

                // Assert
                if (hasDisplayDescription)
                {
                    // Patch Profile pidl uses client side prefill, dataSource is expected
                    PidlAssert.IsValid(profilePidls, 1, clientSidePrefill: true);
                }
                else
                {
                    // For not supported partners, display description should be null
                    PidlAssert.IsValid(profilePidls, 1, displayDescription: false, clientSidePrefill: true);
                }
            }
        }

        [DataRow("Profile", "Partners-Organization", "tr", "update", "", 2, false)]
        [DataRow("Profile", "Partners-Organization", "tr", "update_partial", "", 2, false)]
        [DataRow("Profile", "Partners-Organization", "no", "update", "", 2, false)]
        [DataRow("Profile", "Partners-Organization", "no", "update_partial", "", 2, false)]
        [DataRow("Profile", "Partners-Organization", "by", "update", "", 2, false)]
        [DataRow("Profile", "Partners-Organization", "by", "update_partial", "", 2, false)]
        [DataRow("Profile", "Partners-Organization", "am", "update", "", 2, false)]
        [DataRow("Profile", "Partners-Organization", "am", "update_partial", "", 2, false)]
        [DataRow("Profile", "Partners-Organization", "us", "update", "", 1, false)]
        [DataRow("Profile", "Partners-Organization", "us", "update_partial", "", 1, false)]
        [DataRow("Profile", "Partners-Organization", "tr", "update", "", 2, true)]
        [DataRow("Profile", "Partners-Organization", "tr", "update_partial", "", 2, true)]
        [DataRow("Profile", "Partners-Organization", "no", "update", "", 2, true)]
        [DataRow("Profile", "Partners-Organization", "no", "update_partial", "", 2, true)]
        [DataRow("Profile", "Partners-Organization", "by", "update", "", 2, true)]
        [DataRow("Profile", "Partners-Organization", "by", "update_partial", "", 2, true)]
        [DataRow("Profile", "Partners-Organization", "am", "update", "", 2, true)]
        [DataRow("Profile", "Partners-Organization", "am", "update_partial", "", 2, true)]
        [DataRow("Profile", "Partners-Organization", "us", "update", "", 1, true)]
        [DataRow("Profile", "Partners-Organization", "us", "update_partial", "", 1, true)]
        [DataTestMethod]
        public void GetProfileDescriptions_UpdateOrganizationReturnsTwoPidls(string partnerListIdentity, string partnerListName, string country, string operation, string scenario, int expectedPidlCount, bool isFeatureUseProfileJarvis = false)
        {
            // Arrange
            const string Language = "en-us";
            const string Type = "organization";

            foreach (var partner in namedPartnerLists[partnerListIdentity][partnerListName])
            {
                this.TestContext.WriteLine("Start testing organization profile update Turkey case: Country \"{0}\", Partner \"{1}\", Type \"{2}\"", country, partner, Type);
                PaymentExperienceSetting setting = null;

                if (string.Equals(partner, TestConstants.TemplateNames.DefaultTemplate, StringComparison.OrdinalIgnoreCase))
                {
                    // If the feature is enabled, only the template partner will retrieve the number of profile types from the GetProfileTypeIds method.
                    if (isFeatureUseProfileJarvis)
                    {
                        string settingJsonString = "{\"template\":\"defaulttemplate\",\"features\":{\"useMultipleProfile\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}";
                        setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(settingJsonString);
                    }
                    else if (!isFeatureUseProfileJarvis)
                    {
                        expectedPidlCount = 1;
                    }
                }

                // Act
                List<PIDLResource> profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(
                    country,
                    Type,
                    operation,
                    Language,
                    partner,
                    nextPidlLink: null,
                    profileId: "mockid",
                    profileV3Headers: null,
                    overrideJarvisVersionToV3: true,
                    exposedFlightFeatures: null,
                    scenario,
                    setting: setting);

                // Assert
                PidlAssert.IsValid(profilePidls, expectedPidlCount);
            }
        }

        [DataRow("Profile", "Partners-Organization", "organization", "us", "update_partial", "PXProfileUpdateToHapi", "", "https://{hapi-endpoint}/my-org/profiles/{id}", true)]
        [DataRow("Profile", "Partners-Organization", "organization", "us", "update_partial", "", "twoColumns", "https://{hapi-endpoint}/my-org/profiles/{id}", true)]
        [DataRow("Profile", "Partners-Organization-Update-Partial", "organization", "us", "update_partial", "PXProfileUpdateToHapi", "", "https://{hapi-endpoint}/my-org/profiles/{id}", true)]
        [DataRow("Profile", "Partners-Employee", "employee", "us", "update", "PXProfileUpdateToHapi", "", "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles/mockid", false)]
        [DataTestMethod]
        public void GetProfileDescriptions_UpdateToHapi(string partnerListIdentity, string partnerListName, string type, string country, string operation, string flight, string scenario, string url, bool hasErrorCodeExpression)
        {
            // Arrange
            const string Language = "en-us";

            List<string> exposedFlightFeatures = new List<string>();
            if (flight != null)
            {
                exposedFlightFeatures.Add(flight);
            }

            foreach (var partner in namedPartnerLists[partnerListIdentity][partnerListName])
            {
                this.TestContext.WriteLine("Start testing profile update to Hapi: Country \"{0}\", Partner \"{1}\", Type \"{2}\"", country, partner, type);

                // Act
                List<PIDLResource> profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(country, type, operation, Language, partner, null, "mockid", null, true, exposedFlightFeatures, scenario);

                // Assert
                PidlAssert.IsValid(profilePidls);

                ButtonDisplayHint submitButton = profilePidls[0].GetDisplayHintById(TestConstants.ButtonDisplayHintIds.SaveButton) as ButtonDisplayHint;
                PXCommon.RestLink actionContext = submitButton.Action.Context as PXCommon.RestLink;
                Assert.IsTrue(string.Equals(actionContext.Href, url, StringComparison.OrdinalIgnoreCase));
                if (hasErrorCodeExpression)
                {
                    Assert.IsTrue(string.Equals(actionContext.ErrorCodeExpressions[0], "({contextData.error_code})", StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        /// <summary>
        /// This test is used to verify the profile pidl, when used with hidden profile secnario, should not have visible hints.
        /// </summary>
        [TestMethod]
        public void GetProfileDescriptions_EmployeeWithHiddenProfileScenario_ShouldNotHaveVisibleHints()
        {
            // Arrange
            List<PIDLResource> profilePidls = null;
            string country = "us";
            string type = "employee";
            string operation = "add";
            string language = "en-us";
            string[] partners = { "commercialstores", "defaulttemplate" };
            string scenario = "hiddenProfile";
            string expectedSubmitUrl = "https://{jarvis-endpoint}/JarvisCM/{userId}/profiles";

            foreach (string partner in partners)
            {
                // Act
                profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(
                country: country,
                type: type,
                operation: operation,
                language: language,
                partnerName: partner,
                nextPidlLink: null,
                profileId: null,
                profileV3Headers: null,
                overrideJarvisVersionToV3: false,
                exposedFlightFeatures: new List<string>(),
                scenario: scenario);

                // Assert
                PidlAssert.IsValid(profilePidls, 1);
                foreach (PIDLResource profilePidl in profilePidls)
                {
                    IEnumerable<DisplayHint> hints = profilePidl.GetAllDisplayHints().Where(x => x.IsHidden == false);
                    Assert.IsFalse(hints.Any(), "Profile should not have any visible hints when scenario=hiddenProfile");

                    ButtonDisplayHint submitButton = profilePidl.GetDisplayHintById(TestConstants.ButtonDisplayHintIds.SaveButtonHidden) as ButtonDisplayHint;
                    PXCommon.RestLink actionContext = submitButton.Action.Context as PXCommon.RestLink;
                    Assert.IsNotNull(actionContext, "Action context is missing for submit button");
                    Assert.AreEqual(expectedSubmitUrl, actionContext.Href, ignoreCase: true, message: $"Profile should be posted to {expectedSubmitUrl}");
                }
            }
        }

        /// <summary>
        /// This test is used to verify the profile pidl, to check their property name is not null.
        /// </summary>
        [TestMethod]
        public void GetProfileDescriptions_GetPropertyDescriptionByPropertyNameWithFullPath()
        {
            // Arrange
            string country = "us";
            string type = "employee";
            string operation = "add";
            string language = "en-us";
            string[] partners = { "commercialstores", "defaulttemplate" };
            string[] propertyNames = { "default_address.first_name", "default_address.last_name" };

            foreach (string partner in partners)
            {
                // Act
                List<PIDLResource> profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(
                country: country,
                type: type,
                operation: operation,
                language: language,
                partnerName: partner,
                overrideJarvisVersionToV3: false);

                // Assert
                PidlAssert.IsValid(profilePidls, 1);
                foreach (PIDLResource profilePidl in profilePidls)
                {
                    foreach (string propertyName in propertyNames)
                    {
                        PropertyDescription propertyDescription = profilePidl.GetPropertyDescriptionByPropertyNameWithFullPath(propertyName);
                        Assert.IsNotNull(propertyDescription, propertyName + " can't be null");
                    }
                }
            }
        }

        [TestMethod]
        public void GetProfileDescriptions_ConsumerWithoutMetadata()
        {
            string country = "us";
            string type = "consumer";
            string operation = "update";
            string language = "en-us";
            string partner = "webblends";

            List<PIDLResource> profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(
                country: country,
                type: type,
                operation: operation,
                language: language,
                partnerName: partner,
                overrideJarvisVersionToV3: true);

            PidlAssert.IsValid(profilePidls, 1);

            Assert.IsTrue(profilePidls[0].DataSources != null);
            Assert.IsFalse(profilePidls[0].DataDescription.ContainsKey("profileType"), "This property should be removed from commercial tax Pidl");
            Assert.IsFalse(profilePidls[0].DataDescription.ContainsKey("profileCountry"), "This property should be removed from commercial tax Pidl");
            Assert.IsFalse(profilePidls[0].DataDescription.ContainsKey("profileOperation"), "This property should be removed from commercial tax Pidl");
        }

        [DataRow("smboobe", "organization", "us", "update_partial", "PXProfileUpdateToHapi", null, "false", "country,accountHolderName,accountToken,first_name,last_name,address_line1,address_line2,address_line3,city,region,postal_code")]
        [DataRow("smboobe", "organization", "us", "update_partial", "PXProfileUpdateToHapi", "roobe", "true", "")]
        [DataTestMethod]
        public void GetProfileDescriptions_UpdatePartial_ShouldShowDisplayNameAsExpected(string partner, string type, string country, string operation, string flight, string scenario, string showDisplayName, string propertiesToIgnore)
        {
            const string Language = "en-us";

            List<string> exposedFlightFeatures = new List<string>();
            if (flight != null)
            {
                exposedFlightFeatures.Add(flight);
            }

            List<string> excludedProperties = propertiesToIgnore.Split(',').ToList();

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetProfileDescriptions(country, type, operation, Language, partner, null, "mockid", null, true, exposedFlightFeatures, scenario);
            PidlAssert.IsValid(pidls);

            // Select all property displayhints that are not nested members of pages.
            List<DisplayHint> properties = pidls
                .SelectMany(pidl => pidl.DisplayPages)
                .SelectMany(displayPages => displayPages.Members)
                .Where(displayHint => displayHint.DisplayHintType.Equals("property") && !excludedProperties.Contains(displayHint.PropertyName))
                .ToList();

            // Verify all valid properties have the expected value for showDisplayName
            foreach (DisplayHint displayHint in properties)
            {
                Assert.AreEqual(showDisplayName, ((PropertyDisplayHint)displayHint).ShowDisplayName);
            }
        }

        [DataRow("playxbox", "us", "consumerPrerequisites", "add", "https://{pifd-endpoint}/users/{userId}/addressesEx?partner=playxbox&language=en-US&avsSuggest=false", "POST", "")]
        [DataRow("playxbox", "us", "consumerPrerequisites", "update", "https://{pifd-endpoint}/users/{userId}/addressesEx?partner=playxbox&language=en-US&avsSuggest=false", "POST", null)]
        [DataRow("playxbox", "us", "consumerPrerequisites", "add", "https://{pifd-endpoint}/users/{userId}/profiles", "POST", "PXSkipPifdAddressPostForNonAddressesType")]
        [DataRow("playxbox", "us", "consumerPrerequisites", "update", "https://{pifd-endpoint}/users/{userId}/profiles/testprofileid/update", "POST", "PXSkipPifdAddressPostForNonAddressesType")]
        [DataTestMethod]
        public void GetProfileDescriptions_ValidateSubmitLink(string partner, string country, string identity, string operation, string expectedHref, string expectedMethod, string flights)
        {
            PaymentExperienceSetting setting = null;
            List<string> exposedFlightFeatures = string.IsNullOrEmpty(flights) ? null : flights.Split(',').ToList();
            if (string.Equals(partner, TestConstants.PartnerNames.PlayXbox, StringComparison.OrdinalIgnoreCase))
            {
                string settingJsonString = "{\"template\":\"defaulttemplate\",\"features\":{\"useV3AddressPIDL\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"useAddressesExSubmit\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"addressValidation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"verifyAddressPidlModification\":true}]},\"skipJarvisV3ForProfile\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}";
                setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(settingJsonString);
            }

            List<PIDLResource> profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(country, identity, operation, "en-US", partner, null, profileId: operation == "add" ? null : "testprofileid", setting: setting, exposedFlightFeatures: exposedFlightFeatures);
            PXCommon.RestLink submitActionContext = null;

            UnitTestBase.AssertSubmitHintExists(profilePidls, profileSubmitButtonHintIds, "Profile", out submitActionContext);
            Assert.AreEqual(expectedHref, submitActionContext.Href, "Profile submit display hint action context is expected to match the href provided");
            Assert.AreEqual(expectedMethod, submitActionContext.Method, "Profile submit display hint action context is expected to match the method provided");
        }

        private void ValidatePidlActionForProfile(PIDLResource pidl)
        {
            string actionType = "partnerAction";
            string action = "updateResource";
            HyperlinkDisplayHint profileEditHyperlink = pidl.GetDisplayHintById(TestConstants.ButtonDisplayHintIds.ProfileEditLEHyperlinkId) as HyperlinkDisplayHint;

            Assert.IsTrue(profileEditHyperlink != null && profileEditHyperlink.Action != null && profileEditHyperlink.Action.Context != null, "Profile Pidl DisplayDescription validation failed, editLink not found");

            DisplayHintAction displayHintAction = profileEditHyperlink.Action as DisplayHintAction;
            Assert.IsNotNull(displayHintAction, "Profile Pidl DisplayDescription validation failed, edit link action cant be null");
            Assert.IsTrue(string.Equals(displayHintAction.ActionType, actionType, StringComparison.OrdinalIgnoreCase), "Action type is incorrect");

            ActionContext actionContext = displayHintAction.Context as ActionContext;
            Assert.IsNotNull(actionContext, "Profile Pidl DisplayDescription validation failed, edit link action context cant be null");
            Assert.IsTrue(string.Equals(actionContext.Action, action, StringComparison.OrdinalIgnoreCase), "ActionContext action is incorrect");
        }

        private void ValidateSubmitButtonForProfile(PIDLResource pidl, string operation, string sourceValue = null)
        {
            // Get submit button from pild
            // Verify the addtional headers in submit button's actioncontext
            ButtonDisplayHint submitButton = pidl.GetDisplayHintById(TestConstants.ButtonDisplayHintIds.SaveButton) as ButtonDisplayHint;
            if (submitButton == null)
            {
                submitButton = pidl.GetDisplayHintById(TestConstants.ButtonDisplayHintIds.SubmitButton) as ButtonDisplayHint;
            }

            Assert.IsTrue(submitButton != null && submitButton.Action != null && submitButton.Action.Context != null, "Profile Pidl DisplayDescription validation failed, saveButton not found");

            PXCommon.RestLink actionContext = submitButton.Action.Context as PXCommon.RestLink;
            Assert.IsNotNull(actionContext, "Profile Pidl DisplayDescription validation failed, submit button context cant be null");
            Assert.IsTrue(string.Equals(actionContext.Method, operation, StringComparison.OrdinalIgnoreCase), "Context method is incorrect");

            if (sourceValue != null)
            {
                string targetEtag = null;
                Assert.IsTrue(actionContext.Headers.TryGetValue(TestConstants.AccountV3ExtendedHttpHeaders.Etag, out targetEtag), "Etag is not part of context");
                Assert.AreEqual(sourceValue, targetEtag, "Etag is not the same as source");

                string targetIfMatch = null;
                Assert.IsTrue(actionContext.Headers.TryGetValue(TestConstants.AccountV3ExtendedHttpHeaders.IfMatch, out targetIfMatch), "IfMatch is not part of context");
                Assert.AreEqual(sourceValue, targetIfMatch, "IfMatch is not the same as source");
            }
        }
    }
}
