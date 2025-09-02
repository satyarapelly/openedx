// <copyright file="PIDLResourceConfig.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Instances of this class describes configuration of one InfoDescription.  Each instance
    /// of this class represents a row in the InfoDescriptions.csv
    /// </summary>
    public class PIDLResourceConfig
    {
        private List<string[]> dataDescriptionConfig;
        
        public PIDLResourceConfig()
        {
            this.dataDescriptionConfig = new List<string[]>();
        }

        public List<string[]> DataDescriptionConfig
        {
            get
            {
                return this.dataDescriptionConfig;
            }
        }

        public static void ReadFromConfig(string filePath, out Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, PIDLResourceConfig>>>>> pidlConfigs)
        {
            using (PIDLConfigParser parser = new PIDLConfigParser(
                filePath,
                new ColumnDefinition[]
                {
                    new ColumnDefinition("PIDLResourceType",        ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("PIDLResourceIdentity",    ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Operation",               ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("CountryIds",              ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("PropertyName",            ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("PropertyDescriptionId",   ColumnConstraint.Optional, ColumnFormat.AlphaNumeric)
                },
                true))
            {
                pidlConfigs = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, PIDLResourceConfig>>>>>(StringComparer.CurrentCultureIgnoreCase);
                PIDLResourceConfig currentPidlResourceConfig = null;
                string currentIdString = string.Empty;
                string currentPidlResourcetype = string.Empty;
                string currentOperationType = string.Empty;
                string currentCountryConfig = string.Empty;

                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();
                    string pidlResourceType = cells[PidlResourceCellIndexDescription.PIDLResourceType];
                    string idString = string.IsNullOrWhiteSpace(cells[PidlResourceCellIndexDescription.PIDLResourceIdentity]) ? string.Empty 
                        : cells[PidlResourceCellIndexDescription.PIDLResourceIdentity];
                    string operation = string.IsNullOrWhiteSpace(cells[PidlResourceCellIndexDescription.Operation]) ? string.Empty 
                        : cells[PidlResourceCellIndexDescription.Operation];
                    string countryConfig = string.IsNullOrWhiteSpace(cells[PidlResourceCellIndexDescription.CountryIds]) ? string.Empty
                        : cells[PidlResourceCellIndexDescription.CountryIds];
                    string propertyName = string.IsNullOrWhiteSpace(cells[PidlResourceCellIndexDescription.PropertyName]) ? string.Empty
                        : cells[PidlResourceCellIndexDescription.PropertyName];
                    string propertyDescriptionId = cells[PidlResourceCellIndexDescription.PropertyDescriptionId];

                    if (!string.Equals(currentPidlResourcetype, pidlResourceType, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(currentIdString, idString, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(currentCountryConfig, countryConfig, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(currentOperationType, operation, StringComparison.OrdinalIgnoreCase))
                    {
                        currentPidlResourcetype = pidlResourceType;
                        currentIdString = idString;
                        currentOperationType = operation;
                        currentCountryConfig = countryConfig;

                        currentPidlResourceConfig = new PIDLResourceConfig();

                        PidlFactoryHelper.ResolvePidlResourceIdentity<PIDLResourceConfig>(
                            pidlConfigs,
                            currentPidlResourcetype,
                            currentIdString,
                            currentOperationType,
                            currentCountryConfig,
                            null,
                            () => 
                            {
                                return currentPidlResourceConfig;
                            });
                    }

                    currentPidlResourceConfig.DataDescriptionConfig.Add(new string[] 
                                {
                                    propertyName,
                                    propertyDescriptionId
                                });

                    string errorMessage;

                    if (!PidlFactoryHelper.ValidatePidlSequenceId(propertyDescriptionId, out errorMessage))
                    {
                        throw new PIDLConfigException(
                            filePath,
                            parser.LineNumber,
                            errorMessage,
                            Constants.ErrorCodes.PIDLConfigPIDLResourceIdIsMalformed);
                    }
                }
            }
        }

        private static class PidlResourceCellIndexDescription
        {
            public const int PIDLResourceType = 0;
            public const int PIDLResourceIdentity = 1;
            public const int Operation = 2;
            public const int CountryIds = 3;
            public const int PropertyName = 4;
            public const int PropertyDescriptionId = 5;
        }
    }
}