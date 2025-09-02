// <copyright file="ColumnDefinition.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace PIDLTest
{
    using PidlTest.Diff;
    using System;

    public class ColumnDefinition
    {
        public ColumnDefinition(string name)
            : this(name, ColumnConstraint.Required, ColumnFormat.AlphaNumeric, 0)
        {
        }

        public ColumnDefinition(string name, uint size)
            : this(name, ColumnConstraint.Required, ColumnFormat.AlphaNumeric, size)
        {
        }

        public ColumnDefinition(string name, ColumnConstraint constraint)
            : this(name, constraint, ColumnFormat.AlphaNumeric, 0)
        {
        }

        public ColumnDefinition(string name, ColumnConstraint constraint, uint size)
            : this(name, constraint, ColumnFormat.AlphaNumeric, size)
        {
        }

        public ColumnDefinition(string name, ColumnConstraint constraint, ColumnFormat format)
            : this(name, constraint, format, 0)
        {
        }

        public ColumnDefinition(string name, ColumnConstraint constraint, ColumnFormat format, uint size)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            this.Name = name;
            this.Format = format;
            this.Constraint = constraint;
            this.Size = size;
        }

        public string Name { get; set; }

        public uint Size { get; set; }

        public ColumnFormat Format { get; set; }

        public ColumnConstraint Constraint { get; set; }
    }
}
