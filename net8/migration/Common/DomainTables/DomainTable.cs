// <copyright file="DomainTable.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.DomainTables
{   
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    
    // Domain table is loaded from a file in the csv format (editable in Excel).
    // The arguments in the constructor specify the table schema and rules.
    // Notice how the lexic, syntactic and sematic analysis is all done at load time.
    // It is important to keep this principle to identify data mistakes early.
    // ----------------
    // Schema Definition
    //    AlphaNumeric: The field can have any word character
    //    Number: the field can have only numbers (unsigned integer)
    //    Date: The field has a date of the form dd/mm/yyyy
    //    Size: Can be specified for Alphanumeric and Number columns
    // ----------------
    // Rules Definition
    //    Antecedent: specifies the fields needed to identify the consequent
    //    Consequent: specifies the fields implied by the antecedent
    //    Each column can be
    //        Required: The field has to be provided in each row
    //        Optional: The field can be omitted in each row (wildcard)
    //        Unique: The field is required and cannot be repeated across rows
    // ----------------
    // Algorithm Definition
    //    The default resolver algorithm is DataResolverAlgorithm.RightToLeft. Dynamic programming resolution starting from the last antecendent.
    //    The most specific criteria is assumed to be optimal.
    //    If optimal criteria fails, the wildcard (String.Empty) is tried.
    //    DataResolverAlgorithm.MaxMatchNumber algorithm will pick the max weight result by visiting the whole tree. Exact match calculate weight, wildcard not.
    //    Same weight result will fallback to DataResolverAlgorithm.RightToLeft pick most specific one.
    //    Spec: https://microsoft.sharepoint.com/teams/Commerce_Payments_Logistics/Shared%20Documents/Specifications/Market%20Logistics/1-pager%20-%20MCT%20selection%20logic%20spec.docx
    // ----------------
    // Data Example:
    // Processor,Currency,Market,SOR ,Store,MID
    // FDC      ,EUR     ,      ,    ,     ,M001
    // FDC      ,USD     ,      ,    ,     ,M002
    // FDC      ,USD     ,CA    ,    ,     ,M003
    // FDC      ,USD     ,      ,    ,Xbox ,M004
    // PayPal   ,EUR     ,      ,    ,     ,M005
    // PayPal   ,USD     ,      ,    ,     ,M006
    // PayPal   ,USD     ,CA    ,    ,     ,M007
    // PayPal   ,USD     ,      ,    ,Xbox ,M008
    // PayPal   ,        ,      ,1929,     ,M009
    // ----------------
    // Resolution with DataResolverAlgorithm.MaxMatchNumber:
    // F("FDC", "USD", "US", "1010", "Xbox") -> M004
    // F("FDC", "EUR", "US", "1010", "OneStore") -> M001
    // F("FDC", "USD", "US", "1010", "OneStore") -> M002
    // F("FDC", "USD", "CA", "1010", "OneStore") -> M003
    // F("FDC", "USD", "CA", "1010", "Xbox") -> M004
    // F("PayPal", "USD", "CA", "1010", "OneStore") -> M007
    // F("PayPal", "USD", "CA", "1010", "Xbox") -> M008
    // F("PayPal", "USD", "CA", "1929", "Xbox") -> M008
    // F("PayPal", "USD", "CA", "1929", "OneStore") -> M007
    // F("PayPal", "USD", "US", "1929", "OneStore") -> M009
    // ----------------
    // Resolution without DataResolverAlgorithm.MaxMatchNumber:
    // F("FDC", "USD", "US", "1010", "Xbox") -> M004
    // F("FDC", "EUR", "US", "1010", "OneStore") -> M001
    // F("FDC", "USD", "US", "1010", "OneStore") -> M002
    // F("FDC", "USD", "CA", "1010", "OneStore") -> M003
    // F("FDC", "USD", "CA", "1010", "Xbox") -> M004
    // F("PayPal", "USD", "CA", "1010", "OneStore") -> M007
    // F("PayPal", "USD", "CA", "1010", "Xbox") -> M008
    // F("PayPal", "USD", "CA", "1929", "Xbox") -> M008
    // F("PayPal", "USD", "CA", "1929", "OneStore") -> M009
    // F("PayPal", "USD", "US", "1929", "OneStore") -> M009
    public class DomainTable
    {
        private Tuple<string, Regex>[] rowRegexes; 
        private ColumnDefinition[] antecedentDefinition;
        private ColumnDefinition[] consequentDefinition;
        private string header;
        private Node root;

        public DomainTable(ColumnDefinition[] antecedentColumns, ColumnDefinition[] consequentColumns)
            : this(antecedentColumns, consequentColumns, DataResolverAlgorithms.RightToLeft)
        {
        }

        public DomainTable(ColumnDefinition[] antecedentColumns, ColumnDefinition[] consequentColumns, DataResolverAlgorithms resolverAlgorithm)
        {
            if (antecedentColumns == null)
            {
                throw new ArgumentNullException("antecedentColumns");
            }

            if (consequentColumns == null)
            {
                throw new ArgumentNullException("consequentColumns");
            }

            if (antecedentColumns.Length == 0)
            {
                throw new ArgumentException("antecedentDefinition length cannot be 0");
            }

            if (consequentColumns.Length == 0)
            {
                throw new ArgumentException("consequent length cannot be 0");
            }

            this.DefineSchema(antecedentColumns, consequentColumns);
            this.ResolverAlgorithm = resolverAlgorithm;
        }

        public string DataVersion { get; set; }

        public string SchemaVersion { get; set; }

        public DataResolverAlgorithms ResolverAlgorithm { get; private set; }

        public void Load(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }

            this.Load(new StreamReader(filePath));
        }

        public void Load(StreamReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            Node newRoot = new Node(null, 0);
            using (reader)
            {
                int rowNumber = 1;
                string currentString = reader.ReadLine();
                IDictionary<string, HashSet<string>> uniqueColumnValues = new Dictionary<string, HashSet<string>>();
                while (currentString != null)
                {
                    if (rowNumber == 1)
                    {
                        if (currentString != this.header)
                        {
                            throw new InvalidOperationException(string.Format("Invalid header values: {0}, expected: {1} in: {2}", currentString, this.header, this.GetType().Name));
                        }
                    }
                    else
                    {
                        IDictionary<string, string> matchResult = this.MatchRowString(currentString);
                        if (matchResult.Count <= 1)
                        {
                            throw new InvalidOperationException(string.Format("Invalid format at row: {0} in: {1}", rowNumber, this.GetType().Name));
                        }

                        bool handled = this.SetupNodes(uniqueColumnValues, matchResult, newRoot);

                        if (!handled)
                        {
                            throw new InvalidOperationException(string.Format("Resolution conflict at row: {0} in: {1}", rowNumber, this.GetType().Name));
                        }
                    }

                    currentString = reader.ReadLine();
                    ++rowNumber;
                }
            }

            this.root = newRoot;
        }

        // New Load function loading from PartnerConfigurationHandler storage system instead of local csv files
        public void Load(ConfigurationData config, bool reloadSchema = false)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (reloadSchema)
            {
                if (config.AntecedentColumnMetadata.Count == 0)
                {
                    throw new ArgumentException("antecedentDefinition length cannot be 0");
                }

                if (config.ConsequentColumnMetadata.Count == 0)
                {
                    throw new ArgumentException("consequent length cannot be 0");
                }

                this.DefineSchema(config.AntecedentColumnMetadata.ToArray(), config.ConsequentColumnMetadata.ToArray());
            }

            Node newRoot = new Node(null, 0);

            // Check Correct Header
            if (!this.header.Split(new char[] { ',' }).ToList<string>().SequenceEqual<string>(config.Header))
            {
                throw new InvalidOperationException("Invalid header: " + config.Header);
            }

            IDictionary<string, HashSet<string>> uniqueColumnValues = new Dictionary<string, HashSet<string>>();

            foreach (IDictionary<string, string> row in config.Rows)
            {
                IDictionary<string, string> matchResult = this.MatchRowString(row);
                if (matchResult.Count <= 1)
                {
                    throw new InvalidOperationException(string.Format("Invalid format in configuration file in: {0}", this.GetType().Name));
                }

                bool handled = this.SetupNodes(uniqueColumnValues, matchResult, newRoot);

                if (!handled)
                {
                    throw new InvalidOperationException(string.Format("Resolution conflict in configuration file in: {0}", this.GetType().Name));
                }
            }

            this.root = newRoot;
        }

        public string[] Resolve(params string[] criteria)
        {
            string[] result;

            if (!this.TryResolve(criteria, out result))
            {
                throw new InvalidOperationException(string.Format("Invalid criteria {0}", string.Join(",", criteria)));
            }

            return result;
        }

        public bool TryResolve(string[] criteria, out string[] result)
        {
            if (criteria.Length != this.antecedentDefinition.Length)
            {
                throw new ArgumentException("The number of criteria items has to match the number of antecedent columns.", "criteria");
            }

            for (int i = 0; i < criteria.Length; ++i)
            {
                if (criteria[i] == null)
                {
                    criteria[i] = string.Empty;
                }
            }

            return this.root.TryResolve(criteria, this.ResolverAlgorithm, out result);
        }

        public IEnumerable<string> GetAllKeys()
        {
            if (this.antecedentDefinition.Count() > 1)
            {
                throw new InvalidOperationException();
            }

            return this.root.GetChildrenKeys();
        }

        private static void AppendColumnRegex(List<Tuple<string, Regex>> rowRegexList, ColumnDefinition column)
        {
            // the regex below creates a named group for every match. Once the
            // regex successfully matches a row, we lookup each match by column name
            StringBuilder rowString = new StringBuilder();
            rowString.Append("^");
            switch (column.Format)
            {
                case ColumnFormat.AlphaNumeric:
                    rowString.Append("[\\w\\s-]");
                    break;
                case ColumnFormat.Date:
                    rowString.Append("(\\d{1,2}/\\d{1,2}/\\d{4})");
                    break;
                case ColumnFormat.Number:
                    rowString.Append("\\-?\\d");
                    break;
                case ColumnFormat.Text:
                    rowString.Append(".");
                    break;
                case ColumnFormat.NumericalString:
                    rowString.Append("(?!(\\d*\\.?\\d+([eE][-+]?\\d+))).");
                    break;
            }

            if (column.Format == ColumnFormat.Date)
            {
                if (column.Constraint == ColumnConstraint.Optional)
                {
                    rowString.Append("?");
                }
            }
            else
            {
                if (column.Constraint == ColumnConstraint.Optional)
                {
                    if (column.Size == 0)
                    {
                        rowString.Append("*");
                    }
                    else
                    {
                        rowString.Append("{0,");
                        rowString.Append(column.Size);
                        rowString.Append("}");
                    }
                }
                else
                {
                    if (column.Size == 0)
                    {
                        rowString.Append("+");
                    }
                    else
                    {
                        rowString.Append("{1,");
                        rowString.Append(column.Size);
                        rowString.Append("}");
                    }
                }
            }

            rowString.Append("$");
            rowRegexList.Add(new Tuple<string, Regex>(column.Name, new Regex(rowString.ToString())));
        }

        private Dictionary<string, string> MatchRowString(string rowString)
        {
            Dictionary<string, string> stringGroups = new Dictionary<string, string>();
            char defaultDelimiter = ',';
            char escapeDelimiter = '\"';
            int startPos = 0;
            int i = 0;
            while (startPos < rowString.Length)
            {
                int delimiterPos = rowString.IndexOf(defaultDelimiter, startPos);
                string oneGroup;
                if (delimiterPos < 0 || i >= this.rowRegexes.Length - 1)
                {
                    oneGroup = rowString.Substring(startPos);
                    delimiterPos = rowString.Length;
                }
                else
                {
                    oneGroup = rowString.Substring(startPos, delimiterPos - startPos);
                }

                // Handle the escape character ": quoted string can contain comma, but is treated as in a single cell
                if (oneGroup.StartsWith(escapeDelimiter.ToString()))
                {
                    int nextQuotePos = rowString.IndexOf(escapeDelimiter, startPos + 1);

                    // Inside quoted string, double quote is treated as an escape for quote character
                    while (nextQuotePos < rowString.Length - 2 && rowString[nextQuotePos + 1] == escapeDelimiter)
                    {
                        nextQuotePos = rowString.IndexOf(escapeDelimiter, nextQuotePos + 2);
                    }

                    if (nextQuotePos != rowString.Length - 1)
                    {
                        delimiterPos = rowString.IndexOf(defaultDelimiter, nextQuotePos + 1);

                        // If the pairing quote isn't followed by a comma, the input is not correctly formatted
                        if (!string.IsNullOrWhiteSpace(rowString.Substring(nextQuotePos + 1, delimiterPos - nextQuotePos - 1)))
                        {
                            stringGroups.Clear();
                            break;
                        }
                    }
                    else
                    {
                        delimiterPos = rowString.Length;
                    }

                    oneGroup = rowString.Substring(startPos + 1, nextQuotePos - startPos - 1).Replace("\"\"", "\"");
                }

                if (i < this.rowRegexes.Length)
                {
                    Match match = this.rowRegexes[i].Item2.Match(oneGroup);
                    if (match.Groups.Count > 0 && match.Groups[0].Success)
                    {
                        stringGroups.Add(this.rowRegexes[i].Item1, match.Groups[0].Value);
                    }
                    else
                    {
                        stringGroups.Clear();
                        break;
                    }
                }
                else
                {
                    stringGroups.Clear();
                    break;
                }

                startPos = delimiterPos + 1;
                i++;
            }

            // Handle the trailing comma - last column is empty
            if (i == this.rowRegexes.Length - 1 && rowString.Last() == defaultDelimiter)
            {
                if (this.rowRegexes[i].Item2.IsMatch(string.Empty))
                {
                    stringGroups.Add(this.rowRegexes[i].Item1, string.Empty);
                    i++;
                }
                else
                {
                    stringGroups.Clear();
                }
            }

            // Ensure there is no missing columns
            if (i < this.rowRegexes.Length)
            {
                stringGroups.Clear();
            }

            return stringGroups;
        }

        // Overload for new Load method using PartnerConfigurationHandler system
        private IDictionary<string, string> MatchRowString(IDictionary<string, string> rowDictionary)
        {
            // Make sure each field matches the regex
            int matches = 0;
            foreach (Tuple<string, Regex> regex in this.rowRegexes)
            {
                Match match = regex.Item2.Match(rowDictionary[regex.Item1]);
                if (match.Groups.Count > 0 && match.Groups[0].Success)
                {
                    matches++;
                }
            }

            // Ensure correct number of columns
            if (matches == rowDictionary.Count)
            {
                return new Dictionary<string, string>(rowDictionary);
            }

            return new Dictionary<string, string>();
        }

        private void AddIfUnique(IDictionary<string, HashSet<string>> uniqueColumnValues, ColumnDefinition columnDefinition, string cellValue)
        {
            if (columnDefinition.Constraint == ColumnConstraint.Unique)
            {
                HashSet<string> currentColumnValues;

                if (!uniqueColumnValues.TryGetValue(columnDefinition.Name, out currentColumnValues))
                {
                    currentColumnValues = new HashSet<string>();
                    uniqueColumnValues.Add(columnDefinition.Name, currentColumnValues);
                }

                if (currentColumnValues.Contains(cellValue))
                {
                    throw new InvalidOperationException(string.Format("Value: {0} in column: {1}  is not unique in: {2}", cellValue, columnDefinition.Name, this.GetType().Name));
                }
                else
                {
                    currentColumnValues.Add(cellValue);
                }
            }
        }

        private bool SetupNodes(IDictionary<string, HashSet<string>> uniqueColumnValues, IDictionary<string, string> matchResult, Node currentNode)
        {
            string[] consequentValues = new string[this.consequentDefinition.Length];
            for (int i = 0; i < this.consequentDefinition.Length; ++i)
            {
                // The way the regular expression was defined is all or nothing.
                // If match.Groups.Count > 1 then all groups should be available.
                ColumnDefinition columnDefinition = this.consequentDefinition[i];
                consequentValues[i] = matchResult[columnDefinition.Name];
                this.AddIfUnique(uniqueColumnValues, columnDefinition, consequentValues[i]);
            }

            bool handled = false;
            for (int i = this.antecedentDefinition.Length - 1; i >= 0; --i)
            {
                // The way the regular expression was defined is all or nothing.
                // If match.Groups.Count > 1 then all groups should be available.
                ColumnDefinition columnDefinition = this.antecedentDefinition[i];
                string antecedentValue = matchResult[columnDefinition.Name];
                this.AddIfUnique(uniqueColumnValues, columnDefinition, antecedentValue);
                currentNode = currentNode.Register(antecedentValue, consequentValues, columnDefinition.Weight, out handled);
            }

            return handled;
        }

        private void DefineSchema(ColumnDefinition[] antecedentColumns, ColumnDefinition[] consequentColumns)
        {
            HashSet<string> columnNames = new HashSet<string>();
            List<Tuple<string, Regex>> rowRegexList = new List<Tuple<string, Regex>>();
            StringBuilder headerString = new StringBuilder();

            for (int i = 0; i < antecedentColumns.Length; ++i)
            {
                ColumnDefinition currentColumn = antecedentColumns[i];

                if (columnNames.Contains(currentColumn.Name))
                {
                    throw new ArgumentException(string.Format("Column name {0} already exists", currentColumn.Name));
                }
                else
                {
                    columnNames.Add(currentColumn.Name);
                    headerString.Append(currentColumn.Name);
                    headerString.Append(",");
                }

                AppendColumnRegex(rowRegexList, currentColumn);
            }

            for (int i = 0; i < consequentColumns.Length; ++i)
            {
                ColumnDefinition currentColumn = consequentColumns[i];

                if (columnNames.Contains(currentColumn.Name))
                {
                    throw new ArgumentException(string.Format("Column name {0} already exists", currentColumn.Name));
                }
                else
                {
                    columnNames.Add(currentColumn.Name);
                    headerString.Append(currentColumn.Name);
                    if (i < consequentColumns.Length - 1)
                    {
                        headerString.Append(",");
                    }
                }

                AppendColumnRegex(rowRegexList, currentColumn);
            }

            this.header = headerString.ToString();
            this.rowRegexes = rowRegexList.ToArray();
            this.antecedentDefinition = antecedentColumns;
            this.consequentDefinition = consequentColumns;
        }

        private class Node
        {
            private Dictionary<string, Node> children = new Dictionary<string, Node>(StringComparer.InvariantCultureIgnoreCase);
            private string[] consequentValues;
            private uint weight;

            public Node(string[] rowConsequent, uint weight)
            {
                this.consequentValues = rowConsequent;
                this.weight = weight;
            }

            public Node Register(string cellAntecedent, string[] rowConsequent, uint nodeWeight, out bool handled)
            {
                Node resultNode;
                handled = false;
                if (!this.children.TryGetValue(cellAntecedent, out resultNode))
                {
                    resultNode = new Node(rowConsequent, nodeWeight);
                    this.children.Add(cellAntecedent, resultNode);
                    handled = true;
                }

                return resultNode;
            }

            public bool TryResolve(string[] criteria, DataResolverAlgorithms resolverAlgorithm, out string[] result)
            {
                if ((resolverAlgorithm & DataResolverAlgorithms.MaxMatchNumber) == DataResolverAlgorithms.MaxMatchNumber)
                {
                    List<NodeValue> resultCandidates = new List<NodeValue>();
                    this.TryGreedyResolve(criteria, criteria.Length - 1, resultCandidates, 0);
                    result = null;
                    if (resultCandidates.Count == 0)
                    {       
                        return false;
                    }

                    NodeValue maxMatchResult = null;
                    foreach (NodeValue nodeValue in resultCandidates)
                    {
                        if (maxMatchResult == null)
                        {
                            maxMatchResult = nodeValue;
                        }
                        else if (maxMatchResult.Weight < nodeValue.Weight)
                        {
                            maxMatchResult = nodeValue;
                        }
                    }

                    result = maxMatchResult.ConsequentValues;
                    return true;
                }
                else
                {
                    return this.TryLazyResolve(criteria, criteria.Length - 1, out result);
                }
            }

            public IEnumerable<string> GetChildrenKeys()
            {
                return this.children.Keys;
            }

            private void TryGreedyResolve(string[] criteria, int startIndex, List<NodeValue> resultCandidates, uint totalWeight)
            {
                Node resultNode;
                int nextIndex = startIndex - 1;
                int endIndex = 0;

                if (this.children.TryGetValue(criteria[startIndex], out resultNode))
                {
                    if (startIndex == endIndex)
                    {
                        resultCandidates.Add(new NodeValue() { ConsequentValues = resultNode.consequentValues, Weight = totalWeight + resultNode.weight });
                        return;
                    }

                    resultNode.TryGreedyResolve(criteria, nextIndex, resultCandidates, totalWeight + resultNode.weight);
                }

                // String.Empty is the table wildcard (an empty cell).
                if (this.children.TryGetValue(string.Empty, out resultNode))
                {
                    if (startIndex == endIndex)
                    {
                        resultCandidates.Add(new NodeValue() { ConsequentValues = resultNode.consequentValues, Weight = totalWeight });
                        return;
                    }

                    resultNode.TryGreedyResolve(criteria, nextIndex, resultCandidates, totalWeight);
                }
            }

            private bool TryLazyResolve(string[] criteria, int startIndex, out string[] result)
            {
                Node resultNode;
                int nextIndex = startIndex - 1;
                int endIndex = 0;

                if (this.children.TryGetValue(criteria[startIndex], out resultNode))
                {
                    if (startIndex == endIndex)
                    {
                        result = resultNode.consequentValues;
                        return true;
                    }

                    if (resultNode.TryLazyResolve(criteria, nextIndex, out result))
                    {
                        return true;
                    }
                }

                // String.Empty is the table wildcard (an empty cell).
                if (this.children.TryGetValue(string.Empty, out resultNode))
                {
                    if (startIndex == endIndex)
                    {
                        result = resultNode.consequentValues;
                        return true;
                    }

                    if (resultNode.TryLazyResolve(criteria, nextIndex, out result))
                    {
                        return true;
                    }
                }

                result = null;
                return false;
            }

            private class NodeValue
            {
                public string[] ConsequentValues { get; set; }

                public uint Weight { get; set; }
            }
        }
    }
}