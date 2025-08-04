// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using global::Tests.Common.Model.Pidl;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class BillingGroupDescriptionTests : TestBase
    {
        /// <summary>
        /// Test to verify that the billing group description is returned for the given operation, type and scenario
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="type"></param>
        /// <param name="scenario"></param>
        /// <returns></returns>
        [DataRow("selectinstance", null, null)]
        [DataRow("selectinstance", "list", null)]
        [DataRow("selectinstance", "lightweightv7", null)]
        [DataRow("add", "lightweight", null)]
        [DataRow("add", "lightweightv7", null)]
        [DataRow("update", "lightweight", "billingGroupPONumber")]
        [DataRow("update", "lightweightv7", "billingGroupPONumber")]
        [TestMethod]
        public async Task GetBillingGroupDescription_NoLangPassed_DefaultEnUsLangUsed(string operation, string type, string scenario)
        {
            // Arrange
            string[] partners = new string[] { "commercialstores", "officesmb" };

            foreach (string partner in partners)
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();

                if (string.Equals(partner, Constants.VirtualPartnerNames.OfficeSmb, StringComparison.OrdinalIgnoreCase))
                {
                    string partnerSettingResponse = "{\"add\":{\"template\":\"defaulttemplate\"},\"update\":{\"template\":\"defaulttemplate\"},\"selectinstance\":{\"template\":\"defaulttemplate\"}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                    headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
                }

                string url = $"/v7.0/Account001/billingGroupDescriptions?country=us&partner={partner}&operation={operation}&type={type}&scenario={scenario}";
                string urlEng = url + "&language=en-us";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);
                List<PIDLResource> pidlsEng = await GetPidlFromPXService(urlEng, additionaHeaders: headers);

                // Assert
                // Based on an assumpsion that pidl.data_description and pidl.strings contain strings that have to be localize
                // and the content format stay the same regarless of whether language is passed or not,
                // where pidl.display_description may contain buttons with pidlAction that will have language parameter
                // and can not be used for comparison
                Assert.IsNotNull(pidls);
                for (int i = 0; i < pidls.Count; i++)
                {
                    string dataDescription = JsonConvert.SerializeObject(pidls[i].DataDescription);
                    string dataDescriptionEng = JsonConvert.SerializeObject(pidlsEng[i].DataDescription);
                    Assert.AreEqual(dataDescription, dataDescriptionEng, ignoreCase: true, message: "DataDescriptions don't match");

                    string pidlStrings = JsonConvert.SerializeObject(pidls[i].PidlResourceStrings);
                    string pidlStringsEng = JsonConvert.SerializeObject(pidlsEng[i].PidlResourceStrings);
                    Assert.AreEqual(pidlStrings, pidlStringsEng, ignoreCase: true, message: "Pidl strings don't match");
                }

                PXSettings.PartnerSettingsService.ResetToDefaults();
            }
        }

        [DataRow("commercialstores", "defaulttemplate", true, true, true)]
        [DataRow("commercialstores", "defaulttemplate", true, false, true)]
        [DataRow("commercialstores", "commercialstores", false, false, true)]
        [DataRow("officesmb", "defaulttemplate", true, true, true)]
        [DataRow("officesmb", "defaulttemplate", true, false, false)]
        [TestMethod]
        public async Task GetBillingGroupDescription_AddPartnerActionToBillingGroupAddAndUpdate(string partner, string template, bool usePSS, bool useFeature, bool shouldSeePartnerActionContext)
        {
            // Arrange
            string url = $"/v7.0/Account001/billingGroupDescriptions?country=us&operation=selectInstance&partner={partner}";

            var headers = new Dictionary<string, string>();

            if (usePSS)
            {
                string defaultPSSResponse = "{\"default\":{\"template\":\"" + template + "\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
                string featureResponse = "{\"selectInstance\":{\"template\":\"" + template + "\",\"redirectionPattern\":\"inline\",\"resources\":{\"billingGroup\":{\"lightweight\":{\"template\":\"" + template + "\",\"redirectionPattern\":\"inline\"}}},\"features\":{\"addPartnerActionToBillingGroupAddAndUpdate\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}}";

                PXSettings.PartnerSettingsService.ArrangeResponse(useFeature ? featureResponse : defaultPSSResponse);

                headers.Add("x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache");
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "List of PIDLResources should NOT be null");

            var pidl = pidls.First();
            Assert.IsNotNull(pidl, "First pidl should not be null");

            var displayHint = pidl.GetDisplayHintById("addNewBG");
            Assert.IsNotNull(displayHint, "displayHint var should NOT be null");

            var displayHintAction = displayHint.Action;
            Assert.IsNotNull(displayHintAction, "displayHintAction var should NOT be null");
            Assert.IsTrue(string.Equals(displayHintAction.ActionType, "partnerAction", StringComparison.OrdinalIgnoreCase));

            var actionContext = JsonConvert.DeserializeObject<ActionContext>(JsonConvert.SerializeObject(displayHintAction.Context));

            if (shouldSeePartnerActionContext)
            {
                Assert.IsNotNull(actionContext, "actionContext var should NOT be null");
                Assert.IsTrue(string.Equals(actionContext.Action, PIActionType.AddResource.ToString(), StringComparison.OrdinalIgnoreCase), $"expected actionContext.Action to be {PIActionType.AddResource.ToString()} but saw {actionContext.Action}");
            }
            else
            {
                Assert.IsNull(actionContext, "actionContext var SHOULD be null");
            }
        }

        [DataRow("commercialstores", "defaulttemplate", true, true, true)]
        [DataRow("commercialstores", "defaulttemplate", true, false, true)]
        [DataRow("commercialstores", "commercialstores", false, false, true)]
        [DataRow("officesmb", "defaulttemplate", true, true, true)]
        [DataRow("officesmb", "defaulttemplate", true, false, false)]
        [TestMethod]
        public async Task AddBillingGroupDescription_AddSelectResourcePartnerActionToBillingGroupAddPi(string partner, string template, bool usePSS, bool useFeature, bool shouldSeePartnerActionContext)
        {
            // Arrange
            string url = $"/v7.0/Account001/billingGroupDescriptions?country=us&operation=add&type=lightweight&partner={partner}";

            var headers = new Dictionary<string, string>();

            if (usePSS)
            {
                string defaultPSSResponse = "{\"default\":{\"template\":\"" + template + "\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
                string featureResponse = "{\"add\":{\"template\":\"" + template + "\",\"redirectionPattern\":\"inline\",\"resources\":{\"billingGroup\":{\"lightweight\":{\"template\":\"" + template + "\",\"redirectionPattern\":\"inline\"}}},\"features\":{\"addSelectResourcePartnerActionToBillingGroupAddPi\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}}";

                PXSettings.PartnerSettingsService.ArrangeResponse(useFeature ? featureResponse : defaultPSSResponse);

                headers.Add("x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache");
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "The List of pidls should not be null.");

            var pidl = pidls.First();
            Assert.IsNotNull(pidl, "The first pidl in the list should not be null.");

            var displayHint = pidl.GetDisplayHintById("billingGroupLightWeightAddNewPaymentInstrument");
            Assert.IsNotNull(displayHint, "displayHint var should not be null");

            var displayHintAction = displayHint.Action;
            Assert.IsNotNull(displayHintAction, "displayHintAction var should not be null");
            Assert.IsTrue(string.Equals(displayHintAction.ActionType, "partnerAction", StringComparison.OrdinalIgnoreCase), $"Expected {displayHintAction.ActionType} to == partnerAction");

            var actionContext = JsonConvert.DeserializeObject<ActionContext>(JsonConvert.SerializeObject(displayHintAction.Context));
            if (shouldSeePartnerActionContext)
            {
                Assert.IsNotNull(actionContext, "actionContext should NOT be null");
                Assert.IsTrue(string.Equals(actionContext.Action, PIActionType.SelectResourceType.ToString(), StringComparison.OrdinalIgnoreCase), $"Expected {actionContext.Action} to == {PIActionType.SelectResourceType.ToString()}");
            }
            else
            {
                Assert.IsNull(actionContext, "actionContext SHOULD be null");
            }
        }
    }
}
