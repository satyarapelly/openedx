// <copyright file="RegexMaskDefinition.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System.Text.RegularExpressions;

    public class RegexMaskDefinition
    {
        public Regex Regex { get; set; }

        public MatchEvaluator ReplacementFunction { get; set; }
    }
}