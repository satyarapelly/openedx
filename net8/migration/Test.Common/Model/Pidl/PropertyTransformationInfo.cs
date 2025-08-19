// <copyright file="PropertyTransformationInfo.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class PropertyTransformationInfo
    {
        private static readonly List<string> ValidTransformationTargets = new List<string>() { "forDisplay", "forSubmit" };
        private static readonly List<string> ValidTransformationCategories = new List<string>() { "regex", "service" };

        private string inputRegex = null;
        private string transformRegex = null;
        
        public PropertyTransformationInfo()
        {
        }

        public PropertyTransformationInfo(PropertyTransformationInfo template, Dictionary<string, string> contextTable)
        {
            this.TransformCategory = template.TransformCategory;
            this.inputRegex = template.inputRegex;
            this.transformRegex = template.transformRegex;
            this.UrlTransformationType = template.UrlTransformationType;
            this.TransformUrl = template.TransformUrl;

            foreach (KeyValuePair<string, string> contextKeyValue in contextTable)
            {
                this.inputRegex = this.inputRegex == null ? null : this.inputRegex.Replace(contextKeyValue.Key, contextKeyValue.Value);
                this.transformRegex = this.transformRegex == null ? null : this.transformRegex.Replace(contextKeyValue.Key, contextKeyValue.Value);
            }
        }

        [JsonProperty(Order = 0, PropertyName = "type")]
        public string TransformCategory { get; set; }

        [JsonProperty(Order = 1, PropertyName = "inputregex")]
        public string InputRegex
        {
            get
            {
                return this.inputRegex;
            }

            set
            {
                this.inputRegex = value;
            }
        }

        [JsonProperty(Order = 2, PropertyName = "transformregex")]
        public string TransformRegex
        {
            get
            {
                return this.transformRegex;
            }

            set
            {
                this.transformRegex = value;
            }
        }

        [JsonProperty(Order = 3, PropertyName = "url")]
        public string TransformUrl { get; set; }

        public string UrlTransformationType { get; set; }

        public static bool IsValidTransformationTarget(string transformationTarget)
        {
            return ValidTransformationTargets.Contains(transformationTarget);
        }

        public static bool IsValidTransformationCategory(string transformationCategory)
        {
            return ValidTransformationCategories.Contains(transformationCategory);
        }
    }
}