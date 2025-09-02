// <copyright file="PIDLConfigParser.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory
{
    using Microsoft.VisualBasic.FileIO;

    public class PIDLConfigParser : TextFieldParser
    {
        private ColumnDefinition[] columns;
        private string filePath;

        public PIDLConfigParser(string filePath, ColumnDefinition[] columns, bool hasHeader) 
            : base(filePath)
        {
            this.filePath = filePath;
            this.columns = columns;

            if (!System.IO.File.Exists(filePath))
            {
                throw new PIDLConfigException(
                    string.Format("PIDL config file \"{0}\" does not exist.", filePath), GlobalConstants.ErrorCodes.PIDLConfigFileDoesNotExist);
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

        public string[] ReadValidatedFields()
        {
            string[] retVal = this.ReadFields();
            if (retVal.Length != this.columns.Length)
            {
                throw new PIDLConfigException(
                    this.filePath,
                    this.LineNumber,
                    string.Format("{0} columns found where {1} were expected.", retVal.Length, this.columns.Length),
                    GlobalConstants.ErrorCodes.PIDLConfigFileInvalidNumberOfColumns);
            }

            for (int i = 0; i < retVal.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(retVal[i]) && this.columns[i].Constraint.HasFlag(ColumnConstraint.Required))
                {
                    throw new PIDLConfigException(
                        this.filePath,
                        this.LineNumber,
                        string.Format("Column \"{0}\" is a required column but the actual value is either empty or blank.", this.columns[i].Name),
                        GlobalConstants.ErrorCodes.PIDLConfigFileRequiredColumnIsMissing);
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