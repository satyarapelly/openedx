// <copyright file="SecurePropertyDisplayHint.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// Model class for SecurePropertyDisplayHint
    /// </summary>
    public sealed class SecurePropertyDisplayHint : PropertyDisplayHint
    {
        private const string SecurePXServiceUrl = "https://{securePx-endpoint}/resources/securefield.html";

        public SecurePropertyDisplayHint()
        {
        }

        public SecurePropertyDisplayHint(PropertyDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
            if (template != null)
            {
                this.FrameName = template.PropertyName;
                this.SourceUrl = SecurePXServiceUrl;
            }
        }

        public SecurePropertyDisplayHint(SecurePropertyDisplayHint template, Dictionary<string, string> contextTable)
            : base(template, contextTable)
        {
            if (template != null)
            {
                this.SourceUrl = template.SourceUrl;
                this.FrameName = template.FrameName;
            }
        }

        [JsonProperty(PropertyName = "frameName")]
        public string FrameName { get; set; }

        [JsonProperty(PropertyName = "sourceUrl")]
        public string SourceUrl { get; set; }

        /// <summary>
        /// Creates an instance of SecurePropertyDisplayHint from the given template.
        /// </summary>
        /// <param name="template">Property Display Hint</param>
        /// <returns>Secure Property Display Hint</returns>
        public static SecurePropertyDisplayHint CreateInstance(PropertyDisplayHint template)
        {
            var securePropertyDisplayHint = new SecurePropertyDisplayHint(template, null)
            {
                DisplayErrorMessages = template.DisplayErrorMessages,

                // override these values from the passed PropertyDisplayHint as they are updatedd from contextTable which we are passing as null here
                DisplayName = template.DisplayName,
                DisplayDescription = template.DisplayDescription
            };

            return securePropertyDisplayHint;
        }

        protected override string GetDisplayType()
        {
            return HintType.SecureProperty.ToString().ToLower();
        }
    }
}