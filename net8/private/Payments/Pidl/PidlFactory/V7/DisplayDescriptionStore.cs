// <copyright file="DisplayDescriptionStore.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// This class reads the Display description csv files and generates the Display Descriptions.
    /// </summary>
    internal sealed class DisplayDescriptionStore
    {
        private static Dictionary<string, Dictionary<string, string>> domainDictionaries;
        private Dictionary<string, Dictionary<string, string[]>> displayDictionaries;
        private Dictionary<string, Dictionary<string, List<Tuple<string, string>>>> displaySequences;
        private Dictionary<string, Dictionary<string, Dictionary<string, DisplayHint>>> propertyDisplayDescriptions;
        private Dictionary<string, Dictionary<string, ContainerDisplayHint>> containerDisplayHints;
        private Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, PIDLResourcesDisplaySequences>>>>> pidlResourceDisplaySequences;
        private Dictionary<string, Dictionary<string, PropertyDisplayErrorMessageMap>> propertyDisplayMessages;
        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> propertyDisplayDescriptionTags;
        private Dictionary<string, Dictionary<string, string>> propertyDisplayDescriptionConditionalFields;
        private Dictionary<string, Dictionary<string, List<string>>> displayStringSequences;
        private Dictionary<string, Dictionary<string, DisplayStringMap>> displayStringMap;
        private Dictionary<string, Dictionary<string, Dictionary<string, DisplayTransformation>>> displayTransformations;
        private string displayDescriptionsFolderPath = null;

        internal DisplayDescriptionStore(string displayDescriptionAbsoluteFolderPath)
        {
            if (domainDictionaries == null)
            {
                domainDictionaries = PIDLResourceFactory.GetDomainDictionaries();
            }

            this.displayDescriptionsFolderPath = displayDescriptionAbsoluteFolderPath;
        }

        internal Dictionary<string, Dictionary<string, List<Tuple<string, string>>>> DisplaySequences
        {
            get
            {
                return this.displaySequences;
            }
        }

        internal Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, PIDLResourcesDisplaySequences>>>>> PidlResourceDisplaySequences
        {
            get
            {
                return this.pidlResourceDisplaySequences;
            }
        }

        internal Dictionary<string, Dictionary<string, PropertyDisplayErrorMessageMap>> PropertyDisplayMessages
        {
            get
            {
                return this.propertyDisplayMessages;
            }
        }

        public static Dictionary<string, string[]> GetDictionaryFromConfigString(string configText, Dictionary<string, Dictionary<string, string[]>> displayDictionaries)
        {
            if (string.IsNullOrWhiteSpace(configText))
            {
                throw new PIDLArgumentException(
                    "DisplayDictionary Name is null or blank",
                    Constants.ErrorCodes.PIDLArgumentDisplayDictionaryNameIsNullOrBlank);
            }

            Dictionary<string, string[]> retVal = new Dictionary<string, string[]>(StringComparer.CurrentCultureIgnoreCase);
            if (configText.StartsWith(Constants.ConfigSpecialStrings.CollectionNamePrefix))
            {
                string dictionaryName = configText.Substring(Constants.ConfigSpecialStrings.CollectionNamePrefix.Length);
                if (displayDictionaries.ContainsKey(dictionaryName))
                {
                    retVal = displayDictionaries[dictionaryName];
                }
                else
                {
                    throw new PIDLConfigException(
                        string.Format("Config file references an unknown display dictionary name \"{0}\"", dictionaryName),
                        Constants.ErrorCodes.PIDLConfigUnknownDictionaryName);
                }
            }
            else
            {
                string[] collectionElements = configText.Split(new string[] { Constants.ConfigSpecialStrings.CollectionDelimiter }, StringSplitOptions.None);
                foreach (string element in collectionElements)
                {
                    string[] nameValuePair = element.Split(new string[] { Constants.ConfigSpecialStrings.NameValueDelimiter }, StringSplitOptions.None);
                    if (nameValuePair.Length == 1)
                    {
                        retVal[nameValuePair[0]] = null;
                    }
                    else if (nameValuePair.Length > 2)
                    {
                        throw new PIDLConfigException(
                            string.Format(
                                "Config file contains a string \"{0}\".  This has a substring \"{1}\" which has {2} \"{3}\" delimiters.  Only 0 or 1 occurence of this delimiter is allowed.",
                                configText,
                                element,
                                nameValuePair.Length - 1,
                                Constants.ConfigSpecialStrings.NameValueDelimiter),
                            Constants.ErrorCodes.PIDLConfigFileColumnIsMalformed);
                    }
                }
            }

            return retVal;
        }

        internal static ContainerDisplayHint CreateContainerDisplayHintFromTemplate(ContainerDisplayHint template)
        {
            GroupDisplayHint groupHint = template as GroupDisplayHint;
            if (groupHint != null)
            {
                GroupDisplayHint newDisplayHint = new GroupDisplayHint(groupHint);
                return newDisplayHint;
            }

            TextGroupDisplayHint textGroupHint = template as TextGroupDisplayHint;

            if (textGroupHint != null)
            {
                TextGroupDisplayHint newDisplayHint = new TextGroupDisplayHint(textGroupHint);
                return newDisplayHint;
            }

            PageDisplayHint pageGroupHint = template as PageDisplayHint;

            if (pageGroupHint != null)
            {
                PageDisplayHint newDisplayHint = new PageDisplayHint(pageGroupHint);
                return newDisplayHint;
            }

            return null;
        }

        internal void Initialize()
        {
            // ----------------------------------------------- 
            // Read DisplayDictionary CSV file
            this.ReadDisplayDictionaryConfig();

            // ---------------------------------------------------
            // Read and parse PropertyDisplayHints
            this.ReadPropertyDisplayHintConfig();

            // ---------------------------------------------------
            // Read and parse ContainerDisplayHints
            this.ReadContainerDisplayHintConfig();

            // ----------------------------------------------- 
            // Read DisplaySequences CSV file
            this.ReadDisplaySequencesConfig();

            // ---------------------------------------------------
            // Read and parse PIDLResourcesDisplaySequences
            this.ReadResourceDisplaySequencesConfig();

            // Read and parse PropertyErrorRegex config
            this.ReadPropertyErrorMessageConfig();

            // Read and parse the DisplayDescription tags config
            this.ReadPropertyDisplayDescriptionTags();

            // Read and parse the DisplayDescription conditionalFields config
            this.ReadPropertyDisplayDescriptionConditionalFields();

            // Read and parse the Display string sequence config
            this.ReadDisplayStringSequencesConfig();

            // Read and parse the Display strings config
            this.ReadDisplayStringsConfig();

            // Read and parse the Display transformations config
            this.ReadPropertyDisplayTransformations();
        }

        internal List<string> GetDisplaySequence(string displaySequenceId, string processor, List<string> flightNames)
        {
            if (string.IsNullOrWhiteSpace(displaySequenceId))
            {
                throw new ArgumentException("Parameter \"displaySequenceId\" in GetDisplaySequence is null or whitespaces.");
            }

            // processor could be empty (e.g. where a Group Display Hint does not require processor-specific specialization)
            if (processor == null)
            {
                throw new ArgumentNullException("processor");
            }

            if (!this.displaySequences.ContainsKey(displaySequenceId))
            {
                return null;
            }

            string processorKey = processor;
            if (!this.displaySequences[displaySequenceId].ContainsKey(processorKey))
            {
                processorKey = GlobalConstants.Defaults.ProcessorKey;
                if (!this.displaySequences[displaySequenceId].ContainsKey(processorKey))
                {
                    return null;
                }
            }

            List<string> hintsList = new List<string>();

            foreach (Tuple<string, string> flightHint in this.displaySequences[displaySequenceId][processorKey])
            {
                if (flightNames == null)
                {
                    flightNames = new List<string>() { string.Empty };
                }

                // If flightInFlightHint is empty
                // or flightNames contains flightInFlightHint XOR flightInFlightHint starts with !
                // Add displayHintId to list
                bool negativeFlightName = IsNegativeFlightName(flightHint.Item1);
                string flightInFlightHint = negativeFlightName ? flightHint.Item1.Substring(1) : flightHint.Item1;
                string displayHintIdInFlightHint = flightHint.Item2;

                if (string.IsNullOrEmpty(flightInFlightHint) || (flightNames.Contains(flightInFlightHint) ^ negativeFlightName))
                {
                    hintsList.Add(flightHint.Item2);
                }
            }

            return hintsList;
        }

        internal ContainerDisplayHint GetContainerDisplayHint(string displayHintId, string processor)
        {
            if (string.IsNullOrWhiteSpace(displayHintId))
            {
                throw new ArgumentException("Parameter \"displayHintId\" in GetGroupDisplayHint is null or whitespaces.");
            }

            // processor could be empty (e.g. where a Group Display Hint does not require processor-specific specialization)
            if (processor == null)
            {
                throw new ArgumentNullException("processor");
            }

            if (!this.containerDisplayHints.ContainsKey(displayHintId))
            {
                return null;
            }

            string processorKey = processor;
            if (!this.containerDisplayHints[displayHintId].ContainsKey(processorKey))
            {
                processorKey = GlobalConstants.Defaults.ProcessorKey;
                if (!this.containerDisplayHints[displayHintId].ContainsKey(processorKey))
                {
                    return null;
                }
            }

            return this.containerDisplayHints[displayHintId][processorKey];
        }

        internal DisplayHint GetPropertyDisplayHint(string displayHintId, string processor, List<string> flightNames)
        {
            if (string.IsNullOrWhiteSpace(displayHintId))
            {
                throw new ArgumentException("Parameter \"displayHintId\" in GetPropertyDisplayHint is null or whitespaces.");
            }

            // processor could be empty (e.g. where a Property Display Hint does not require processor-specific specialization)
            if (processor == null)
            {
                throw new ArgumentNullException("processor");
            }

            if (!this.propertyDisplayDescriptions.ContainsKey(displayHintId))
            {
                return null;
            }

            string processorKey = processor;
            if (!this.propertyDisplayDescriptions[displayHintId].ContainsKey(processorKey))
            {
                processorKey = GlobalConstants.Defaults.ProcessorKey;
                if (!this.propertyDisplayDescriptions[displayHintId].ContainsKey(processorKey))
                {
                    return null;
                }
            }

            Dictionary<string, DisplayHint> featureNamesToDisplayHintMappings = this.propertyDisplayDescriptions[displayHintId][processorKey];

            DisplayHint displayHint = this.propertyDisplayDescriptions[displayHintId][processorKey][GlobalConstants.Defaults.FeatureNameKey];

            if (flightNames == null || flightNames.Count == 0)
            {
                return displayHint;
            }
            else
            {
                // FeatureName column could contain one featureName like "removeOptionalInLabel"
                // or multiple featureNames separated by semicolon like "removeOptionalInLabel;removeAdditonalInformation"
                HashSet<string> flightNamesSet = new HashSet<string>(flightNames, StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, DisplayHint> featureNamesToDisplayHintMapping in featureNamesToDisplayHintMappings)
                {
                    string[] featureNames = featureNamesToDisplayHintMapping.Key.Split(';');
                    if (featureNames.All(featureName => flightNamesSet.Contains(featureName)))
                    {
                        return featureNamesToDisplayHintMapping.Value;
                    }
                }
            }

            return displayHint;
        }

        internal Dictionary<string, string> GetPropertyDisplayDescriptionTags(string displayHintId, string country)
        {
            // If the display description tag is null, then tags were not present for the partner
            if (this.propertyDisplayDescriptionTags == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(displayHintId))
            {
                throw new ArgumentException("Parameter \"displayHintId\" in GetPropertyDisplayDescriptionTags is null or whitespaces.");
            }

            // country could be empty (e.g. where a Property Display Hint does not require processor-specific specialization)
            if (country == null)
            {
                throw new ArgumentNullException("country");
            }

            if (!this.propertyDisplayDescriptionTags.ContainsKey(displayHintId))
            {
                return null;
            }

            string countryKey = country;
            if (!this.propertyDisplayDescriptionTags[displayHintId].ContainsKey(countryKey))
            {
                countryKey = GlobalConstants.Defaults.CountryKey;
                if (!this.propertyDisplayDescriptionTags[displayHintId].ContainsKey(countryKey))
                {
                    return null;
                }
            }

            return this.propertyDisplayDescriptionTags[displayHintId][countryKey];
        }

        internal Dictionary<string, string> GetPropertyDisplayDescriptionConditionalFields(string displayHintId)
        {
            // If the display description tag is null, then tags were not present for the partner
            if (this.propertyDisplayDescriptionConditionalFields == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(displayHintId))
            {
                throw new ArgumentException("Parameter \"displayHintId\" in GetPropertyDisplayDescriptionConditionalFields is null or whitespaces.");
            }

            if (!this.propertyDisplayDescriptionConditionalFields.ContainsKey(displayHintId))
            {
                return null;
            }

            return this.propertyDisplayDescriptionConditionalFields[displayHintId];
        }

        internal Dictionary<string, DisplayTransformation> GetDisplayTransformations(string displayHintId, string country)
        {
            // If the display transformations is null, then transformations were not present for the partner
            if (this.displayTransformations == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(displayHintId))
            {
                throw new ArgumentException("Parameter \"displayHintId\" in GetDisplayTransformations is null or whitespaces.");
            }

            // country could be empty (e.g. where a Display Hint does not require processor-specific specialization)
            if (country == null)
            {
                throw new ArgumentNullException("country");
            }

            if (!this.displayTransformations.ContainsKey(displayHintId))
            {
                return null;
            }

            string countryKey = country;
            if (!this.displayTransformations[displayHintId].ContainsKey(countryKey))
            {
                countryKey = GlobalConstants.Defaults.CountryKey;
                if (!this.displayTransformations[displayHintId].ContainsKey(countryKey))
                {
                    return null;
                }
            }

            return this.displayTransformations[displayHintId][countryKey];
        }

        internal DisplayStringMap GetDisplayStringMap(string displayStringId, string country)
        {
            if (string.IsNullOrWhiteSpace(displayStringId))
            {
                throw new ArgumentException("Parameter \"displayStringId\" in GetDisplayStringMap is null or whitespaces.");
            }

            if (this.displayStringMap == null)
            {
                throw new PIDLConfigException(
                    string.Format("No Display string map found for DisplayStringId: {0}", displayStringId),
                    Constants.ErrorCodes.PIDLInvalidDisplayStringMapping);
            }

            // country could be empty (e.g. where a Property Display Hint does not require processor-specific specialization)
            if (country == null)
            {
                throw new ArgumentNullException("country");
            }

            if (!this.displayStringMap.ContainsKey(displayStringId))
            {
                throw new PIDLConfigException(
                    string.Format("No Display string map found for DisplayStringId: {0}", displayStringId),
                    Constants.ErrorCodes.PIDLInvalidDisplayStringMapping);
            }

            string countryKey = country;
            if (!this.displayStringMap[displayStringId].ContainsKey(countryKey))
            {
                countryKey = GlobalConstants.Defaults.CountryKey;
                if (!this.displayStringMap[displayStringId].ContainsKey(countryKey))
                {
                    throw new PIDLConfigException(
                        string.Format("No Display string map found for DisplayStringId: {0} for country : {1}", displayStringId, countryKey),
                        Constants.ErrorCodes.PIDLInvalidDisplayStringMapping);
                }
            }

            return this.displayStringMap[displayStringId][countryKey];
        }

        internal List<string> GetDisplayStringSequence(string displayStringSequenceId, string processor)
        {
            if (string.IsNullOrWhiteSpace(displayStringSequenceId))
            {
                throw new ArgumentException("Parameter \"displayStringSequenceId\" in GetDisplayStringSequence is null or whitespaces.");
            }

            if (processor == null)
            {
                throw new ArgumentNullException("processor");
            }

            if (this.displayStringSequences == null)
            {
                throw new PIDLConfigException(
                       string.Format("No Display string sequence found for DisplayStringSequenceId: {0}", displayStringSequenceId),
                       Constants.ErrorCodes.PIDLInvalidDisplayStringMapping);
            }

            if (!this.displayStringSequences.ContainsKey(displayStringSequenceId))
            {
                throw new PIDLConfigException(
                      string.Format("No Display string sequence found for DisplayStringSequenceId: {0}", displayStringSequenceId),
                      Constants.ErrorCodes.PIDLInvalidDisplayStringMapping);
            }

            string processorKey = processor;
            if (!this.displayStringSequences[displayStringSequenceId].ContainsKey(processorKey))
            {
                processorKey = GlobalConstants.Defaults.ProcessorKey;
                if (!this.displayStringSequences[displayStringSequenceId].ContainsKey(processorKey))
                {
                    throw new PIDLConfigException(
                      string.Format("No Display string sequence found for DisplayStringSequenceId: {0} for processor: {1}", displayStringSequenceId, processorKey),
                      Constants.ErrorCodes.PIDLInvalidDisplayStringMapping);
                }
            }

            return this.displayStringSequences[displayStringSequenceId][processorKey];
        }

        private static bool IsNegativeFlightName(string flightId)
        {
            return !string.IsNullOrEmpty(flightId) && flightId.StartsWith("!");
        }

        private static bool? GetIsModalGroupProperty(string id)
        {
            if (Constants.ModalGroupIds.Contains(id))
            {
                return true;
            }
            else
            {
                return null;
            }
        }

        private string GetDisplayDescriptionFullPath(string displayDescriptionFileName)
        {
            return Path.Combine(this.displayDescriptionsFolderPath, displayDescriptionFileName);
        }

        private void ReadContainerDisplayHintConfig()
        {
            using (PIDLConfigParser parser = new PIDLConfigParser(
                this.GetDisplayDescriptionFullPath(Constants.DisplayDescriptionFileNames.ContainerDisplayDescriptionsCSV),
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
                var displayHints = new Dictionary<string, Dictionary<string, ContainerDisplayHint>>(StringComparer.CurrentCultureIgnoreCase);
                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    string id = cells[ContainerCellIndexDescription.ContainerHintId];
                    string countryConfig = cells[ContainerCellIndexDescription.CountryId];
                    string displaySequenceId = cells[ContainerCellIndexDescription.DisplaySequenceId];
                    string displayName = cells[ContainerCellIndexDescription.DisplayName];
                    string displayType = cells[ContainerCellIndexDescription.DisplayType];
                    string layoutOrientation = string.IsNullOrEmpty(cells[ContainerCellIndexDescription.LayoutOrientation]) ? null : cells[ContainerCellIndexDescription.LayoutOrientation];
                    string layoutAlignment = string.IsNullOrEmpty(cells[ContainerCellIndexDescription.LayoutAlignment]) ? null : cells[ContainerCellIndexDescription.LayoutAlignment];
                    string containerDescription = string.IsNullOrEmpty(cells[ContainerCellIndexDescription.ContainerDescription]) ? null : cells[ContainerCellIndexDescription.ContainerDescription];
                    DisplayCondition displayCondition = string.IsNullOrEmpty(cells[ContainerCellIndexDescription.DisplayConditionFunctionName]) ? null : new DisplayCondition(cells[ContainerCellIndexDescription.DisplayConditionFunctionName]);
                    string dataCollectionSource = string.IsNullOrEmpty(cells[ContainerCellIndexDescription.DataCollectionSource]) ? null : cells[ContainerCellIndexDescription.DataCollectionSource];
                    DataCollectionFilterDescription dataCollectionFilterDescription = string.IsNullOrEmpty(cells[ContainerCellIndexDescription.DataCollectionFilterFunctionName]) ? null : new DataCollectionFilterDescription(cells[ContainerCellIndexDescription.DataCollectionFilterFunctionName]);
                    IEnumerable<string> styleHints = PidlFactoryHelper.ParseStyleHints(cells[ContainerCellIndexDescription.StyleHints]);
                    ContainerDisplayHint newDisplayHint = null;

                    if (string.Equals(displayType, HintType.Group.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(displayType, HintType.DataCollectionBindingGroup.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        GroupDisplayHint newGroupDispalyHint = new GroupDisplayHint()
                        {
                            HintId = id,
                            DisplaySequenceId = displaySequenceId,
                            DisplayName = displayName,
                            ContainerDisplayType = displayType,
                            LayoutOrientation = layoutOrientation,
                            LayoutAlignment = layoutAlignment,
                            DisplayCondition = displayCondition,
                            DataCollectionSource = dataCollectionSource,
                            DataCollectionFilterDescription = dataCollectionFilterDescription,
                            IsModalGroup = GetIsModalGroupProperty(id),
                            StyleHints = styleHints
                        };

                        if (!string.IsNullOrWhiteSpace(cells[ContainerCellIndexDescription.ShowDisplayName]))
                        {
                            ShowDisplayNameOption showDisplayNameOption;
                            if (!Enum.TryParse<ShowDisplayNameOption>(cells[ContainerCellIndexDescription.ShowDisplayName].ToUpper(), out showDisplayNameOption))
                            {
                                throw new PIDLConfigException(
                                    Constants.DisplayDescriptionFileNames.ContainerDisplayDescriptionsCSV,
                                    parser.LineNumber,
                                    string.Format("Column {0} could not be parsed.  Allowed strings in this column are true, false, optional.", 7),
                                    Constants.ErrorCodes.PIDLConfigFileColumnIsMalformed);
                            }
                            else
                            {
                                newGroupDispalyHint.ShowDisplayName = showDisplayNameOption.ToString().ToLower();
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(cells[ContainerCellIndexDescription.IsSubmitGroup]))
                        {
                            bool isSubmitGroupOption;
                            if (!bool.TryParse(cells[ContainerCellIndexDescription.IsSubmitGroup].ToUpper(), out isSubmitGroupOption))
                            {
                                throw new PIDLConfigException(
                                    Constants.DisplayDescriptionFileNames.ContainerDisplayDescriptionsCSV,
                                    parser.LineNumber,
                                    string.Format("Column {0} could not be parsed.  Allowed strings in this column are true and false.", 8),
                                    Constants.ErrorCodes.PIDLConfigFileColumnIsMalformed);
                            }
                            else
                            {
                                newGroupDispalyHint.IsSumbitGroup = isSubmitGroupOption;
                            }
                        }

                        newDisplayHint = newGroupDispalyHint;
                    }
                    else if (string.Equals(displayType, HintType.TextGroup.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        newDisplayHint = new TextGroupDisplayHint()
                        {
                            HintId = id,
                            DisplaySequenceId = displaySequenceId,
                            ContainerDisplayType = displayType,
                            LayoutOrientation = layoutOrientation,
                            LayoutAlignment = layoutAlignment,
                            StyleHints = styleHints
                        };
                    }
                    else if (string.Equals(displayType, HintType.Page.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        PageDisplayHint newPageDisplayHint = new PageDisplayHint()
                        {
                            HintId = id,
                            DisplayName = displayName,
                            DisplaySequenceId = displaySequenceId,
                            ContainerDisplayType = displayType,
                            ContainerDescription = containerDescription,
                            StyleHints = styleHints
                        };

                        if (!string.IsNullOrWhiteSpace(cells[ContainerCellIndexDescription.Extend]))
                        {
                            bool extendOption;
                            if (!bool.TryParse(cells[ContainerCellIndexDescription.Extend].ToUpper(), out extendOption))
                            {
                                throw new PIDLConfigException(
                                    Constants.DisplayDescriptionFileNames.ContainerDisplayDescriptionsCSV,
                                    parser.LineNumber,
                                    string.Format("Column {0} could not be parsed.  Allowed strings in this column are true and false.", 9),
                                    Constants.ErrorCodes.PIDLConfigFileColumnIsMalformed);
                            }
                            else
                            {
                                newPageDisplayHint.Extend = extendOption;
                                newPageDisplayHint.FirstButtonGroup = string.IsNullOrEmpty(cells[ContainerCellIndexDescription.FirstButtonGroup]) ? null : cells[ContainerCellIndexDescription.FirstButtonGroup];
                                newPageDisplayHint.ExtendButtonGroup = string.IsNullOrEmpty(cells[ContainerCellIndexDescription.ExtendButtonGroup]) ? null : cells[ContainerCellIndexDescription.ExtendButtonGroup];
                            }
                        }

                        newDisplayHint = newPageDisplayHint;
                    }
                    else
                    {
                        throw new PIDLConfigException(
                                   Constants.DisplayDescriptionFileNames.ContainerDisplayDescriptionsCSV,
                                   parser.LineNumber,
                                   string.Format("Column {0} could not be parsed.  Invalid Container Display Description: {1} found", 3, displayType),
                                   Constants.ErrorCodes.PIDLConfigFileColumnIsMalformed);
                    }

                    if (!displayHints.ContainsKey(id))
                    {
                        displayHints[id] = new Dictionary<string, ContainerDisplayHint>(StringComparer.CurrentCultureIgnoreCase);
                    }

                    if (string.IsNullOrEmpty(countryConfig))
                    {
                        displayHints[id][string.Empty] = newDisplayHint;
                    }
                    else
                    {
                        Dictionary<string, string[]> parsedCountries = GetDictionaryFromConfigString(countryConfig, this.displayDictionaries);
                        foreach (string countryId in parsedCountries.Keys)
                        {
                            displayHints[id][countryId] = newDisplayHint;
                        }
                    }
                }

                this.containerDisplayHints = displayHints;
            }
        }

        private void ReadPropertyDisplayHintConfig()
        {
            string fileName = this.GetDisplayDescriptionFullPath(Constants.DisplayDescriptionFileNames.PropertyDisplayDescriptionsCSV);

            using (PIDLConfigParser parser = new PIDLConfigParser(
                fileName,
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
                    new ColumnDefinition("StyleHints",                            ColumnConstraint.Optional, ColumnFormat.Text),
                },
                true))
            {
                var displayHints = new Dictionary<string, Dictionary<string, Dictionary<string, DisplayHint>>>(StringComparer.CurrentCultureIgnoreCase);
                Dictionary<string, Dictionary<string, DisplayHint>> currentDisplayHint = null;
                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    string id = cells[PropertyDisplayHintFactory.CellIndexDescription.PropertyHintId];
                    string countryConfig = cells[PropertyDisplayHintFactory.CellIndexDescription.CountryId];
                    string featureNameConfig = cells[PropertyDisplayHintFactory.CellIndexDescription.FeatureName];

                    PropertyDisplayHintFactory properyHintFactory = new PropertyDisplayHintFactory(cells, fileName, parser.LineNumber);

                    DisplayHint newDisplayHint = properyHintFactory.CreateDisplayHint(this.displayDictionaries);

                    if (!displayHints.TryGetValue(id, out currentDisplayHint))
                    {
                        currentDisplayHint = new Dictionary<string, Dictionary<string, DisplayHint>>(StringComparer.CurrentCultureIgnoreCase);
                        displayHints[id] = currentDisplayHint;
                    }

                    List<string> parsedCountries = null;
                    if (string.IsNullOrEmpty(countryConfig))
                    {
                        parsedCountries = new List<string>() { string.Empty };
                    }
                    else
                    {
                        parsedCountries = GetDictionaryFromConfigString(countryConfig, this.displayDictionaries).Keys.ToList();
                    }

                    foreach (string countryId in parsedCountries)
                    {
                        if (!currentDisplayHint.ContainsKey(countryId))
                        {
                            currentDisplayHint[countryId] = new Dictionary<string, DisplayHint>(StringComparer.CurrentCultureIgnoreCase);
                        }

                        currentDisplayHint[countryId][featureNameConfig] = newDisplayHint;
                    }
                }

                this.propertyDisplayDescriptions = displayHints;
            }
        }

        private void ReadResourceDisplaySequencesConfig()
        {
            using (PIDLConfigParser parser = new PIDLConfigParser(
               this.GetDisplayDescriptionFullPath(Constants.DisplayDescriptionFileNames.PIDLResourcesDisplaySequencesCSV),
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
                if (this.pidlResourceDisplaySequences == null)
                {
                    this.pidlResourceDisplaySequences = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, PIDLResourcesDisplaySequences>>>>>(StringComparer.CurrentCultureIgnoreCase);
                }

                PIDLResourcesDisplaySequences currentPidlDisplaySequences = null;
                string currentIdString = string.Empty;
                string currentPidlResourcetype = string.Empty;
                string currentCountryConfig = string.Empty;
                string currentOperation = string.Empty;
                string currentScenario = string.Empty;

                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();
                    string pidlResourceType = cells[PidlSequenceCellIndexDescription.PIDLResourceType];
                    string idString = string.IsNullOrWhiteSpace(cells[PidlSequenceCellIndexDescription.PIDLResourceIdentity]) ? string.Empty : cells[PidlSequenceCellIndexDescription.PIDLResourceIdentity];
                    string operation = string.IsNullOrWhiteSpace(cells[PidlSequenceCellIndexDescription.Operation]) ? string.Empty : cells[PidlSequenceCellIndexDescription.Operation];
                    string countryConfig = string.IsNullOrWhiteSpace(cells[PidlSequenceCellIndexDescription.CountryIds]) ? string.Empty : cells[PidlSequenceCellIndexDescription.CountryIds];
                    string scenarioConfig = string.IsNullOrWhiteSpace(cells[PidlSequenceCellIndexDescription.Scenario]) ? string.Empty : cells[PidlSequenceCellIndexDescription.Scenario];
                    string displaySequenceId = string.IsNullOrWhiteSpace(cells[PidlSequenceCellIndexDescription.DisplaySequenceId]) ? string.Empty : cells[PidlSequenceCellIndexDescription.DisplaySequenceId];
                    string displayStringsSequenceId = string.IsNullOrWhiteSpace(cells[PidlSequenceCellIndexDescription.DisplayStringSequenceId]) ? string.Empty : cells[PidlSequenceCellIndexDescription.DisplayStringSequenceId];

                    if (!string.Equals(currentPidlResourcetype, pidlResourceType, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(currentIdString, idString, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(currentCountryConfig, countryConfig, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(currentOperation, operation, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(currentScenario, scenarioConfig, StringComparison.OrdinalIgnoreCase))
                    {
                        // This is a new PIDLResourcesDisplaySequences.
                        currentPidlResourcetype = pidlResourceType;
                        currentIdString = idString;
                        currentCountryConfig = countryConfig;
                        currentOperation = operation;
                        currentScenario = scenarioConfig;
                        currentPidlDisplaySequences = new PIDLResourcesDisplaySequences();

                        PidlFactoryHelper.ResolvePidlResourceIdentity<PIDLResourcesDisplaySequences>(
                            this.pidlResourceDisplaySequences,
                            currentPidlResourcetype,
                            currentIdString,
                            currentOperation,
                            currentCountryConfig,
                            currentScenario,
                            () =>
                            {
                                return currentPidlDisplaySequences;
                            });
                    }

                    // This row is a continuation of the current PIDLResourcesDisplayHints.
                    currentPidlDisplaySequences.DisplaySequenceIds.Add(displaySequenceId);
                    if (!string.IsNullOrEmpty(displayStringsSequenceId))
                    {
                        currentPidlDisplaySequences.DisplayStringsSequenceIds.Add(displayStringsSequenceId);
                    }
                }
            }
        }

        private void ReadDisplayDictionaryConfig()
        {
            using (PIDLConfigParser dictionaryParser = new PIDLConfigParser(
                this.GetDisplayDescriptionFullPath(Constants.DisplayDescriptionFileNames.DisplayDictionariesCSV),
                new[]
                {
                    new ColumnDefinition("DictionaryName", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Key",            ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Name",           ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DisplayHintId",  ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                },
                true))
            {
                Dictionary<string, string[]> currentDictionary = null;

                if (this.displayDictionaries == null)
                {
                    this.displayDictionaries = new Dictionary<string, Dictionary<string, string[]>>(StringComparer.CurrentCultureIgnoreCase);
                }

                while (!dictionaryParser.EndOfData)
                {
                    string[] cells = dictionaryParser.ReadValidatedFields();

                    if (string.IsNullOrWhiteSpace(cells[0]))
                    {
                        if (currentDictionary == null)
                        {
                            throw new PIDLConfigException(
                                Constants.DisplayDescriptionFileNames.DisplayDictionariesCSV,
                                dictionaryParser.LineNumber,
                                string.Format("Name of the first display dictionary is missing."),
                                Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                        }
                    }
                    else
                    {
                        if (!this.displayDictionaries.TryGetValue(cells[0], out currentDictionary))
                        {
                            currentDictionary = new Dictionary<string, string[]>(StringComparer.CurrentCultureIgnoreCase);
                            this.displayDictionaries[cells[0]] = currentDictionary;
                        }
                    }

                    string key = cells[1];

                    // If the key contains an expression (say "({partnerData.id})"), then the key should not be changed to lowercase.
                    if (!(key.StartsWith("(", StringComparison.OrdinalIgnoreCase) && key.EndsWith(")", StringComparison.OrdinalIgnoreCase)))
                    {
                        key = key.ToLower();
                    }

                    if (currentDictionary.ContainsKey(key))
                    {
                        throw new PIDLConfigException(
                            Constants.DataDescriptionFilePaths.DomainDictionariesCSV,
                            dictionaryParser.LineNumber,
                            string.Format("Duplicate Key \"{0}\" found in the config file.", key),
                            Constants.ErrorCodes.PIDLConfigDuplicateId);
                    }
                    else
                    {
                        currentDictionary[key] = new string[] { cells[2], cells[3] };
                    }
                }
            }

            // As part of [Bug Fix - 43357499] Remove the Common Config from DisplayDictionaries.csv of each partner and 
           // with the below mention logic getting their data from DomainDictionaries.csv and updated the DisplayDictionaries
            foreach (var outerKvp in domainDictionaries)
            {
                if (!this.displayDictionaries.TryGetValue(outerKvp.Key, out var innerDictionary))
                {
                    innerDictionary = outerKvp.Value.ToDictionary(kvp => kvp.Key, kvp => new string[] { kvp.Value, null });
                    this.displayDictionaries.Add(outerKvp.Key, innerDictionary);
                }
            }
        }

        private void ReadDisplaySequencesConfig()
        {
            using (PIDLConfigParser dictionaryParser = new PIDLConfigParser(
                this.GetDisplayDescriptionFullPath(Constants.DisplayDescriptionFileNames.DisplaySequencesCSV),
                new[]
                {
                    new ColumnDefinition("DisplaySequenceId",     ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds",            ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("FlightName",            ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("HintId",                ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                },
                true))
            {
                Dictionary<string, List<Tuple<string, string>>> currentDictionary = null;

                if (this.displaySequences == null)
                {
                    this.displaySequences = new Dictionary<string, Dictionary<string, List<Tuple<string, string>>>>();
                }

                while (!dictionaryParser.EndOfData)
                {
                    string[] cells = dictionaryParser.ReadValidatedFields();

                    string displaySequenceId = cells[DisplaySequenceCellIndexDescription.DisplaySequenceId];
                    string countryConfig = cells[DisplaySequenceCellIndexDescription.CountryId];
                    string flightConfig = cells[DisplaySequenceCellIndexDescription.FlightName];
                    string hintId = cells[DisplaySequenceCellIndexDescription.HintId];

                    if (string.IsNullOrWhiteSpace(displaySequenceId))
                    {
                        if (currentDictionary == null)
                        {
                            throw new PIDLConfigException(
                                Constants.DisplayDescriptionFileNames.DisplaySequencesCSV,
                                dictionaryParser.LineNumber,
                                string.Format("Name of the first group DisplaySequenceId is missing."),
                                Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                        }
                    }
                    else
                    {
                        if (!this.displaySequences.TryGetValue(displaySequenceId, out currentDictionary))
                        {
                            currentDictionary = new Dictionary<string, List<Tuple<string, string>>>(StringComparer.CurrentCultureIgnoreCase);
                            this.displaySequences[displaySequenceId] = currentDictionary;
                        }
                    }

                    List<string> parsedCountries = null;
                    if (string.IsNullOrEmpty(countryConfig))
                    {
                        parsedCountries = new List<string>() { string.Empty };
                    }
                    else
                    {
                        parsedCountries = GetDictionaryFromConfigString(countryConfig, this.displayDictionaries).Keys.ToList();
                    }

                    foreach (string countryId in parsedCountries)
                    {
                        if (!currentDictionary.ContainsKey(countryId))
                        {
                            currentDictionary[countryId] = new List<Tuple<string, string>>();
                        }

                        currentDictionary[countryId].Add(new Tuple<string, string>(flightConfig, hintId));
                    }

                    string errorMessage;

                    if (!PidlFactoryHelper.ValidatePidlSequenceId(hintId, out errorMessage))
                    {
                        throw new PIDLConfigException(
                            this.GetDisplayDescriptionFullPath(Constants.DisplayDescriptionFileNames.DisplaySequencesCSV),
                            dictionaryParser.LineNumber,
                            errorMessage,
                            Constants.ErrorCodes.PIDLConfigPIDLResourceIdIsMalformed);
                    }
                }
            }
        }

        private void ReadPropertyErrorMessageConfig()
        {
            string filePath = this.GetDisplayDescriptionFullPath(Constants.DisplayDescriptionFileNames.PropertyErrorMessagesCSV);
            using (PIDLConfigParser parser = new PIDLConfigParser(
                filePath,
                new ColumnDefinition[]
                {
                    new ColumnDefinition("PropertyHintId", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("MessageSource", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DefaultErrorMessage", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ErrorRegEx", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ErrorCode", ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ErrorMessage", ColumnConstraint.Required, ColumnFormat.AlphaNumeric)
                },
                true))
            {
                if (this.propertyDisplayMessages == null)
                {
                    this.propertyDisplayMessages = new Dictionary<string, Dictionary<string, PropertyDisplayErrorMessageMap>>(StringComparer.OrdinalIgnoreCase);
                }

                Dictionary<string, PropertyDisplayErrorMessageMap> currentDisplayMessages = null;
                MessageSourceType? currentMessageSource = null;
                string currentPropertyHintId = null;
                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    string propertyHintId = cells[PropertyErrorRegexCellIndexDescription.PropertyHintId];
                    string countryConfig = cells[PropertyErrorRegexCellIndexDescription.CountryIds];
                    string messageSource = cells[PropertyErrorRegexCellIndexDescription.MessageSource];
                    string defaultErrorMessage = cells[PropertyErrorRegexCellIndexDescription.DefaultErrorMessage];
                    string errorRegex = string.IsNullOrWhiteSpace(cells[PropertyErrorRegexCellIndexDescription.ErrorRegEx]) ? null : cells[PropertyErrorRegexCellIndexDescription.ErrorRegEx];
                    string errorCode = cells[PropertyErrorRegexCellIndexDescription.ErrorCode];
                    string errorMessage = cells[PropertyErrorRegexCellIndexDescription.ErrorMessage];

                    if (string.IsNullOrWhiteSpace(propertyHintId))
                    {
                        if (string.IsNullOrEmpty(currentPropertyHintId))
                        {
                            throw new PIDLConfigException(
                                filePath,
                                parser.LineNumber,
                                string.Format("propertyHintId is missing."),
                                Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                        }
                    }
                    else
                    {
                        currentPropertyHintId = propertyHintId;
                    }

                    if (string.IsNullOrWhiteSpace(messageSource))
                    {
                        if (currentMessageSource == null)
                        {
                            throw new PIDLConfigException(
                                filePath,
                                parser.LineNumber,
                                string.Format("messageSource is missing."),
                                Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                        }
                    }
                    else
                    {
                        MessageSourceType messageSourceType;

                        if (!Enum.TryParse<MessageSourceType>(messageSource.ToLower(), out messageSourceType))
                        {
                            throw new PIDLConfigException(
                                filePath,
                                parser.LineNumber,
                                string.Format("Column {0} could not be parsed.  Allowed strings in this column are fromErrorCode or fromRegex.", 3),
                                Constants.ErrorCodes.PIDLConfigFileColumnIsMalformed);
                        }

                        currentMessageSource = messageSourceType;
                    }

                    if (!this.propertyDisplayMessages.TryGetValue(currentPropertyHintId, out currentDisplayMessages))
                    {
                        currentDisplayMessages = new Dictionary<string, PropertyDisplayErrorMessageMap>(StringComparer.CurrentCultureIgnoreCase);
                        this.propertyDisplayMessages[currentPropertyHintId] = currentDisplayMessages;
                    }

                    List<string> parsedCountries = null;
                    if (string.IsNullOrEmpty(countryConfig))
                    {
                        parsedCountries = new List<string>() { string.Empty };
                    }
                    else
                    {
                        parsedCountries = GetDictionaryFromConfigString(countryConfig, this.displayDictionaries).Keys.ToList();
                    }

                    foreach (string countryId in parsedCountries)
                    {
                        if (!currentDisplayMessages.ContainsKey(countryId))
                        {
                            currentDisplayMessages[countryId] = new PropertyDisplayErrorMessageMap();
                        }

                        if (!string.IsNullOrEmpty(defaultErrorMessage))
                        {
                            currentDisplayMessages[countryId].DefaultErrorMessage = defaultErrorMessage;
                        }

                        currentDisplayMessages[countryId].AddDisplayMessage(
                            currentMessageSource,
                            new PropertyDisplayErrorMessage()
                            {
                                ErrorCode = errorCode,
                                ErrorMessage = errorMessage,
                                Regex = errorRegex
                            });
                    }
                }
            }
        }

        private void ReadPropertyDisplayDescriptionTags()
        {
            string filePath = this.GetDisplayDescriptionFullPath(Constants.DisplayDescriptionFileNames.DisplayDescriptionTagsCSV);

            if (!File.Exists(filePath))
            {
                return;
            }

            using (PIDLConfigParser parser = new PIDLConfigParser(
                filePath,
                new ColumnDefinition[]
                {
                    new ColumnDefinition("PropertyHintId", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("TagKey", ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("TagValue", ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                },
                true))
            {
                if (this.propertyDisplayDescriptionTags == null)
                {
                    this.propertyDisplayDescriptionTags = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>(
                        StringComparer.OrdinalIgnoreCase);
                }

                Dictionary<string, Dictionary<string, string>> currentPropertyDisplayTags = null;

                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    string propertyHintId = cells[PropertyDisplayDescriptionTagsCellIndexDescription.PropertyHintId];
                    string countryConfig = cells[PropertyDisplayDescriptionTagsCellIndexDescription.CountryIds];
                    string tagKey = cells[PropertyDisplayDescriptionTagsCellIndexDescription.TagKey];
                    string tagValue = cells[PropertyDisplayDescriptionTagsCellIndexDescription.TagValue];

                    if (string.IsNullOrWhiteSpace(propertyHintId))
                    {
                        if (currentPropertyDisplayTags == null)
                        {
                            throw new PIDLConfigException(
                                filePath,
                                parser.LineNumber,
                                string.Format("PropertyHintId is missing."),
                                Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                        }
                    }
                    else
                    {
                        if (!this.propertyDisplayDescriptionTags.TryGetValue(propertyHintId, out currentPropertyDisplayTags))
                        {
                            currentPropertyDisplayTags = new Dictionary<string, Dictionary<string, string>>(StringComparer.CurrentCultureIgnoreCase);
                            this.propertyDisplayDescriptionTags[propertyHintId] = currentPropertyDisplayTags;
                        }
                    }

                    List<string> parsedCountries = null;
                    if (string.IsNullOrEmpty(countryConfig))
                    {
                        parsedCountries = new List<string>() { string.Empty };
                    }
                    else
                    {
                        parsedCountries = GetDictionaryFromConfigString(countryConfig, this.displayDictionaries).Keys.ToList();
                    }

                    foreach (string countryId in parsedCountries)
                    {
                        if (!currentPropertyDisplayTags.ContainsKey(countryId))
                        {
                            currentPropertyDisplayTags[countryId] = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
                        }

                        currentPropertyDisplayTags[countryId][tagKey] = tagValue;
                    }
                }
            }
        }

        private void ReadPropertyDisplayDescriptionConditionalFields()
        {
            string filePath = this.GetDisplayDescriptionFullPath(Constants.DisplayDescriptionFileNames.DisplayDescriptionConditionalFieldsCSV);

            if (!File.Exists(filePath))
            {
                return;
            }

            using (PIDLConfigParser parser = new PIDLConfigParser(
                filePath,
                new ColumnDefinition[]
                {
                    new ColumnDefinition("PropertyHintId", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ConditionalFieldKey", ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ConditionalFieldValue", ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                },
                true))
            {
                if (this.propertyDisplayDescriptionConditionalFields == null)
                {
                    this.propertyDisplayDescriptionConditionalFields = new Dictionary<string, Dictionary<string, string>>(
                        StringComparer.OrdinalIgnoreCase);
                }

                Dictionary<string, string> currentPropertyDisplayConditionalFields = null;

                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    string propertyHintId = cells[PropertyDisplayDescriptionConditionalFieldsCellIndexDescription.PropertyHintId];
                    string conditionalFieldKey = cells[PropertyDisplayDescriptionConditionalFieldsCellIndexDescription.ConditionalFieldKey];
                    string conditionalFieldValue = cells[PropertyDisplayDescriptionConditionalFieldsCellIndexDescription.ConditionalFieldValue];

                    if (string.IsNullOrWhiteSpace(propertyHintId))
                    {
                        if (currentPropertyDisplayConditionalFields == null)
                        {
                            throw new PIDLConfigException(
                                filePath,
                                parser.LineNumber,
                                string.Format("PropertyHintId is missing."),
                                Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                        }
                    }
                    else
                    {
                        if (!this.propertyDisplayDescriptionConditionalFields.TryGetValue(propertyHintId, out currentPropertyDisplayConditionalFields))
                        {
                            currentPropertyDisplayConditionalFields = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
                            this.propertyDisplayDescriptionConditionalFields[propertyHintId] = currentPropertyDisplayConditionalFields;
                        }
                    }

                    List<string> parsedKeys = null;
                    if (string.IsNullOrEmpty(conditionalFieldKey))
                    {
                        parsedKeys = new List<string>() { string.Empty };
                    }
                    else
                    {
                        parsedKeys = GetDictionaryFromConfigString(conditionalFieldKey, this.displayDictionaries).Keys.ToList();
                    }

                    foreach (string key in parsedKeys)
                    {
                        currentPropertyDisplayConditionalFields[key] = conditionalFieldValue;
                    }
                }
            }
        }

        private void ReadPropertyDisplayTransformations()
        {
            string filePath = this.GetDisplayDescriptionFullPath(Constants.DisplayDescriptionFileNames.DisplayTransformationsCSV);

            if (!File.Exists(filePath))
            {
                return;
            }

            using (PIDLConfigParser parser = new PIDLConfigParser(
                filePath,
                new ColumnDefinition[]
                {
                    new ColumnDefinition("PropertyHintId",     ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds",         ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Target",             ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Category",           ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("InputRegex",         ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("ReplacementPattern", ColumnConstraint.Optional, ColumnFormat.AlphaNumeric)
                },
                true))
            {
                if (this.displayTransformations == null)
                {
                    this.displayTransformations = new Dictionary<string, Dictionary<string, Dictionary<string, DisplayTransformation>>>(StringComparer.CurrentCultureIgnoreCase);
                }

                Dictionary<string, Dictionary<string, DisplayTransformation>> currentPropertyHintDisplayTransformations = null;

                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    string propertyHintId = cells[DisplayTransformationsCellIndexDescription.PropertyHintId];
                    string countryConfig = cells[DisplayTransformationsCellIndexDescription.CountryIds];
                    string transformTarget = cells[DisplayTransformationsCellIndexDescription.Target];
                    string transformCategory = cells[DisplayTransformationsCellIndexDescription.Category];
                    string inputRegex = string.IsNullOrEmpty(cells[DisplayTransformationsCellIndexDescription.InputRegex]) ? null
                    : cells[DisplayTransformationsCellIndexDescription.InputRegex];
                    string replacementPattern = string.IsNullOrEmpty(cells[DisplayTransformationsCellIndexDescription.ReplacementPattern]) ? null
                    : cells[DisplayTransformationsCellIndexDescription.ReplacementPattern];

                    if (!DisplayTransformation.IsValidTransformationTarget(transformTarget))
                    {
                        throw new PIDLConfigException(
                            filePath,
                            parser.LineNumber,
                            string.Format("Invalid display transformation target."),
                            Constants.ErrorCodes.PIDLConfigInvalidTransformationTarget);
                    }
                    else if (!DisplayTransformation.IsValidTransformationCategory(transformCategory))
                    {
                        throw new PIDLConfigException(
                            filePath,
                            parser.LineNumber,
                            string.Format("Invalid display transformation category."),
                            Constants.ErrorCodes.PIDLConfigInvalidTransformationCategory);
                    }

                    DisplayTransformation newDisplayTransformation = new DisplayTransformation()
                    {
                        TransformCategory = transformCategory,
                        InputRegex = inputRegex,
                        ReplacementPattern = replacementPattern
                    };

                    if (string.IsNullOrWhiteSpace(propertyHintId))
                    {
                        if (currentPropertyHintDisplayTransformations == null)
                        {
                            throw new PIDLConfigException(
                                filePath,
                                parser.LineNumber,
                                string.Format("PropertyHintId is missing."),
                                Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                        }
                    }
                    else
                    {
                        if (!this.displayTransformations.TryGetValue(propertyHintId, out currentPropertyHintDisplayTransformations))
                        {
                            currentPropertyHintDisplayTransformations = new Dictionary<string, Dictionary<string, DisplayTransformation>>(StringComparer.CurrentCultureIgnoreCase);
                            this.displayTransformations[propertyHintId] = currentPropertyHintDisplayTransformations;
                        }
                    }

                    List<string> parsedCountries = null;
                    if (string.IsNullOrEmpty(countryConfig))
                    {
                        parsedCountries = new List<string>() { string.Empty };
                    }
                    else
                    {
                        parsedCountries = GetDictionaryFromConfigString(countryConfig, this.displayDictionaries).Keys.ToList();
                    }

                    foreach (string countryId in parsedCountries)
                    {
                        if (!currentPropertyHintDisplayTransformations.ContainsKey(countryId))
                        {
                            currentPropertyHintDisplayTransformations[countryId] = new Dictionary<string, DisplayTransformation>(StringComparer.CurrentCultureIgnoreCase);
                        }

                        currentPropertyHintDisplayTransformations[countryId][transformTarget] = newDisplayTransformation;
                    }
                }
            }
        }

        private void ReadDisplayStringSequencesConfig()
        {
            string filePath = this.GetDisplayDescriptionFullPath(Constants.DisplayDescriptionFileNames.DisplayStringSequencesCSV);

            if (!File.Exists(filePath))
            {
                return;
            }

            using (PIDLConfigParser dictionaryParser = new PIDLConfigParser(
                filePath,
                new[]
                {
                    new ColumnDefinition("DisplayStringSequenceId",        ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds",                     ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DisplayStringId",                ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                },
                true))
            {
                Dictionary<string, List<string>> currentDictionary = null;

                if (this.displayStringSequences == null)
                {
                    this.displayStringSequences = new Dictionary<string, Dictionary<string, List<string>>>();
                }

                while (!dictionaryParser.EndOfData)
                {
                    string[] cells = dictionaryParser.ReadValidatedFields();

                    string displayStringSequenceId = cells[DisplayStringSequenceCellIndexDescription.DisplayStringSequenceId];
                    string countryConfig = cells[DisplayStringSequenceCellIndexDescription.CountryId];
                    string displayStringId = cells[DisplayStringSequenceCellIndexDescription.DisplayStringId];

                    if (string.IsNullOrWhiteSpace(displayStringSequenceId))
                    {
                        if (currentDictionary == null)
                        {
                            throw new PIDLConfigException(
                                filePath,
                                dictionaryParser.LineNumber,
                                string.Format("Name of the first group DisplayStringSequenceId is missing."),
                                Constants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
                        }
                    }
                    else
                    {
                        if (!this.displayStringSequences.TryGetValue(displayStringSequenceId, out currentDictionary))
                        {
                            currentDictionary = new Dictionary<string, List<string>>(StringComparer.CurrentCultureIgnoreCase);
                            this.displayStringSequences[displayStringSequenceId] = currentDictionary;
                        }
                    }

                    List<string> parsedCountries = null;
                    if (string.IsNullOrEmpty(countryConfig))
                    {
                        parsedCountries = new List<string>() { string.Empty };
                    }
                    else
                    {
                        parsedCountries = GetDictionaryFromConfigString(countryConfig, this.displayDictionaries).Keys.ToList();
                    }

                    foreach (string countryId in parsedCountries)
                    {
                        if (!currentDictionary.ContainsKey(countryId))
                        {
                            currentDictionary[countryId] = new List<string>();
                        }

                        currentDictionary[countryId].Add(displayStringId);
                    }

                    string errorMessage;

                    if (!PidlFactoryHelper.ValidatePidlSequenceId(displayStringId, out errorMessage))
                    {
                        throw new PIDLConfigException(
                            filePath,
                            dictionaryParser.LineNumber,
                            errorMessage,
                            Constants.ErrorCodes.PIDLConfigPIDLResourceIdIsMalformed);
                    }
                }
            }
        }

        private void ReadDisplayStringsConfig()
        {
            string filePath = this.GetDisplayDescriptionFullPath(Constants.DisplayDescriptionFileNames.DisplayStringsCSV);

            if (!File.Exists(filePath))
            {
                return;
            }

            using (PIDLConfigParser parser = new PIDLConfigParser(
                filePath,
                new ColumnDefinition[]
                {
                    new ColumnDefinition("DisplayStringId",         ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("CountryIds",              ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Type",                    ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Code",                    ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Value",                   ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Target",                  ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                },
                true))
            {
                Dictionary<string, Dictionary<string, DisplayStringMap>> tempDisplayStringMap = new Dictionary<string, Dictionary<string, DisplayStringMap>>(StringComparer.CurrentCultureIgnoreCase);
                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();

                    string id = cells[DisplayStringMapCellIndexDescription.DisplayStringId];
                    string countryConfig = cells[DisplayStringMapCellIndexDescription.CountryIds];
                    string type = cells[DisplayStringMapCellIndexDescription.Type];
                    string code = cells[DisplayStringMapCellIndexDescription.Code];
                    string value = cells[DisplayStringMapCellIndexDescription.Value];
                    string target = cells[DisplayStringMapCellIndexDescription.Target];

                    DisplayStringMap newDisplayStringMap = new DisplayStringMap()
                    {
                        DisplayStringId = id,
                        DisplayStringCode = code,
                        DisplayStringValue = value,
                        DisplayStringTarget = target
                    };

                    DisplayStringType displayStringType;

                    if (!Enum.TryParse<DisplayStringType>(type.ToLower(), out displayStringType))
                    {
                        throw new PIDLConfigException(
                            filePath,
                            parser.LineNumber,
                            string.Format("Column {0} could not be parsed.  Allowed strings in this column are constant or errorcode.", 3),
                            Constants.ErrorCodes.PIDLConfigFileColumnIsMalformed);
                    }

                    newDisplayStringMap.DisplayStringType = displayStringType;

                    if (!tempDisplayStringMap.ContainsKey(id))
                    {
                        tempDisplayStringMap[id] = new Dictionary<string, DisplayStringMap>(StringComparer.CurrentCultureIgnoreCase);
                    }

                    if (string.IsNullOrWhiteSpace(countryConfig))
                    {
                        tempDisplayStringMap[id][string.Empty] = newDisplayStringMap;
                    }
                    else
                    {
                        Dictionary<string, string[]> parsedCountries = GetDictionaryFromConfigString(countryConfig, this.displayDictionaries);
                        foreach (string countryId in parsedCountries.Keys)
                        {
                            tempDisplayStringMap[id][countryId] = newDisplayStringMap;
                        }
                    }
                }

                this.displayStringMap = tempDisplayStringMap;
            }
        }

        private static class ContainerCellIndexDescription
        {
            public const int ContainerHintId = 0;
            public const int CountryId = 1;
            public const int DisplaySequenceId = 2;
            public const int DisplayType = 3;
            public const int DisplayName = 4;
            public const int LayoutOrientation = 5;
            public const int LayoutAlignment = 6;
            public const int ShowDisplayName = 7;
            public const int IsSubmitGroup = 8;
            public const int Extend = 9;
            public const int FirstButtonGroup = 10;
            public const int ExtendButtonGroup = 11;
            public const int ContainerDescription = 12;
            public const int DisplayConditionFunctionName = 13;
            public const int DataCollectionSource = 14;
            public const int DataCollectionFilterFunctionName = 15;
            public const int StyleHints = 16;
        }

        private static class DisplaySequenceCellIndexDescription
        {
            public const int DisplaySequenceId = 0;
            public const int CountryId = 1;
            public const int FlightName = 2;
            public const int HintId = 3;
        }

        private static class DisplayStringSequenceCellIndexDescription
        {
            public const int DisplayStringSequenceId = 0;
            public const int CountryId = 1;
            public const int DisplayStringId = 2;
        }

        private static class PidlSequenceCellIndexDescription
        {
            public const int PIDLResourceType = 0;
            public const int PIDLResourceIdentity = 1;
            public const int Operation = 2;
            public const int CountryIds = 3;
            public const int Scenario = 4;
            public const int DisplaySequenceId = 5;
            public const int DisplayStringSequenceId = 6;
        }

        private static class PropertyErrorRegexCellIndexDescription
        {
            public const int PropertyHintId = 0;
            public const int CountryIds = 1;
            public const int MessageSource = 2;
            public const int DefaultErrorMessage = 3;
            public const int ErrorRegEx = 4;
            public const int ErrorCode = 5;
            public const int ErrorMessage = 6;
        }

        private static class PropertyDisplayDescriptionTagsCellIndexDescription
        {
            public const int PropertyHintId = 0;
            public const int CountryIds = 1;
            public const int TagKey = 2;
            public const int TagValue = 3;
        }

        private static class PropertyDisplayDescriptionConditionalFieldsCellIndexDescription
        {
            public const int PropertyHintId = 0;
            public const int ConditionalFieldKey = 1;
            public const int ConditionalFieldValue = 2;
        }

        private static class DisplayStringMapCellIndexDescription
        {
            public const int DisplayStringId = 0;
            public const int CountryIds = 1;
            public const int Type = 2;
            public const int Code = 3;
            public const int Value = 4;
            public const int Target = 5;
        }

        private static class DisplayTransformationsCellIndexDescription
        {
            public const int PropertyHintId = 0;
            public const int CountryIds = 1;
            public const int Target = 2;
            public const int Category = 3;
            public const int InputRegex = 4;
            public const int ReplacementPattern = 5;
        }
    }
}