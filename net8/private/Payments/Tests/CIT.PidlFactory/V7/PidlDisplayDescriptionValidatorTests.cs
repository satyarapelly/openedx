// <copyright company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlFactory;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Test.Common;

    [TestClass]
    public class PidlDisplayDescriptionValidatorTests : UnitTestBase
    {
        readonly string basePath = AppDomain.CurrentDomain.BaseDirectory + @"\V7\Config\DisplayDescriptions\";

        readonly Dictionary<string, string[]> namedPartnerLists = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "Partners-Validated",
                new string[] { "consoletemplate", "twopage", "appsource", "commercialstores", "commercialsupport", "selectpmdropdown", "ggpdeds", "msteams", "northstarweb", "officeoobe", "onedrive", "oxodime", "oxooobe", "oxowebdirect", "payin", "setupoffice", "setupofficesdx", "storeoffice", "webblends", "windowsnative", "xbox", "onepage", "selectpmbuttonlist", "selectpmradiobuttonlist", "defaulttemplate", "listpiradiobutton", "listpibuttonlist", "secondscreentemplate" }
            },
            {
                "Partners-Skip",
                new string[] { "amc", "amcweb", "amcxbox", "azure", "azuresignup", "azureibiza", "bingtravel", "cart", "consumersupport", "default", "officeoobeinapp", "smboobe", "test", "webblends_inline", "webpay", "wallet", "bing", "commercialwebblends", "marketplace", "mseg", "office", "windowssettings", "windowssubs", "windowsstore", "storify", "xboxsubs", "xboxsettings", "saturn", "xboxweb", "xboxnative", "listpidropdown" }
            },
            {
                "Partners-CITValidated",
                new string[] { "onepage", "twopage", "selectpmbuttonlist", "selectpmradiobuttonlist", "selectpmdropdown", "listpidropdown", "listpiradiobutton", "listpibuttonlist", "defaulttemplate", "consoletemplate", "secondscreentemplate" }
            }
        };

        private List<string> skipDisplaySequencesId = new List<string> { "paymentOptionDisplayGroupSequence", "paymentOptionTextGroupSequence", "paymentMethodSelectPageSequence", "cancelSelectPMGroupSequence", "cancelGroupSequence", "billingAddressShowAddForConditionalFieldsGroupSequence" }; // DisplaySequenceId which is not used in csv file but unsed by code.
        private List<string> skipContainerToSequenceMapping = new List<string> { "paymentOptionDisplayGroup", "paymentOptionTextGroup", "paymentMethodSelectPage", "cancelSelectPMGroup", "paymentSelectionGroup", "paymentSelectionChangeGroup", "cancelGroup", "addressGroup", "billingAddressShowAddForConditionalFieldsGroup" }; // ContainerHintId which is skipped in DisplaySequenceId mapping
        private List<string> skipPropertyDisplayHintsId = new List<string> { "secureCardNumber", "secureCardNumberAmex", "secureCvv",  "secureCvv3", "secureCvv4" }; // PropertyDisplayHintsId which is not used in csv file but unsed by code.
        private List<string> inexistentDisplayHintIdInDisplaySequence = new List<string>(); // List for unique HintId used by DisplaySequences.csv
        private List<string> inexistentDisplaySequenceIdInPIDLResourceDisplaySequence = new List<string>(); // List for unique SequenceId used by PIDLResorucedisplaySequence.csv
        private List<string> inexistentDisplaySequenceIdInContainerDisplayHints = new List<string>(); // List for unique SeqeunceId used by ContainerDisplayHints.csv

        private HashSet<string> pidlResourcesDisplaySequencesSet; // Hashset for unique DisplaySequenceId in PIDLResourcesDisplaySequences.csv
        private HashSet<string> pidlResourcesDisplayStringSequencesSet; // Hashset for unique DisplayStringSequenceId in PIDLResourcesDisplaySequences.csv
        private Dictionary<string, KeyValuePair<string, long>> containerToSequenceMapping; // Dictionary for ContainerHintId to DisplaySequenceId mapping
        private HashSet<string> allUsedDisplayHintIdInDisplaySequence; // Hashset for unique HintId used by DisplaySequences.csv
        private Dictionary<string, List<KeyValuePair<string, long>>> sequenceToContainerMapping; // Dictionary for DisplaySequenceId to ContainerHintId mapping, with line number in DispalySequence.csv
        private HashSet<string> displayDictionaryDisplayHintIdSet; // Hashset for unique DisplayHintId in DisplayDictionaries.csv
        private Dictionary<string, Tuple<bool, long>> propertyDisplayHintsDictionary; // Dictionary for <DisplayHintId, <HasAccessiblityName, LineNumber>> in PropertyDisplayHint.csv
        private Dictionary<string, long> propertyErrorMessagesHintDictionary; // Dictionary for all PropertyHintId in PropertyErrorMessages.csv, with line number
        private Dictionary<string, long> displayDescriptionTagsHintDictionary; // Dictionary for all PropertyHintId in DisplayDescriptionTags.csv, with line number
        private Dictionary<string, long> displayDescriptionConditionalFieldsHintDictionary; // Dictionary for all PropertyHintId in DisplayDescriptionConditionalFields.csv, with line number

        private Dictionary<string, long> allDisplaySequencesId; // All DisplaySequenceId in DisplaySequences.csv, with line number
        private Dictionary<string, long> allDisplayStringSequencesId; // All DisplayStringSequenceId in DisplayStringSequences.csv, with line number
        private Dictionary<string, long> allDisplayStringId; // All DisplayStringId in DisplayStrings.csv, with line number
        private HashSet<string> allUsedDisplayStringId; // All DisplayStringId in DisplayStringSequences.csv
        private HashSet<string> allHelpSequenceId; // All HelpSequenceId in PropertyDisplayHint.csv

        [TestMethod]
        [TestCategory(TestCategory.TestCoverage)]
        public void PidlDisplayDescriptionValidator_EnsureCITsCoverAllPartners()
        {
            // Arrange
            var allProfilePartners = namedPartnerLists["Partners-Validated"]
                .Concat(namedPartnerLists["Partners-Skip"]);

            // Assert
            CollectionAssert.AreEquivalent(TestConstants.AllPartners.ToList(), allProfilePartners.Distinct().ToList(), "CIT to verify PidlDisplayDescriptionValidatorTests cover all partners");
        }

        [TestMethod]
        public void PidlDisplayDescriptionValidator_ValidateDisplaySequenceAndContainerDisplayHint()
        {
            foreach (string partnerName in this.namedPartnerLists["Partners-Validated"])
            {
                this.InitializeTestData();
                this.ReadPidlResourcesDisplaySequences(partnerName);
                this.ReadDisplaySequences(partnerName);
                this.ReadPropertyDisplayHints(partnerName);
                this.ReadDisplayStringSequences(partnerName);

                // Remove display sequence Id's which is used in feature post process.
                if (partnerName.Equals(GlobalConstants.PartnerNames.Webblends, StringComparison.OrdinalIgnoreCase)
                    || partnerName.Equals(TestConstants.TemplateNames.Selectpmbuttonlist)
                    || partnerName.Equals(TestConstants.TemplateNames.Selectpmradiobuttonlist)
                    || partnerName.Equals(TestConstants.TemplateNames.Selectpmdropdown))
                {
                    // Remove the sequenceid which is used in code not in csv file
                    foreach (string displaySequenceId in this.skipDisplaySequencesId)
                    {
                        if (this.allDisplaySequencesId.ContainsKey(displaySequenceId))
                        {
                            this.allDisplaySequencesId.Remove(displaySequenceId);
                        }
                    }

                    // Remove the sequenceid which is used in code not in csv file
                    foreach (string displaySequenceId in this.skipContainerToSequenceMapping)
                    {
                        if (this.containerToSequenceMapping.ContainsKey(displaySequenceId))
                        {
                            this.containerToSequenceMapping.Remove(displaySequenceId);
                        }
                    }
                }

                // Remove used display sequence id
                foreach (string displaySequenceId in this.pidlResourcesDisplaySequencesSet)
                {
                    if (this.allDisplaySequencesId.ContainsKey(displaySequenceId))
                    {
                        allDisplaySequencesId.Remove(displaySequenceId);

                        // If there is a related container hint id, remove it as well
                        if (this.sequenceToContainerMapping.ContainsKey(displaySequenceId))
                        {
                            List<string> cur = new List<string>();
                            cur.Add(displaySequenceId);

                            while (cur.Count != 0)
                            {
                                List<string> next = new List<string>();
                                foreach (string currentDisplaySequenceId in cur)
                                {
                                    if (this.sequenceToContainerMapping.ContainsKey(currentDisplaySequenceId))
                                    {
                                        foreach (KeyValuePair<string, long> sequenceToContainer in this.sequenceToContainerMapping[currentDisplaySequenceId])
                                        {
                                            if (this.containerToSequenceMapping.ContainsKey(sequenceToContainer.Key))
                                            {
                                                this.allDisplaySequencesId.Remove(this.containerToSequenceMapping[sequenceToContainer.Key].Key);
                                                next.Add(this.containerToSequenceMapping[sequenceToContainer.Key].Key);
                                                this.containerToSequenceMapping.Remove(sequenceToContainer.Key);
                                            }
                                        }

                                        this.sequenceToContainerMapping.Remove(currentDisplaySequenceId);
                                    }
                                }

                                cur = next;
                            }
                        }
                    }
                }

                // Remove DisplaySequenceId from list if it is a valid HelpSequenceId
                foreach (string displaySequenceId in this.allHelpSequenceId)
                {
                    if (this.allDisplaySequencesId.ContainsKey(displaySequenceId))
                    {
                        this.allDisplaySequencesId.Remove(displaySequenceId);
                    }
                }

                bool noUnusedDisplaySequenceEntry = true;

                if (this.allDisplaySequencesId.Count > 0)
                {
                    TestContext.WriteLine("found unused entry in DisplaySequences.csv");

                    foreach (KeyValuePair<string, long> pair in this.allDisplaySequencesId)
                    {
                        TestContext.WriteLine(pair.Key + " in line " + pair.Value + ".");
                    }

                    noUnusedDisplaySequenceEntry = false;
                }

                if (this.containerToSequenceMapping.Count > 0)
                {
                    TestContext.WriteLine("found unused entry in ContainerDisplayHints.csv");

                    foreach (KeyValuePair<string, KeyValuePair<string, long>> pair in this.containerToSequenceMapping)
                    {
                        TestContext.WriteLine(pair.Key + " in line " + pair.Value.Value + ".");
                    }

                    noUnusedDisplaySequenceEntry = false;
                }

                Assert.IsTrue(noUnusedDisplaySequenceEntry, "Unused entry found in " + partnerName + "'s ContainerDisplayHints.csv and DisplaySequences.csv");

                foreach (string displayStringSequenceId in this.pidlResourcesDisplayStringSequencesSet)
                {
                    if (this.allDisplayStringSequencesId.ContainsKey(displayStringSequenceId))
                    {
                        this.allDisplayStringSequencesId.Remove(displayStringSequenceId);
                    }
                }

                bool noUnusedDisplayStringSequenceEntry = true;

                if (this.allDisplayStringSequencesId.Count > 0)
                {
                    TestContext.WriteLine("found unused entry in DisplayStringSequences.csv");

                    foreach (KeyValuePair<string, long> pair in this.allDisplayStringSequencesId)
                    {
                        TestContext.WriteLine(pair.Key + " in line " + pair.Value + ".");
                    }

                    noUnusedDisplayStringSequenceEntry = false;
                }

                Assert.IsTrue(noUnusedDisplayStringSequenceEntry, "Unused entry found in " + partnerName + "'s DisplayStringSequences.csv");
            }
        }

        /// <summary>
        /// Validates the DisplaySequenceId from PIDLResourceDisplaySequence.csv and DisplaySequence.csv for partners.
        /// </summary>
        [TestMethod]
        public void PidlDisplayDescriptionValidator_ValidatePIDLDisplaySequenceAndDisplaySequence()
        {
            // Arrange
            foreach (string partnerName in this.namedPartnerLists["Partners-CITValidated"])
            {
                // Initialize test data
                this.InitializeTestData();

                // Act
                // Read necessary data for validation
                this.ReadPidlResourcesDisplaySequences(partnerName);
                this.ReadDisplaySequences(partnerName);

                // Remove display sequence Id's which are used in feature post-process.
                if (partnerName.Equals(TestConstants.TemplateNames.Selectpmbuttonlist, StringComparison.OrdinalIgnoreCase)
                    || partnerName.Equals(TestConstants.TemplateNames.Selectpmradiobuttonlist, StringComparison.OrdinalIgnoreCase)
                    || partnerName.Equals(TestConstants.TemplateNames.Selectpmdropdown, StringComparison.OrdinalIgnoreCase))
                {
                    // Remove the SequenceId which is used in code, not in the CSV file
                    foreach (string displaySequenceId in this.skipDisplaySequencesId.Where(this.allDisplaySequencesId.ContainsKey).ToList())
                    {
                        this.allDisplaySequencesId.Remove(displaySequenceId);
                    }
                }

                // Remove used DisplaySequenceId
                foreach (string displaySequenceId in this.pidlResourcesDisplaySequencesSet.ToList())
                {
                    if (this.allDisplaySequencesId.ContainsKey(displaySequenceId))
                    {
                        this.allDisplaySequencesId.Remove(displaySequenceId);
                    }
                    else
                    {
                        this.inexistentDisplaySequenceIdInPIDLResourceDisplaySequence.Add(displaySequenceId);
                    }
                }

                // Check for Inexistent DisplaySequenceId entries in PIDLResourceDisplaySequence.csv
                bool inexistentDisplaySequenceEntry = true;

                if (this.inexistentDisplaySequenceIdInPIDLResourceDisplaySequence.Count > 0)
                {
                    TestContext.WriteLine("Inexistent DisplaySequenceId entries found in PIDLResourceDisplaySequence.csv for partner: " + partnerName);
                    foreach (string inexistentDisplayHintId in this.inexistentDisplaySequenceIdInPIDLResourceDisplaySequence)
                    {
                        TestContext.WriteLine(inexistentDisplayHintId);
                    }

                    inexistentDisplaySequenceEntry = false;
                }

                // Assert
                // Validate the test results
                Assert.IsTrue(inexistentDisplaySequenceEntry, $"Inexistent DisplaySequenceId found in {partnerName}'s PIDLResourceDisplaySequence.csv");
            }
        }

        /// <summary>
        /// Validates the unused HintId from DisplaySequence.csv.
        /// From the ReadDisplaySequences method, we retrieve allUsedDisplayHintIdInDisplaySequence,
        /// which contains HintIds that are not available in ContainerDisplayHint.csv.
        /// </summary>
        [TestMethod]
        public void PidlHintIdValidator_ValidatePropertyDisplayHintOrContainerHintId()
        {
            // Arrange
            foreach (string partnerName in this.namedPartnerLists["Partners-CITValidated"])
            {
                // Initialize test data
                this.InitializeTestData();

                // Act
                // Read necessary data for validation
                this.ReadDisplaySequences(partnerName);
                this.ReadPropertyDisplayHints(partnerName);

                // Remove the DisplayHintId which is used in code not in csv file
                foreach (string skippedDisplayHintId in this.skipPropertyDisplayHintsId)
                {
                    if (this.propertyDisplayHintsDictionary.ContainsKey(skippedDisplayHintId))
                    {
                        this.propertyDisplayHintsDictionary.Remove(skippedDisplayHintId);
                    }
                }

                // Remove used DisplayHintIds from propertyDisplayHintsDictionary
                foreach (string usedDisplayHintId in this.allUsedDisplayHintIdInDisplaySequence)
                {
                    if (this.propertyDisplayHintsDictionary.ContainsKey(usedDisplayHintId))
                    {
                        this.propertyDisplayHintsDictionary.Remove(usedDisplayHintId);
                    }
                    else
                    {
                        inexistentDisplayHintIdInDisplaySequence.Add(usedDisplayHintId);
                    }
                }

                // Check for unused HintId entries in DisplaySequence.csv
                bool inexistentDisplaySequenceEntry = true;
                if (partnerName.Equals(TestConstants.TemplateNames.Selectpmbuttonlist)
                    || partnerName.Equals(TestConstants.TemplateNames.Selectpmradiobuttonlist)
                    || partnerName.Equals(TestConstants.TemplateNames.Selectpmdropdown))
                {
                    inexistentDisplayHintIdInDisplaySequence.RemoveAll(item => skipContainerToSequenceMapping.Any(skipItem => item.IndexOf(skipItem, StringComparison.OrdinalIgnoreCase) >= 0));
                }

                if (inexistentDisplayHintIdInDisplaySequence.Count > 0)
                {
                    TestContext.WriteLine("Inexistent HintId entries found in DisplaySequence.csv for partner: " + partnerName);
                    foreach (string usedDisplayHintId in inexistentDisplayHintIdInDisplaySequence)
                    {
                        TestContext.WriteLine(usedDisplayHintId);
                    }

                    inexistentDisplaySequenceEntry = false;
                }

                // Assert
                // Validate the test results
                Assert.IsTrue(inexistentDisplaySequenceEntry, "Inexistent HintId found in " + partnerName + "'s DisplaySequence.csv");
            }
        }

        /// <summary>
        /// Validates the DisplaySequenceId from CotainerDisplayHints.csv and DisplaySequence.csv for partners.
        /// The test checks for inexistent DisplaySequenceId from ContainerDisplayHints.csv after validating the DisplaySequenceId from DisplaySequence.csv
        /// </summary>
        [TestMethod]
        public void PidlDisplayDescriptionValidator_ValidateContainerSequenceAndDisplaySequence()
        {
            foreach (string partnerName in this.namedPartnerLists["Partners-CITValidated"])
            {
                // Arrange
                // Initialize test data
                this.InitializeTestData();

                // Read necessary data for validation
                this.ReadPidlResourcesDisplaySequences(partnerName);
                this.ReadDisplaySequences(partnerName);

                // Remove display sequence Id's which are used in feature post-process.
                if (partnerName.Equals(TestConstants.TemplateNames.Selectpmbuttonlist, StringComparison.OrdinalIgnoreCase)
                    || partnerName.Equals(TestConstants.TemplateNames.Selectpmradiobuttonlist, StringComparison.OrdinalIgnoreCase)
                    || partnerName.Equals(TestConstants.TemplateNames.Selectpmdropdown, StringComparison.OrdinalIgnoreCase))
                {
                    // Remove the SequenceId which is used in code, not in the CSV file
                    foreach (string displaySequenceId in this.skipDisplaySequencesId.Where(this.allDisplaySequencesId.ContainsKey).ToList())
                    {
                        this.allDisplaySequencesId.Remove(displaySequenceId);
                    }

                    // Remove the sequenceid which is used in code, not in the CSV file
                    foreach (string displaySequenceId in this.skipContainerToSequenceMapping)
                    {
                        if (this.containerToSequenceMapping.ContainsKey(displaySequenceId))
                        {
                            this.containerToSequenceMapping.Remove(displaySequenceId);
                        }
                    }
                }

                // Act
                // Remove used DisplaySequenceId
                List<string> keysToRemove = new List<string>();

                foreach (KeyValuePair<string, KeyValuePair<string, long>> pair in this.containerToSequenceMapping)
                {
                    string displaySequenceId = pair.Value.Key;

                    if (this.allDisplaySequencesId.ContainsKey(displaySequenceId))
                    {
                        keysToRemove.Add(pair.Key);
                    }
                    else
                    {
                        this.inexistentDisplaySequenceIdInContainerDisplayHints.Add(displaySequenceId);
                    }
                }

                keysToRemove.ForEach(key => this.containerToSequenceMapping.Remove(key));

                // Check for unused DisplaySequenceId entries in CotainerDisplayHints.csv
                bool inexistentDisplaySequenceEntry = true;

                if (this.inexistentDisplaySequenceIdInContainerDisplayHints.Count > 0)
                {
                    TestContext.WriteLine("Inexistent DisplaySequenceId entries found in CotainerDisplayHints.csv for partner: " + partnerName);
                    this.inexistentDisplaySequenceIdInContainerDisplayHints.ForEach(inexistentDisplaySequenceId => TestContext.WriteLine(inexistentDisplaySequenceId));
                    inexistentDisplaySequenceEntry = false;
                }

                // Assert
                // Validate the test results
                Assert.IsTrue(inexistentDisplaySequenceEntry, $"Inexistent DisplaySequenceId found in {partnerName}'s CotainerDisplayHints.csv");
            }
        }

        [TestMethod]
        public void PidlDisplayDescriptionValidator_ValidatePropertyDisplayHint()
        {
            foreach (string partnerName in this.namedPartnerLists["Partners-Validated"])
            {
                this.InitializeTestData();
                this.ReadDisplaySequences(partnerName);
                this.ReadPropertyDisplayHints(partnerName);

                foreach (string usedDisplayHintId in this.allUsedDisplayHintIdInDisplaySequence)
                {
                    if (this.propertyDisplayHintsDictionary.ContainsKey(usedDisplayHintId))
                    {
                        this.propertyDisplayHintsDictionary.Remove(usedDisplayHintId);
                    }
                }

                // Remove the DisplayHintId which is used in code not in csv file
                foreach (string skippedDisplayHintId in this.skipPropertyDisplayHintsId)
                {
                    if (this.propertyDisplayHintsDictionary.ContainsKey(skippedDisplayHintId))
                    {
                        this.propertyDisplayHintsDictionary.Remove(skippedDisplayHintId);
                    }
                }

                bool noUnusedEntry = true;

                if (this.propertyDisplayHintsDictionary.Count > 0)
                {
                    TestContext.WriteLine("found unused entry in PropertyDisplayHints.csv");
                    foreach (KeyValuePair<string, Tuple<bool, long>> pair in this.propertyDisplayHintsDictionary)
                    {
                        TestContext.WriteLine(pair.Key + " in line " + pair.Value.Item2 + ".");
                    }

                    noUnusedEntry = false;
                }

                Assert.IsTrue(noUnusedEntry, "Unused entry found in " + partnerName + "'s PropertyDisplayHints.csv");
            }
        }

        [TestMethod]
        public void PidlDisplayDescriptionValidator_ValidatePropertyErrorMessages()
        {
            foreach (string partnerName in this.namedPartnerLists["Partners-Validated"])
            {
                this.InitializeTestData();
                this.ReadDisplaySequences(partnerName);
                this.ReadPropertyDisplayHints(partnerName);
                this.ReadPropertyErrorMessages(partnerName);

                foreach (string usedDisplayHintId in this.allUsedDisplayHintIdInDisplaySequence)
                {
                    if (this.propertyDisplayHintsDictionary.ContainsKey(usedDisplayHintId))
                    {
                        this.propertyErrorMessagesHintDictionary.Remove(usedDisplayHintId);
                    }
                }

                // Remove the DisplayHintId which is used in code not in csv file
                foreach (string skippedDisplayHintId in this.skipPropertyDisplayHintsId)
                {
                    if (this.propertyErrorMessagesHintDictionary.ContainsKey(skippedDisplayHintId))
                    {
                        this.propertyErrorMessagesHintDictionary.Remove(skippedDisplayHintId);
                    }
                }

                bool noUnusedEntry = true;

                if (this.propertyErrorMessagesHintDictionary.Count > 0)
                {
                    TestContext.WriteLine("found unused entry in PropertyErrorMessages.csv");
                    foreach (KeyValuePair<string, long> pair in this.propertyErrorMessagesHintDictionary)
                    {
                        TestContext.WriteLine(pair.Key + " in line " + pair.Value + ".");
                    }

                    noUnusedEntry = false;
                }

                Assert.IsTrue(noUnusedEntry, "Unused entry found in " + partnerName + "'s PropertyErrorMessages.csv");
            }
        }

        [TestMethod]
        public void PidlDisplayDescriptionValidator_ValidateDisplayDescriptionTags()
        {
            foreach (string partnerName in this.namedPartnerLists["Partners-Validated"])
            {
                this.InitializeTestData();
                this.ReadDisplaySequences(partnerName);
                this.ReadPropertyDisplayHints(partnerName);
                this.ReadDisplayDescriptionTags(partnerName);

                List<string> missingDescriptionTags = new List<string>();

                foreach (string usedDisplayHintId in this.allUsedDisplayHintIdInDisplaySequence)
                {
                    if (this.propertyDisplayHintsDictionary.ContainsKey(usedDisplayHintId))
                    {
                        if (this.displayDescriptionTagsHintDictionary.ContainsKey(usedDisplayHintId))
                        {
                            this.displayDescriptionTagsHintDictionary.Remove(usedDisplayHintId);
                        }
                        else if (this.propertyDisplayHintsDictionary[usedDisplayHintId].Item1)
                        {
                            missingDescriptionTags.Add(usedDisplayHintId);
                        }
                    }
                }

                // Remove the DisplayHintId which is used in code not in csv file
                foreach (string skippedDisplayHintId in this.skipPropertyDisplayHintsId)
                {
                    if (this.displayDescriptionTagsHintDictionary.ContainsKey(skippedDisplayHintId))
                    {
                        this.displayDescriptionTagsHintDictionary.Remove(skippedDisplayHintId);
                    }
                }

                bool noUnusedEntry = true;

                if (this.displayDescriptionTagsHintDictionary.Count > 0)
                {
                    TestContext.WriteLine("found unused entry in DisplayDescriptionTags.csv");
                    foreach (KeyValuePair<string, long> pair in this.displayDescriptionTagsHintDictionary)
                    {
                        TestContext.WriteLine(pair.Key + " in line " + pair.Value + ".");
                    }

                    noUnusedEntry = false;
                }

                if (missingDescriptionTags.Count > 0)
                {
                    TestContext.WriteLine("found property hint that does not have accessibility name");
                    foreach (string propertyHint in missingDescriptionTags)
                    {
                        TestContext.WriteLine(propertyHint);
                    }

                    noUnusedEntry = false;
                }

                Assert.IsTrue(noUnusedEntry, "Unused entry found in " + partnerName + "'s DisplayDescriptionTags.csv");
            }
        }

        [TestMethod]
        public void PidlDisplayDescriptionValidator_ValidateDisplayDescriptionConditionalFields()
        {
            foreach (string partnerName in this.namedPartnerLists["Partners-Validated"])
            {
                this.InitializeTestData();
                this.ReadDisplaySequences(partnerName);
                this.ReadPropertyDisplayHints(partnerName);
                this.ReadDisplayDescriptionConditionalFields(partnerName);

                foreach (string usedDisplayHintId in this.allUsedDisplayHintIdInDisplaySequence)
                {
                    if (this.propertyDisplayHintsDictionary.ContainsKey(usedDisplayHintId))
                    {
                        if (this.displayDescriptionConditionalFieldsHintDictionary.ContainsKey(usedDisplayHintId))
                        {
                            this.displayDescriptionConditionalFieldsHintDictionary.Remove(usedDisplayHintId);
                        }
                    }
                }

                foreach (string displaySequenceId in this.skipContainerToSequenceMapping)
                {
                    if (this.displayDescriptionConditionalFieldsHintDictionary.ContainsKey(displaySequenceId))
                    {
                        this.displayDescriptionConditionalFieldsHintDictionary.Remove(displaySequenceId);
                    }
                }

                bool noUnusedEntry = true;

                if (this.displayDescriptionConditionalFieldsHintDictionary.Count > 0)
                {
                    TestContext.WriteLine("found unused entry in DisplayDescriptionConditionalFields.csv");
                    foreach (KeyValuePair<string, long> pair in this.displayDescriptionConditionalFieldsHintDictionary)
                    {
                        TestContext.WriteLine(pair.Key + " in line " + pair.Value + ".");
                    }

                    noUnusedEntry = false;
                }

                Assert.IsTrue(noUnusedEntry, "Unused entry found in " + partnerName + "'s DisplayDescriptionConditionalFields.csv");
            }
        }

        [TestMethod]
        public void PidlDisplayDescriptionValidator_ValidateDisplayStrings()
        {
            foreach (string partnerName in this.namedPartnerLists["Partners-Validated"])
            {
                this.InitializeTestData();
                this.ReadDisplayStringSequences(partnerName);
                this.ReadDisplayStrings(partnerName);

                foreach (string usedDisplayStringId in this.allUsedDisplayStringId)
                {
                    if (this.allDisplayStringId.ContainsKey(usedDisplayStringId))
                    {
                        this.allDisplayStringId.Remove(usedDisplayStringId);
                    }
                }

                bool noUnusedEntry = true;

                if (this.allDisplayStringId.Count > 0)
                {
                    TestContext.WriteLine("found unused entry in DisplayStrings.csv");
                    foreach (KeyValuePair<string, long> pair in this.allDisplayStringId)
                    {
                        TestContext.WriteLine(pair.Key + " in line " + pair.Value + ".");
                    }

                    noUnusedEntry = false;
                }

                Assert.IsTrue(noUnusedEntry, "Unused entry found in " + partnerName + "'s DisplayStrings.csv");
            }
        }

        private void InitializeTestData()
        {
            this.pidlResourcesDisplaySequencesSet = new HashSet<string>();
            this.pidlResourcesDisplayStringSequencesSet = new HashSet<string>();
            this.containerToSequenceMapping = new Dictionary<string, KeyValuePair<string, long>>();
            this.allUsedDisplayHintIdInDisplaySequence = new HashSet<string>();
            this.sequenceToContainerMapping = new Dictionary<string, List<KeyValuePair<string, long>>>();
            this.displayDictionaryDisplayHintIdSet = new HashSet<string>();
            this.propertyDisplayHintsDictionary = new Dictionary<string, Tuple<bool, long>>();
            this.propertyErrorMessagesHintDictionary = new Dictionary<string, long>();
            this.displayDescriptionTagsHintDictionary = new Dictionary<string, long>();
            this.displayDescriptionConditionalFieldsHintDictionary = new Dictionary<string, long>();

            this.allDisplaySequencesId = new Dictionary<string, long>();
            this.allDisplayStringSequencesId = new Dictionary<string, long>();
            this.allUsedDisplayStringId = new HashSet<string>();
            this.allDisplayStringId = new Dictionary<string, long>();
            this.allHelpSequenceId = new HashSet<string>();
        }

        private void ReadPidlResourcesDisplaySequences(string partnerName)
        {
            // Collect all unique DisplaySequenceId from PIDLResourcesDisplaySequences.csv
            // PIDLResourcesDisplaySequences.csv is the entrance point for DisplayDescription
            using (PIDLConfigParser parser = new PIDLConfigParser(
                this.basePath + partnerName + @"\PIDLResourcesDisplaySequences.csv",
                new[]
                {
                    new ColumnDefinition("PIDLResourceType",           ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("PIDLResourceIdentity",       ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Operation",                  ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("CountryIds",                 ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Scenario",                   ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DisplaySequenceId",          ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DisplayStringsSequenceId",   ColumnConstraint.Optional, ColumnFormat.AlphaNumeric)
                },
                true))
            {
                while (!parser.EndOfData)
                {
                    // DisplaySequenceId is the 6th column in PIDLResourcesDisplaySequences.csv
                    string[] cells = parser.ReadValidatedFields();
                    this.pidlResourcesDisplaySequencesSet.Add(cells[5]);
                    this.pidlResourcesDisplayStringSequencesSet.Add(cells[6]);
                }
            }
        }

        private void ReadPropertyErrorMessages(string partnerName)
        {
            // Get PropertyHintId from PropertyErrorMessages.csv
            using (PIDLConfigParser parser = new PIDLConfigParser(
                this.basePath + partnerName + @"\PropertyErrorMessages.csv",
                new[]
                {
                    new ColumnDefinition("PropertyHintId",             ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds",                 ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("MessageSource",              ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DefaultErrorMessage",        ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ErrorRegEx",                 ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ErrorCode",                  ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ErrorMessage",               ColumnConstraint.Required, ColumnFormat.AlphaNumeric)
                },
                true))
            {
                string propertyHintId = null;
                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    // PropertyHintId is the 1st column in PropertyErrorMessages.csv
                    if (!string.IsNullOrEmpty(cells[0]))
                    {
                        propertyHintId = cells[0];

                        if (!this.propertyErrorMessagesHintDictionary.ContainsKey(propertyHintId))
                        {
                            this.propertyErrorMessagesHintDictionary.Add(propertyHintId, parser.LineNumber);
                        }
                    }
                }
            }
        }

        private void ReadDisplayDescriptionTags(string partnerName)
        {
            // Get DisplayStringSequenceId from DisplayStringSequences.csv
            using (PIDLConfigParser parser = new PIDLConfigParser(
                this.basePath + partnerName + @"\DisplayDescriptionTags.csv",
                new ColumnDefinition[]
                {
                    new ColumnDefinition("PropertyHintId", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("TagKey", ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("TagValue", ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                },
                true))
            {
                string propertyHintId = null;
                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    // PropertyHintId is the 1st column in DisplayDescriptionTags.csv
                    if (!string.IsNullOrEmpty(cells[0]))
                    {
                        propertyHintId = cells[0];
                    }

                    if (!this.displayDescriptionTagsHintDictionary.ContainsKey(propertyHintId) && string.Equals(cells[2], "accessibilityName", StringComparison.OrdinalIgnoreCase))
                    {
                        this.displayDescriptionTagsHintDictionary.Add(propertyHintId, parser.LineNumber);
                    }
                }
            }
        }

        private void ReadDisplayDescriptionConditionalFields(string partnerName)
        {
            // Get DisplayStringSequenceId from DisplayStringSequences.csv
            using (PIDLConfigParser parser = new PIDLConfigParser(
                this.basePath + partnerName + @"\DisplayDescriptionConditionalFields.csv",
                new ColumnDefinition[]
                {
                    new ColumnDefinition("PropertyHintId", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ConditionalFieldKey", ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ConditionalFieldValue", ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                },
                true))
            {
                string propertyHintId = null;
                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    // PropertyHintId is the 1st column in DisplayDescriptionConditionalFields.csv
                    if (!string.IsNullOrEmpty(cells[0]))
                    {
                        propertyHintId = cells[0];
                    }

                    if (!this.displayDescriptionConditionalFieldsHintDictionary.ContainsKey(propertyHintId))
                    {
                        this.displayDescriptionConditionalFieldsHintDictionary.Add(propertyHintId, parser.LineNumber);
                    }
                }
            }
        }

        private void ReadContainerDisplayHints(string partnerName)
        {
            // Collect ContainerHintId and DisplaySequenceId pair from ContainerDisplayHints.csv
            using (PIDLConfigParser parser = new PIDLConfigParser(
                this.basePath + partnerName + @"\ContainerDisplayHints.csv",
                new[]
                {
                    new ColumnDefinition("ContainerHintId",                     ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds",                          ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DisplaySequenceId",                   ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DisplayType",                         ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DisplayName",                         ColumnConstraint.Optional | ColumnConstraint.DoNotTrimWhiteSpaces, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("LayoutOrientation",                   ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("LayoutAlignment",                     ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("ShowDisplayName",                     ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("IsSubmitGroup",                       ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("Extend",                              ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("FirstButtonGroup",                    ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("ExtendButtonGroup",                   ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("ContainerDescription",                ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("DisplayConditionFunctionName",        ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DataCollectionSource",                ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DataCollectionFilterFunctionName",    ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("StyleHints",                          ColumnConstraint.Optional, ColumnFormat.Text)
                },
                true))
            {
                while (!parser.EndOfData)
                {
                    // ContainerHintId is the 1st column in ContainerDisplayHints.csv
                    // DisplaySequenceId is the 3rd column in ContainerDisplayHints.csv
                    string[] cells = parser.ReadValidatedFields();

                    if (!string.IsNullOrEmpty(cells[0]) && !string.IsNullOrEmpty(cells[2]) && !this.containerToSequenceMapping.ContainsKey(cells[0]))
                    {
                        this.containerToSequenceMapping.Add(cells[0], new KeyValuePair<string, long>(cells[2], parser.LineNumber));
                    }
                }
            }
        }

        private void ReadDisplayStringSequences(string partnerName)
        {
            // Collect DisplayStringSequenceId from DisplayStringSequence.csv
            using (PIDLConfigParser parser = new PIDLConfigParser(
                this.basePath + partnerName + @"\DisplayStringSequences.csv",
                new[]
                {
                    new ColumnDefinition("DisplayStringSequenceId",    ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds",                 ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DisplayStringId",            ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                },
                true))
            {
                while (!parser.EndOfData)
                {
                    // DisplayStringSequenceId is the 1st column in DisplayStringSequence.csv
                    // DisplayStringId is the 3rd column in DisplayStringSequence.csv
                    string[] cells = parser.ReadValidatedFields();
                    if (!string.IsNullOrEmpty(cells[0]) && !this.allDisplayStringSequencesId.ContainsKey(cells[0]))
                    {
                        this.allDisplayStringSequencesId.Add(cells[0], parser.LineNumber);
                    }

                    if (!string.IsNullOrEmpty(cells[2]) && !this.allUsedDisplayStringId.Contains(cells[2]))
                    {
                        this.allUsedDisplayStringId.Add(cells[2]);
                    }
                }
            }
        }

        private void ReadDisplayStrings(string partnerName)
        {
            // Collect DisplayStringId from DisplayStrings.csv
            using (PIDLConfigParser parser = new PIDLConfigParser(
                this.basePath + partnerName + @"\DisplayStrings.csv",
                new[]
                {
                    new ColumnDefinition("DisplayStringId",    ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds",         ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Type",               ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Code",               ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Value",              ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Target",             ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                },
                true))
            {
                while (!parser.EndOfData)
                {
                    // DisplayStringId is the 1st column in DisplayStrings.csv
                    string[] cells = parser.ReadValidatedFields();
                    if (!string.IsNullOrEmpty(cells[0]) && !this.allDisplayStringId.ContainsKey(cells[0]))
                    {
                        this.allDisplayStringId.Add(cells[0], parser.LineNumber);
                    }
                }
            }
        }

        private void ReadDisplaySequences(string partnerName)
        {
            this.ReadContainerDisplayHints(partnerName);

            using (PIDLConfigParser parser = new PIDLConfigParser(
                this.basePath + partnerName + @"\DisplaySequences.csv",
                new[]
                {
                    new ColumnDefinition("DisplaySequenceId",     ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds",            ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("FlightName",            ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("HintId",                ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                },
                true))
            {
                string displaySequenceId = null;
                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    // DisplaySequenceId is the 1st column in DisplaySequences.csv
                    // HintId is the 4th column in DisplaySequences.csv
                    if (!string.IsNullOrEmpty(cells[0]))
                    {
                        displaySequenceId = cells[0];
                    }

                    string displayHintId = cells[3];

                    if (!this.allDisplaySequencesId.ContainsKey(displaySequenceId))
                    {
                        this.allDisplaySequencesId.Add(displaySequenceId, parser.LineNumber);
                    }

                    // Skip expressions
                    if (displayHintId.Contains("."))
                    {
                        continue;
                    }
                    else
                    {
                        // 1, collect a full list display sequence id
                        if (!this.allDisplaySequencesId.ContainsKey(displaySequenceId))
                        {
                            this.allDisplaySequencesId.Add(displaySequenceId, parser.LineNumber);
                        }

                        // 2, collect a full list of display hint id that is used in DisplaySequences.csv
                        // 3, collect mappings from display sequence id to container hint id
                        // ContainerDisplayHints.csv and DisplaySequences.csv refer to each other, need to collect mappings for both direction
                        if (this.containerToSequenceMapping.ContainsKey(displayHintId))
                        {
                            if (!this.sequenceToContainerMapping.ContainsKey(displaySequenceId))
                            {
                                this.sequenceToContainerMapping[displaySequenceId] = new List<KeyValuePair<string, long>>();
                            }

                            this.sequenceToContainerMapping[displaySequenceId].Add(new KeyValuePair<string, long>(displayHintId, parser.LineNumber));
                        }
                        else if (!this.allDisplaySequencesId.ContainsKey(displayHintId) && !this.allUsedDisplayHintIdInDisplaySequence.Contains(displayHintId))
                        {
                            this.allUsedDisplayHintIdInDisplaySequence.Add(displayHintId);
                        }
                    }
                }
            }

            // Collect DisplayHintId from DisplayDictionaries.csv
            using (PIDLConfigParser parser = new PIDLConfigParser(
                this.basePath + partnerName + @"\DisplayDictionaries.csv",
                new[]
                {
                    new ColumnDefinition("DictionaryName", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Key",            ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Name",           ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DisplayHintId",  ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                },
                true))
            {
                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    // DisplayHintId is the 4th column in DisplaySequences.csv
                    string displayHintId = cells[3];

                    if (!string.IsNullOrEmpty(displayHintId))
                    {
                        this.displayDictionaryDisplayHintIdSet.Add(displayHintId);
                    }
                }

                // DisplayHintId in DisplayDictionaries.csv is used as a foreign key in ContainerDisplayHints.csv
                // In this case, dummy item is added to displayDictionaryDisplayHintIdSet (for PIDLResourcesDisplaySequences.csv) and allDisplaySequencesId (for DisplaySequences.csv)
                if (this.displayDictionaryDisplayHintIdSet.Count > 0)
                {
                    foreach (string ids in this.displayDictionaryDisplayHintIdSet)
                    {
                        if (!this.pidlResourcesDisplaySequencesSet.Contains(ids))
                        {
                            this.pidlResourcesDisplaySequencesSet.Add(ids);
                        }

                        if (!this.allDisplaySequencesId.ContainsKey(ids))
                        {
                            this.allDisplaySequencesId.Add(ids, -1);
                        }

                        if (this.containerToSequenceMapping.ContainsKey(ids))
                        {
                            if (!this.sequenceToContainerMapping.ContainsKey(ids))
                            {
                                this.sequenceToContainerMapping[ids] = new List<KeyValuePair<string, long>>();
                            }

                            this.sequenceToContainerMapping[ids].Add(new KeyValuePair<string, long>(ids, -1));
                        }
                    }
                }
            }
        }

        private void ReadPropertyDisplayHints(string partnerName)
        {
            // Get DisplayHintId from PropertyDisplayHints.csv
            using (PIDLConfigParser parser = new PIDLConfigParser(
                this.basePath + partnerName + @"\PropertyDisplayHints.csv",
                new[]
                {
                    new ColumnDefinition("PropertyHintId",                        ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds",                            ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("FeatureNames",                          ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("DisplayType",                           ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("PropertyName",                          ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DependentPropertyName",                 ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DependentPropertyValueRegex",           ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DisplayName",                           ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("DisplayDescription",                    ColumnConstraint.Optional | ColumnConstraint.DoNotTrimWhiteSpaces, ColumnFormat.Text),
                    new ColumnDefinition("IsHidden",                              ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("IsDisabled",                            ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("IsHighlighted",                         ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("IsDefault",                             ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("IsBack",                                ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("InputScope",                            ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("MaskInput",                             ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("ShowDisplayName",                       ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("DisplayFormat",                         ColumnConstraint.Optional | ColumnConstraint.DoNotTrimWhiteSpaces, ColumnFormat.Text),
                    new ColumnDefinition("DisplayExample",                        ColumnConstraint.Optional | ColumnConstraint.DoNotTrimWhiteSpaces, ColumnFormat.Text),
                    new ColumnDefinition("MinLength",                             ColumnConstraint.Optional, ColumnFormat.Number),
                    new ColumnDefinition("MaxLength",                             ColumnConstraint.Optional, ColumnFormat.Number),
                    new ColumnDefinition("DisplayContent",                        ColumnConstraint.Optional | ColumnConstraint.DoNotTrimWhiteSpaces, ColumnFormat.Text),
                    new ColumnDefinition("DisplayContentDescription",             ColumnConstraint.Optional | ColumnConstraint.DoNotTrimWhiteSpaces, ColumnFormat.Text),
                    new ColumnDefinition("DisplayHelpSequenceId",                 ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("DisplayHelpSequenceText",               ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("ClientAction",                          ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("Context",                               ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DestinationId",                         ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("PossibleValues",                        ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DisplaySelectionText",                  ColumnConstraint.Optional | ColumnConstraint.DoNotTrimWhiteSpaces, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("SourceUrl",                             ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DisplayLogo",                           ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("SelectType",                            ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("MaskDisplay",                           ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("DisplayConditionFunctionName",          ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DataCollectionSource",                  ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DataCollectionFilterFunctionName",      ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("IsSelectFirstItem",                     ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("DisplayImage",                          ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DisplayHelpPosition",                   ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Styles",                                ColumnConstraint.Optional, ColumnFormat.Text),
                },
                true))
            {
                List<string> hintsType = new List<string>() { "button", "property" }; // Type of Hint which should have accessibility name
                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    if (!string.IsNullOrEmpty(cells[0]))
                    {
                        // DisplayId, PropertyName and lineNumber
                        if (!this.propertyDisplayHintsDictionary.ContainsKey(cells[0]))
                        {
                            // If PropertyDisplayHint's type is button, propery or logo, and IsHidden property (index 10) is not TRUE
                            // It should have accessibility name
                            bool hasAccessibilityTag = hintsType.Contains(cells[3]) && !string.Equals(cells[9], "TRUE", StringComparison.OrdinalIgnoreCase);
                            this.propertyDisplayHintsDictionary.Add(cells[0], new Tuple<bool, long>(hasAccessibilityTag, parser.LineNumber));
                        }

                        // Get a collection of HelpSequenceId
                        // HelpSequenceId is the 1st column in ContainerDisplayHints.csv
                        if (!string.IsNullOrEmpty(cells[23]))
                        {
                            this.allHelpSequenceId.Add(cells[23]);
                        }
                    }
                }
            }
        }
    }
}
