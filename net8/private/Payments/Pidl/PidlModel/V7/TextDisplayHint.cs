// <copyright file="TextDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Class for describing a Text DisplayHint
    /// </summary>
    public sealed class TextDisplayHint : ContentDisplayHint
    {
        public TextDisplayHint()
        {
        }

        public TextDisplayHint(TextDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
        }

        protected override string GetDisplayType()
        {
            return HintType.Text.ToString().ToLower();
        }
    }
}