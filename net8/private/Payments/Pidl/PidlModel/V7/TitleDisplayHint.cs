// <copyright file="TitleDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Class for describing a Title DisplayHint
    /// </summary>
    public sealed class TitleDisplayHint : ContentDisplayHint
    {
        public TitleDisplayHint()
        {
        }

        public TitleDisplayHint(TitleDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
        }

        protected override string GetDisplayType()
        {
            return HintType.Title.ToString().ToLower();
        }
    }
}