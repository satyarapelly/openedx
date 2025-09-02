// <copyright file="SubheadingDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Class for describing a Subheading DisplayHint
    /// </summary>
    public sealed class SubheadingDisplayHint : ContentDisplayHint
    {
        public SubheadingDisplayHint()
        { 
        }

        public SubheadingDisplayHint(SubheadingDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
        }

        protected override string GetDisplayType()
        {
            return HintType.SubHeading.ToString().ToLower();
        }
    }
}