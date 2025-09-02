// <copyright file="PidlConfigParser.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace PidlTest.Diff
{
    using Microsoft.VisualBasic.FileIO;

    public class PidlConfigParser : TextFieldParser
    {
        private ColumnDefinition[] columns;
        private string filePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="PidlConfigParser"/> class.
        /// Check file exists
        /// Remove header row
        /// </summary>
        /// <param name="filePath">Path to the KnownDiffs file</param>
        /// <param name="columns">A set of definitions for each column. Field Type, Required...</param>
        /// <param name="hasHeader">Identifies if the file has a header</param>
        public PidlConfigParser(string filePath, ColumnDefinition[] columns, bool hasHeader) : base(filePath)
        {
            this.filePath = filePath;
            this.columns = columns;

            if (!System.IO.File.Exists(filePath))
            {
                throw new PidlConfigException(
                    string.Format("PIDL config file \"{0}\" does not exist.", filePath));
            }

            this.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            this.Delimiters = new string[] { "," };
            this.HasFieldsEnclosedInQuotes = true;
            this.TrimWhiteSpace = false;

            if (hasHeader)
            {
                this.ReadFields();
            }
        }

        /// <summary>
        /// Read a row of data
        /// Validate column length
        /// Validate required fields
        /// Trim white spaces where posible
        /// </summary>
        /// <returns>All data rows in the file</returns>
        public string[] ReadValidatedFields()
        {
            string[] retVal = this.ReadFields();
            if (retVal.Length != this.columns.Length)
            {
                throw new PidlConfigException(
                    this.filePath,
                    this.LineNumber,
                    string.Format("{0} columns found where {1} were expected.", retVal.Length, this.columns.Length));
            }

            for (int i = 0; i < retVal.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(retVal[i]) && this.columns[i].Constraint.HasFlag(ColumnConstraint.Required))
                {
                    throw new PidlConfigException(
                        this.filePath,
                        this.LineNumber,
                        string.Format("Column \"{0}\" is a required column but the actual value is either empty or blank.", this.columns[i].Name));
                }

                if (!this.columns[i].Constraint.HasFlag(ColumnConstraint.DoNotTrimWhiteSpaces))
                {
                    if (!string.IsNullOrEmpty(retVal[i]))
                    {
                        retVal[i] = retVal[i].Trim();
                    }
                }
            }

            return retVal;
        }
    }
}