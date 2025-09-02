// <copyright file="HeadingDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Class for describing a Heading Display Hint
    /// </summary>
    public class HeadingDisplayHint : ContentDisplayHint
    {
        public HeadingDisplayHint()
        { 
        }

        public HeadingDisplayHint(HeadingDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
        }

        protected override string GetDisplayType()
        {
            return HintType.Heading.ToString().ToLower();
        }
    }
}