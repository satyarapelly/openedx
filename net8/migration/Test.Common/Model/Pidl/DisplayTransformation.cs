// <copyright file="DisplayTransformation.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class DisplayTransformation
    {
        private static readonly List<string> ValidTransformationTargets = new List<string>() { "forFormat", "forModel" };
        private static readonly List<string> ValidTransformationCategories = new List<string>() { "regex" };

        private string inputRegex = null;
        private string replacementPattern = null;

        public DisplayTransformation()
        {
        }

        public DisplayTransformation(DisplayTransformation template)
        {
            this.TransformCategory = template.TransformCategory;
            this.inputRegex = template.inputRegex;
            this.replacementPattern = template.replacementPattern;
        }

        [JsonProperty(Order = 0, PropertyName = "type")]
        public string TransformCategory { get; set; }

        [JsonProperty(Order = 1, PropertyName = "inputRegex")]
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

        [JsonProperty(Order = 2, PropertyName = "replacementPattern")]
        public string ReplacementPattern
        {
            get
            {
                return this.replacementPattern;
            }

            set
            {
                this.replacementPattern = value;
            }
        }

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