// <copyright file="DataSourcesConfig.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Instances of this class describes configuration of one DataSource. Each instance
    /// of this class represents a row in the DataSources.csv
    /// </summary>
    public class DataSourcesConfig
    {
        private List<string[]> config;
        private Dictionary<string, string> headers;

        public DataSourcesConfig()
        {
            this.config = new List<string[]>();
            this.headers = new Dictionary<string, string>();
        }

        public List<string[]> Config
        {
            get
            {
                return this.config;
            }
        }

        public Dictionary<string, string> Headers
        {
            get
            {
                return this.headers;
            }
        }

        public static void ReadFromConfig(string filePath, out Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, DataSourcesConfig>>>>> dataSourcesConfigs)
        {
            using (PIDLConfigParser parser = new PIDLConfigParser(
                filePath,
                new ColumnDefinition[]
                {
                    new ColumnDefinition("PIDLResourceType",        ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("PIDLResourceIdentity",    ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Operation",               ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("CountryIds",              ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("DataSourceName",          ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Href",                    ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Method",                  ColumnConstraint.Required, ColumnFormat.Text),
                    new ColumnDefinition("Headers",                 ColumnConstraint.Optional, ColumnFormat.Text),
                },
                true))
            {
                dataSourcesConfigs = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, DataSourcesConfig>>>>>(StringComparer.CurrentCultureIgnoreCase);
                DataSourcesConfig currentDataSourcesConfig = null;
                string currentIdString = string.Empty;
                string currentPidlResourcetype = string.Empty;
                string currentOperationType = string.Empty;
                string currentCountryConfig = string.Empty;

                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();
                    string pidlResourceType = cells[DataSourcesCellIndexDescription.PIDLResourceType];
                    string idString = string.IsNullOrWhiteSpace(cells[DataSourcesCellIndexDescription.PIDLResourceIdentity]) ? string.Empty
                        : cells[DataSourcesCellIndexDescription.PIDLResourceIdentity];
                    string operation = string.IsNullOrWhiteSpace(cells[DataSourcesCellIndexDescription.Operation]) ? string.Empty
                        : cells[DataSourcesCellIndexDescription.Operation];
                    string countryConfig = string.IsNullOrWhiteSpace(cells[DataSourcesCellIndexDescription.CountryIds]) ? string.Empty
                        : cells[DataSourcesCellIndexDescription.CountryIds];
                    string dataSourceName = cells[DataSourcesCellIndexDescription.DataSourceName];
                    string href = cells[DataSourcesCellIndexDescription.Href];
                    string method = cells[DataSourcesCellIndexDescription.Method];
                    string headers = cells[DataSourcesCellIndexDescription.Headers];

                    if (!string.Equals(currentPidlResourcetype, pidlResourceType, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(currentIdString, idString, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(currentCountryConfig, countryConfig, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(currentOperationType, operation, StringComparison.OrdinalIgnoreCase))
                    {
                        currentPidlResourcetype = pidlResourceType;
                        currentIdString = idString;
                        currentOperationType = operation;
                        currentCountryConfig = countryConfig;

                        currentDataSourcesConfig = new DataSourcesConfig();

                        PidlFactoryHelper.ResolvePidlResourceIdentity<DataSourcesConfig>(
                            dataSourcesConfigs,
                            currentPidlResourcetype,
                            currentIdString,
                            currentOperationType,
                            currentCountryConfig,
                            null,
                            () =>
                            {
                                return currentDataSourcesConfig;
                            });
                    }

                    currentDataSourcesConfig.Config.Add(new string[]
                        {
                            dataSourceName,
                            href,
                            method
                        });

                    // Add extra headers
                    // ex: "apiversion=2015-03-31;x-ms-correlation-id" will be translated to "headers":{"apiversion":"2015-03-31","x-ms-correlation-id":""}
                    // Task 19251234: PxService Engineering improvement: revisit the design for DataSource class
                    if (!string.IsNullOrEmpty(headers))
                    {
                        string[] headerArray = headers.Split(';');
                        foreach (string headerEntry in headerArray)
                        {
                            string[] headerContent = headerEntry.Split(':');
                            if (!string.IsNullOrEmpty(headerContent[0]))
                            {
                                currentDataSourcesConfig.Headers.Add(headerContent[0], headerContent.Length > 1 ? headerContent[1] : string.Empty);
                            } 
                        }
                    }
                }
            }
        }

        private static class DataSourcesCellIndexDescription
        {
            public const int PIDLResourceType = 0;
            public const int PIDLResourceIdentity = 1;
            public const int Operation = 2;
            public const int CountryIds = 3;
            public const int DataSourceName = 4;
            public const int Href = 5;
            public const int Method = 6;
            public const int Headers = 7;
        }
    }
}