// <copyright file="CsvDataTable.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2017. All rights reserved.</copyright>
namespace Test.Common
{
    using System;
    using System.Data;
    using System.Runtime.Serialization;
    using Microsoft.VisualBasic.FileIO;

    [Serializable]
    public class CsvDataTable : DataTable
    {
        public CsvDataTable(string csvFile, bool hasColumnNames = true, bool trimWhiteSpaces = false)
        {
            using (TextFieldParser parser = new TextFieldParser(csvFile))
            {
                parser.Delimiters = new string[] { "," };
                parser.HasFieldsEnclosedInQuotes = true;

                parser.TrimWhiteSpace = trimWhiteSpaces;

                if (hasColumnNames)
                {
                    // Read the first line to get column names
                    string[] columnNames = parser.ReadFields();

                    foreach (string columnName in columnNames)
                    {
                        this.Columns.Add(columnName);
                    }
                }

                while (!parser.EndOfData)
                {
                    string[] cells = null;
                    cells = parser.ReadFields();
                    this.Rows.Add(cells);
                }
            }
        }

        protected CsvDataTable(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
