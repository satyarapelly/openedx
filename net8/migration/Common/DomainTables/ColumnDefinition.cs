// <copyright file="ColumnDefinition.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.DomainTables
{    
    using System;
    using Newtonsoft.Json;

    [JsonObject]
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
            this.Weight = 1;
        }

        // Parameterless constructor needed for serialization purposes
        public ColumnDefinition()
        {
        }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Size")]
        public uint Size { get; set; }

        [JsonProperty("weight")]
        public uint Weight { get; set; }

        [JsonProperty("Format")]
        public ColumnFormat Format { get; set; }

        [JsonProperty("Constraint")]
        public ColumnConstraint Constraint { get; set; }
    }
}
