// <copyright file="ConfigurationParser.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class ConfigurationParser<T> where T : ConfigurationObject
    {
        public ConfigurationParser()
        {
            this.ColumnNames = new Dictionary<string, int>();
        }

        public ConfigurationComponentRule Component { get; set; }

        public Dictionary<string, int> ColumnNames { get; }

        public T Parse(string path) 
        {
            List<string[]> rows = ConfigurationParser<T>.ReadConfigStream(path);

            foreach (string column in rows.First())
            {
                if (this.ColumnNames.ContainsKey(column))
                {
                    throw new InvalidDataException("Configuration must not contain duplicate header column names");
                }

                this.ColumnNames[column] = this.ColumnNames.Count;
            }

            ParsedConfigurationComponent component = this.Component.Parse(rows, 1, rows.Count - 1);

            return ConfigurationObject.ConstructFromConfiguration<T>(rows, component, this.ColumnNames);
        }

        public bool MatchesColumns(string[] row, ResourceLifecycleStateManager.SetOperation op, params string[] columnNames)
        {
            if (columnNames.Length == 0)
            {
                throw new ArgumentException("At least one column name must be given");
            }

            bool doesMatch = false;

            HashSet<int> columnPositions = new HashSet<int>(columnNames.Select(c => this.ColumnNames[c])); 

            switch (op)
            {
                case ResourceLifecycleStateManager.SetOperation.AllEmpty:
                    for (int index = 0; index < row.Length; index++)
                    {
                        if (columnPositions.Contains(index))
                        {
                            if (!string.IsNullOrEmpty(row[index]))
                            {
                                return doesMatch;
                            }
                        }
                    }

                    doesMatch = true;

                    break;
                case ResourceLifecycleStateManager.SetOperation.AllFull:
                    for (int index = 0; index < row.Length; index++)
                    {
                        if (columnPositions.Contains(index))
                        {
                            if (string.IsNullOrEmpty(row[index]))
                            {
                                return doesMatch;
                            }
                        }
                    }

                    doesMatch = true;

                    break;
                case ResourceLifecycleStateManager.SetOperation.SomeEmpty:
                    for (int index = 0; index < row.Length; index++)
                    {
                        if (columnPositions.Contains(index))
                        {
                            if (string.IsNullOrEmpty(row[index]))
                            {
                                doesMatch = true;
                                break;
                            }
                        }
                    }

                    break;
                case ResourceLifecycleStateManager.SetOperation.SomeFull:
                    for (int index = 0; index < row.Length; index++)
                    {
                        if (columnPositions.Contains(index))
                        {
                            if (!string.IsNullOrEmpty(row[index]))
                            {
                                doesMatch = true;
                                break;
                            }
                        }
                    }

                    break;
            }

            return doesMatch;
        }

        private static List<string[]> ReadConfigStream(string path)
        {
            List<string[]> rows = new List<string[]>();

            using (StreamReader sw = new StreamReader(path))
            {
                string line;

                while ((line = sw.ReadLine()) != null)
                {
                    string[] row = line.Split(',');
                    rows.Add(row);
                }
            }

            return rows;
        }
    }
}