// <copyright file="SpinnerDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;

    /// <summary>
    /// Class for describing a Spinner DisplayHint
    /// </summary>
    public sealed class SpinnerDisplayHint : DisplayHint
    {
        public SpinnerDisplayHint()
        {
        }

        public SpinnerDisplayHint(SpinnerDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
        }

        protected override string GetDisplayType()
        {
            return HintType.Spinner.ToString().ToLower();
        }
    }
}