// <copyright file="ExpressionDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a Expression Display Hint
    /// </summary>
    public sealed class ExpressionDisplayHint : ContentDisplayHint
    {
        public ExpressionDisplayHint()
        {
        }

        public ExpressionDisplayHint(ExpressionDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
        }

        protected override string GetDisplayType()
        {
            return HintType.Expression.ToString().ToLower();
        }
    }
}