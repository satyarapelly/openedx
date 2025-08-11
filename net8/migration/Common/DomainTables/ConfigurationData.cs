// <copyright file="ConfigurationData.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.DomainTables
{
    using System.Collections.Generic;
    using System.Text;
    using Newtonsoft.Json;

    [JsonObject]
    public class ConfigurationData
    {
        public ConfigurationData()
        {
            this.Header = new List<string>();
            this.AntecedentColumnMetadata = new List<ColumnDefinition>();
            this.ConsequentColumnMetadata = new List<ColumnDefinition>();
            this.Rows = new List<IDictionary<string, string>>();
            this.Validators = new Dictionary<string, string>();
        }

        [JsonProperty("header")]
        public IList<string> Header { get; private set; }

        [JsonProperty("antecedentColumnMetadata")]
        public IList<ColumnDefinition> AntecedentColumnMetadata { get; private set; }

        [JsonProperty("consequentColumnMetadata")]
        public IList<ColumnDefinition> ConsequentColumnMetadata { get; private set; }

        [JsonProperty("rows")]
        public IList<IDictionary<string, string>> Rows { get; private set; }

        [JsonProperty("validators")]
        public IDictionary<string, string> Validators { get; private set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        /// <summary>
        /// Uses the ColumnMetadata of the instance to populate the Validators property with regexes that can check the correctness of entries for each column
        /// This is the only way the validators of this object should be set
        /// </summary>
        public void SetValidators()
        {
            List<ColumnDefinition> allColumns = new List<ColumnDefinition>(this.AntecedentColumnMetadata);
            allColumns.AddRange(this.ConsequentColumnMetadata);

            foreach (ColumnDefinition def in allColumns)
            {
                StringBuilder regex = new StringBuilder();
                regex.Append("^");
                switch (def.Format)
                {
                    case ColumnFormat.AlphaNumeric:
                        regex.Append("[\\w\\s-]");
                        break;
                    case ColumnFormat.Date:
                        regex.Append("(\\d{1,2}/\\d{1,2}/\\d{4})");
                        break;
                    case ColumnFormat.Number:
                        regex.Append("\\-?\\d");
                        break;
                    case ColumnFormat.Text:
                        regex.Append(".");
                        break;
                    case ColumnFormat.NumericalString:
                        regex.Append("(?!(\\d*\\.?\\d+([eE][-+]?\\d+))).");
                        break;
                }

                if (def.Format == ColumnFormat.Date)
                {
                    if (def.Constraint == ColumnConstraint.Optional)
                    {
                        regex.Append("?");
                    }
                }
                else
                {
                    if (def.Constraint == ColumnConstraint.Optional)
                    {
                        if (def.Size == 0)
                        {
                            regex.Append("*");
                        }
                        else
                        {
                            regex.Append("{0," + def.Size.ToString() + "}");
                        }
                    }
                    else
                    {
                        if (def.Size == 0)
                        {
                            regex.Append("+");
                        }
                        else
                        {
                            regex.Append("{1," + def.Size.ToString() + "{");
                        }
                    }
                }

                regex.Append("$");
                this.Validators.Add(def.Name, regex.ToString());
            }
        }
    }
}
