// <copyright file="KnownDiffsConfig.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace PidlTest.Diff
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using JsonDiff;
    using Microsoft.Commerce.Payments.PXService.ApiSurface.Diff;

    /// <summary>
    /// Generate a collection of differences that will be used to modify the baseline to match new changes
    /// </summary>
    public class KnownDiffsConfig
    {
        private Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>>>>> allKnownDiffsByPidlIdentity;
        private string diffsFilePath;

        public KnownDiffsConfig(string diffFilePath)
        {
            this.diffsFilePath = diffFilePath;
        }

        /// <summary>
        /// Verify that a file of differences exists before reading data
        /// </summary>
        public void Initialize()
        {
            this.allKnownDiffsByPidlIdentity = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>>>>>(StringComparer.CurrentCultureIgnoreCase);

            if (!File.Exists(this.diffsFilePath))
            {
                throw new PidlConfigException(
                    string.Format("Diff file \"{0}\" does not exist.", this.diffsFilePath));
            }

            this.ReadFromConfig(this.diffsFilePath);
        }        

        /// <summary>
        /// Returns a list of known differences for a given identity path
        /// </summary>
        /// <param name="identity">parameters that identify the PXService request</param>
        /// <returns>list of known diffs</returns>
        public List<KnownDiffsDescription> GetDiffConfig(PidlIdentity identity)
        {
            List<KnownDiffsDescription> knownDiffs = new List<KnownDiffsDescription>();

            List<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>>>>> diffset0 = new List<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>>>>>();
            List<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>>>> diffset1 = new List<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>>>>();
            List<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>>> diffset2 = new List<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>>>();
            List<Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>> diffset3 = new List<Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>>();
            List<Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>> diffset4 = new List<Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>();
            List<Dictionary<string, List<KnownDiffsDescription>>> diffset5 = new List<Dictionary<string, List<KnownDiffsDescription>>>();

            if (this.allKnownDiffsByPidlIdentity.ContainsKey(Constants.DiffTest.Any))
            {
                diffset0.Add(this.allKnownDiffsByPidlIdentity[Constants.DiffTest.Any]);
            }

            if (this.allKnownDiffsByPidlIdentity.ContainsKey(identity.ResourceName))
            {
                diffset0.Add(this.allKnownDiffsByPidlIdentity[identity.ResourceName]);
            }

            foreach (Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>>>> dict0 in diffset0)
            {
                if (dict0.ContainsKey(Constants.DiffTest.Any))
                {
                    diffset1.Add(dict0[Constants.DiffTest.Any]);
                }

                if (dict0.ContainsKey(identity.Id))
                {
                    diffset1.Add(dict0[identity.Id]);
                }

                foreach (Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>>> dict1 in diffset1)
                {
                    if (dict1.ContainsKey(Constants.DiffTest.Any))
                    {
                        diffset2.Add(dict1[Constants.DiffTest.Any]);
                    }

                    if (dict1.ContainsKey(identity.Country))
                    {
                        diffset2.Add(dict1[identity.Country]);
                    }

                    foreach (Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>> dict2 in diffset2)
                    {
                        if (dict2.ContainsKey(Constants.DiffTest.Any))
                        {
                            diffset3.Add(dict2[Constants.DiffTest.Any]);
                        }

                        if (dict2.ContainsKey(identity.Language))
                        {
                            diffset3.Add(dict2[identity.Language]);
                        }

                        foreach (Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>> dict3 in diffset3)
                        {
                            if (dict3.ContainsKey(Constants.DiffTest.Any))
                            {
                                diffset4.Add(dict3[Constants.DiffTest.Any]);
                            }

                            if (dict3.ContainsKey(identity.Partner))
                            {
                                diffset4.Add(dict3[identity.Partner]);
                            }

                            foreach (Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>> dict4 in diffset4)
                            {
                                if (dict4.ContainsKey(Constants.DiffTest.Any))
                                {
                                    diffset5.Add(dict4[Constants.DiffTest.Any]);
                                }

                                if (dict4.ContainsKey(identity.Operation))
                                {
                                    diffset5.Add(dict4[identity.Operation]);
                                }

                                foreach (Dictionary<string, List<KnownDiffsDescription>> dict5 in diffset5)
                                {
                                    if (dict5.ContainsKey(Constants.DiffTest.Any))
                                    {
                                        foreach (KnownDiffsDescription diffDescription in dict5[Constants.DiffTest.Any])
                                        {
                                            if (!knownDiffs.Contains(diffDescription))
                                            {
                                                knownDiffs.Add(diffDescription);
                                            }
                                        }
                                    }

                                    if (dict5.ContainsKey(identity.Operation))
                                    {
                                        foreach (KnownDiffsDescription diffDescription in dict5[identity.Operation])
                                        {
                                            if (!knownDiffs.Contains(diffDescription))
                                            {
                                                knownDiffs.Add(diffDescription);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return knownDiffs;
        }

        /// <summary>
        /// Populate a collection with a given identity path
        /// </summary>
        /// <param name="resourceDictionary">known diff node tree</param>
        /// <param name="identity">parameters that identify the PXService request</param>
        /// <param name="diffDescriptions">list of known diffs</param>
        private static void PopulateResourceIdentity(
            Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>>>>> resourceDictionary,
            PidlIdentity identity,
            KnownDiffsDescription diffDescriptions)
        {
            identity.ResourceName = string.IsNullOrWhiteSpace(identity.ResourceName) ? Constants.DiffTest.Any : identity.ResourceName;
            if (!resourceDictionary.ContainsKey(identity.ResourceName))
            {
                resourceDictionary[identity.ResourceName] = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>>>>(StringComparer.CurrentCultureIgnoreCase);
            }

            identity.Id = string.IsNullOrWhiteSpace(identity.Id) ? Constants.DiffTest.Any : identity.Id;
            if (!resourceDictionary[identity.ResourceName].ContainsKey(identity.Id))
            {
                resourceDictionary[identity.ResourceName][identity.Id] = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>>>(StringComparer.CurrentCultureIgnoreCase);
            }

            identity.Country = string.IsNullOrWhiteSpace(identity.Country) ? Constants.DiffTest.Any : identity.Country;
            if (!resourceDictionary[identity.ResourceName][identity.Id].ContainsKey(identity.Country))
            {
                resourceDictionary[identity.ResourceName][identity.Id][identity.Country] = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>>(StringComparer.CurrentCultureIgnoreCase);
            }

            identity.Language = string.IsNullOrWhiteSpace(identity.Language) ? Constants.DiffTest.Any : identity.Language;
            if (!resourceDictionary[identity.ResourceName][identity.Id][identity.Country].ContainsKey(identity.Language))
            {
                resourceDictionary[identity.ResourceName][identity.Id][identity.Country][identity.Language] = new Dictionary<string, Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>>(StringComparer.CurrentCultureIgnoreCase);
            }

            identity.Partner = string.IsNullOrWhiteSpace(identity.Partner) ? Constants.DiffTest.Any : identity.Partner;
            if (!resourceDictionary[identity.ResourceName][identity.Id][identity.Country][identity.Language].ContainsKey(identity.Partner))
            {
                resourceDictionary[identity.ResourceName][identity.Id][identity.Country][identity.Language][identity.Partner] = new Dictionary<string, Dictionary<string, List<KnownDiffsDescription>>>(StringComparer.CurrentCultureIgnoreCase);
            }

            identity.Operation = string.IsNullOrWhiteSpace(identity.Operation) ? Constants.DiffTest.Any : identity.Operation;
            if (!resourceDictionary[identity.ResourceName][identity.Id][identity.Country][identity.Language][identity.Partner].ContainsKey(identity.Operation))
            {
                resourceDictionary[identity.ResourceName][identity.Id][identity.Country][identity.Language][identity.Partner][identity.Operation] = new Dictionary<string, List<KnownDiffsDescription>>(StringComparer.CurrentCultureIgnoreCase);
            }

            identity.Scenario = string.IsNullOrWhiteSpace(identity.Scenario) ? Constants.DiffTest.Any : identity.Scenario;
            if (!resourceDictionary[identity.ResourceName][identity.Id][identity.Country][identity.Language][identity.Partner][identity.Operation].ContainsKey(identity.Scenario))
            {
                resourceDictionary[identity.ResourceName][identity.Id][identity.Country][identity.Language][identity.Partner][identity.Operation][identity.Scenario] = new List<KnownDiffsDescription>();
            }

            resourceDictionary[identity.ResourceName][identity.Id][identity.Country][identity.Language][identity.Partner][identity.Operation][identity.Scenario].Add(diffDescriptions);
        }

        /// <summary>
        /// Populate the allKnownDiffsByPidlIdentity dictionary tree with data
        /// Gets parsed differences
        /// for each row of data do the following:
        /// Validate all differences exists
        /// Populate the collection with an identity path, if one does not exists.
        /// Add diff to the identity collection
        /// </summary>
        /// <param name="filePath">Location to the known diffs file</param>
        private void ReadFromConfig(string filePath)
        {
            ColumnDefinition[] columns = new ColumnDefinition[]
            {
                new ColumnDefinition("PIDLResourceType",        ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                new ColumnDefinition("ID",                      ColumnConstraint.Optional, ColumnFormat.Text),
                new ColumnDefinition("Country",                 ColumnConstraint.Optional, ColumnFormat.Text),
                new ColumnDefinition("Language",                ColumnConstraint.Optional, ColumnFormat.Text),
                new ColumnDefinition("Partner",                 ColumnConstraint.Optional, ColumnFormat.Text),
                new ColumnDefinition("Operation",               ColumnConstraint.Optional, ColumnFormat.Text),
                new ColumnDefinition("Scenario",                ColumnConstraint.Optional, ColumnFormat.Text),
                new ColumnDefinition("DeltaType",               ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                new ColumnDefinition("BaselineJpath",           ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                new ColumnDefinition("NewJPath",                ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                new ColumnDefinition("NewValue",                ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
            };

            PidlConfigParser parser = new PidlConfigParser(filePath, columns, true);
            List<KnownDiffsDescription> allKnownDiffsForCurrentPidlIdentity = null;
            while (!parser.EndOfData)
            {
                string[] cells = parser.ReadValidatedFields();
                string resourceName = string.IsNullOrWhiteSpace(cells[Constants.DiffTest.DiffCellIndexDescription.ResourceName]) ? string.Empty :
                    cells[Constants.DiffTest.DiffCellIndexDescription.ResourceName];
                string id = string.IsNullOrWhiteSpace(cells[Constants.DiffTest.DiffCellIndexDescription.ID]) ? string.Empty :
                    cells[Constants.DiffTest.DiffCellIndexDescription.ID];
                string country = string.IsNullOrWhiteSpace(cells[Constants.DiffTest.DiffCellIndexDescription.Country]) ? string.Empty :
                    cells[Constants.DiffTest.DiffCellIndexDescription.Country];
                string language = string.IsNullOrWhiteSpace(cells[Constants.DiffTest.DiffCellIndexDescription.Language]) ? string.Empty :
                        cells[Constants.DiffTest.DiffCellIndexDescription.Language];
                string partner = string.IsNullOrWhiteSpace(cells[Constants.DiffTest.DiffCellIndexDescription.Partner]) ? string.Empty :
                    cells[Constants.DiffTest.DiffCellIndexDescription.Partner];
                string operation = string.IsNullOrWhiteSpace(cells[Constants.DiffTest.DiffCellIndexDescription.Operation]) ? string.Empty :
                    cells[Constants.DiffTest.DiffCellIndexDescription.Operation];
                string scenario = string.IsNullOrWhiteSpace(cells[Constants.DiffTest.DiffCellIndexDescription.Scenario]) ? string.Empty :
                    cells[Constants.DiffTest.DiffCellIndexDescription.Scenario];
                string deltaType = string.IsNullOrWhiteSpace(cells[Constants.DiffTest.DiffCellIndexDescription.DeltaType]) ? string.Empty
                    : cells[Constants.DiffTest.DiffCellIndexDescription.DeltaType];
                string baselineJPath = string.IsNullOrWhiteSpace(cells[Constants.DiffTest.DiffCellIndexDescription.BaselineJPath]) ? string.Empty
                    : cells[Constants.DiffTest.DiffCellIndexDescription.BaselineJPath];
                string newJPath = string.IsNullOrWhiteSpace(cells[Constants.DiffTest.DiffCellIndexDescription.NewJPath]) ? string.Empty
                    : cells[Constants.DiffTest.DiffCellIndexDescription.NewJPath];
                string newValue = string.IsNullOrWhiteSpace(cells[Constants.DiffTest.DiffCellIndexDescription.NewValue]) ? string.Empty
                    : cells[Constants.DiffTest.DiffCellIndexDescription.NewValue];

                // Verify content exists for each DiffOperation
                if (string.Equals(deltaType, DiffType.add.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(newJPath))
                    {
                        throw new PidlConfigException(filePath, parser.LineNumber, "NewJPath not provided for DeltaType add");
                    }
                }

                if (string.Equals(deltaType, DiffType.delete.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(baselineJPath))
                    {
                        throw new PidlConfigException(filePath, parser.LineNumber, "BaselineJpath not provided for DeltaType delete");
                    }
                }

                if (string.Equals(deltaType, DiffType.edit.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(baselineJPath))
                    {
                        throw new PidlConfigException(filePath, parser.LineNumber, "BaselineJpath not provided for DeltaType edit");
                    }

                    if (string.IsNullOrEmpty(newValue))
                    {
                        throw new PidlConfigException(filePath, parser.LineNumber, "NewValue not provided for DeltaType edit");
                    }
                }

                if (string.Equals(deltaType, DiffType.move.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(baselineJPath))
                    {
                        throw new PidlConfigException(filePath, parser.LineNumber, "BaselineJpath not provided for DeltaType move");
                    }

                    if (string.IsNullOrEmpty(newJPath))
                    {
                        throw new PidlConfigException(filePath, parser.LineNumber, "NewJPath not provided for DeltaType move");
                    }
                }

                PidlIdentity identity = new PidlIdentity()
                {
                    ResourceName = resourceName,
                    Id = id,
                    Country = country,
                    Language = language,
                    Operation = operation,
                    Partner = partner,
                    Scenario = scenario
                };

                allKnownDiffsForCurrentPidlIdentity = new List<KnownDiffsDescription>();
                PopulateResourceIdentity(
                    this.allKnownDiffsByPidlIdentity,
                    identity,
                    new KnownDiffsDescription()
                    {
                        DeltaType = deltaType,
                        BaselineJPath = baselineJPath,
                        NewJPath = newJPath,
                        NewValue = newValue
                    });
            }
        }
    }
}