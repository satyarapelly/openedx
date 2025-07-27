// <copyright file="ParsedConfigurationComponent.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ParsedConfigurationComponent
    {
        public ParsedConfigurationComponent(Tuple<int, int> range)
        {
            this.Range = range;
            this.SubComponents = new Dictionary<string, List<ParsedConfigurationComponent>>();
        }

        public Tuple<int, int> Range { get; }

        public Dictionary<string, List<ParsedConfigurationComponent>> SubComponents { get; }

        public List<ParsedConfigurationComponent> this[string key] => this.SubComponents[key];

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("(");
            sb.Append(this.Range.Item1);
            sb.Append(",");
            sb.Append(this.Range.Item2);
            sb.Append(") ");

            if (this.SubComponents.Count != 0)
            {
                sb.Append("SubComponents: ");
                sb.Append(this.SubComponents.Keys.Select(key => key + "(" + this.SubComponents[key].Count + ")").Aggregate((a, n) => a + ","));
            }

            return sb.ToString();
        }
    }
}