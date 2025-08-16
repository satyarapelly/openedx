// <copyright file="TextGroupDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Class for describing a Text Group Display Hint
    /// </summary>
    public class TextGroupDisplayHint : ContainerDisplayHint
    {
        public TextGroupDisplayHint() :
            base()
        {
        }

        public TextGroupDisplayHint(TextGroupDisplayHint template)
            : base(template)
        {
        }

        protected override string GetDisplayType()
        {
            return HintType.TextGroup.ToString().ToLower();
        }
    }
}