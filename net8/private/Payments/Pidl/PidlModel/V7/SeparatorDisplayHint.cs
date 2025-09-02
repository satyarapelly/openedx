// <copyright file="SeparatorDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;

    /// <summary>
    /// Class for describing a Separator DisplayHint
    /// </summary>
    public sealed class SeparatorDisplayHint : DisplayHint
    {
        public SeparatorDisplayHint()
        {
        }

        public SeparatorDisplayHint(SeparatorDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
        }

        protected override string GetDisplayType()
        {
            return HintType.Separator.ToString().ToLower();
        }
    }
}