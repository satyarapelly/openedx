// <copyright file="ConfigurationComponentRule.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;

    public class ConfigurationComponentRule
    {
        private static readonly Func<string[], int, bool> SingleRowPredicate = (row, i) => false;

        public ConfigurationComponentRule(Func<string[], int, bool>? isRowInRange = null, Func<string[], bool>? isRowInList = null, Dictionary<string, ConfigurationComponentRule>? subComponentRules = null)
        {
            this.IsRowInRange = isRowInRange ?? ConfigurationComponentRule.SingleRowPredicate;
            this.IsRowInList = isRowInList;
            this.SubComponentRules = subComponentRules ?? new Dictionary<string, ConfigurationComponentRule>();
        }

        // Function parameters: next row, relative current row index (0-based) 
        public Func<string[], int, bool> IsRowInRange { get; }

        public Func<string[], bool>? IsRowInList { get; }

        public bool IsList
        {
            get
            {
                return this.IsRowInList != null;
            }
        }

        public Dictionary<string, ConfigurationComponentRule> SubComponentRules { get; }

        public ConfigurationComponentRule this[string key] => this.SubComponentRules[key];

        public ParsedConfigurationComponent Parse(List<string[]> rows, int first, int last)
        {
            ParsedConfigurationComponent parsedComponent = new ParsedConfigurationComponent(new Tuple<int, int>(first, last));

            foreach (KeyValuePair<string, ConfigurationComponentRule> subRule in this.SubComponentRules)
            {
                parsedComponent.SubComponents[subRule.Key] = new List<ParsedConfigurationComponent>();

                int currentRow = first;

                do
                {
                    int currentStartingRow = currentRow;

                    while (currentRow < last && currentRow + 1 < rows.Count && subRule.Value.IsRowInRange(rows[currentRow + 1], currentRow - currentStartingRow))
                    {
                        currentRow++;
                    }

                    parsedComponent.SubComponents[subRule.Key].Add(subRule.Value.Parse(rows, currentStartingRow, currentRow));

                    currentRow++;
                }
                while (subRule.Value.IsList && currentRow <= last && subRule.Value.IsRowInList(rows[currentRow]));
            }

            return parsedComponent;
        }
    }
}