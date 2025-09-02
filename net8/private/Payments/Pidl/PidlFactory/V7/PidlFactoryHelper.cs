// <copyright file="PidlFactoryHelper.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    internal static class PidlFactoryHelper
    {
        public static void ResolvePidlResourceIdentity<T>(
            Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, T>>>>> pidlResourceDictionary,
            string pidlResouceType,
            string pidlResourceIdentity,
            string operation,
            string countryId,
            string scenario,
            Func<T> result)
        {
            if (!pidlResourceDictionary.ContainsKey(pidlResouceType))
            {
                pidlResourceDictionary[pidlResouceType] = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, T>>>>(StringComparer.CurrentCultureIgnoreCase);
            }

            if (!pidlResourceDictionary[pidlResouceType].ContainsKey(pidlResourceIdentity))
            {
                pidlResourceDictionary[pidlResouceType][pidlResourceIdentity] = new Dictionary<string, Dictionary<string, Dictionary<string, T>>>(StringComparer.CurrentCultureIgnoreCase);
            }

            List<string> operationTypes = string.IsNullOrWhiteSpace(operation)
                ? Constants.PidlPossibleOperationsTypes
                : new List<string>() { operation };

            foreach (var pidlOperation in operationTypes)
            {
                if (!pidlResourceDictionary[pidlResouceType][pidlResourceIdentity].ContainsKey(pidlOperation))
                {
                    pidlResourceDictionary[pidlResouceType][pidlResourceIdentity][pidlOperation] = new Dictionary<string, Dictionary<string, T>>(StringComparer.CurrentCultureIgnoreCase);
                }

                List<string> parsedCountries = string.IsNullOrWhiteSpace(countryId)
                    ? new List<string>() { string.Empty }
                    : PIDLResourceFactory.GetDictionaryFromConfigString(countryId).Keys.ToList();

                foreach (string country in parsedCountries)
                {
                    if (!pidlResourceDictionary[pidlResouceType][pidlResourceIdentity][pidlOperation].ContainsKey(country))
                    {
                        pidlResourceDictionary[pidlResouceType][pidlResourceIdentity][pidlOperation][country] = new Dictionary<string, T>(StringComparer.CurrentCultureIgnoreCase);
                    }

                    string scenarioKey = string.IsNullOrWhiteSpace(scenario) ? string.Empty : scenario;
                    pidlResourceDictionary[pidlResouceType][pidlResourceIdentity][pidlOperation][country][scenarioKey] = result();
                }
            }
        }

        public static bool ValidatePidlSequenceId(string sequenceId, out string errorMessage)
        {
            if (!string.IsNullOrWhiteSpace(sequenceId))
            {
                // Done adding to cache.  Below is to check invalid config data.
                if (sequenceId.IndexOf('.') > -1)
                {
                    // This is a reference to another PIDLResource which needs to be expanded into PropertyDescriptions
                    // at request-time based on the country parameter.  Ensure that the config is valid.
                    // e.g. of a valid cell is "address.billing.us|address.billing.fr|address.billing.ca"
                    string[] infoIds = sequenceId.Split(new char[] { '|' });
                    foreach (string infoId in infoIds)
                    {
                        string[] infoIdParts = infoId.Split(new char[] { '.' });
                        if (infoIdParts.Length != 4)
                        {
                            errorMessage = string.Format("PIDLResource Id \"{0}\" has {1} dot/s.  three dots are expected.", infoId, infoIdParts.Length - 1);
                            return false;
                        }

                        if (string.IsNullOrWhiteSpace(infoIdParts[0]))
                        {
                            errorMessage = string.Format("PIDLResource Id \"{0}\" is invalid.  The first part (before the first dot) has to be non-empty", infoId);
                            return false;
                        }
                    }
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        public static Dictionary<string, PropertyTransformationInfo> GetPropertyTransformationInfo(string propertyId, string country)
        {
            // Try to find a country-specific property transformation.  If not found, fall-back to a generic transformation.
            Dictionary<string, PropertyTransformationInfo> transformationTemplate = PIDLResourceFactory.Instance.GetPropertyTransformation(propertyId, country);
            if (transformationTemplate == null)
            {
                return null;
            }

            Dictionary<string, PropertyTransformationInfo> transformation = new Dictionary<string, PropertyTransformationInfo>();
            foreach (var key in transformationTemplate.Keys)
            {
                transformation[key] = new PropertyTransformationInfo(
                    transformationTemplate[key],
                    new Dictionary<string, string>()
                                {
                                    { Constants.ConfigSpecialStrings.CountryId, country }
                                });
            }

            return transformation;
        }

        public static IEnumerable<string> ParseStyleHints(string commaSeparatedStyleHintsList)
        {
            if (string.IsNullOrWhiteSpace(commaSeparatedStyleHintsList))
            {
               return null;
            }

            var styleHintsArray = commaSeparatedStyleHintsList.Split(',');

            var styleHints = new List<string>();
            foreach (var style in styleHintsArray)
            {
                var trimmedStyle = style.Trim();
                if (!string.IsNullOrEmpty(trimmedStyle))
                {
                    styleHints.Add(trimmedStyle);
                }
            }

            if (styleHints.Count == 0)
            {
                return null;
            }

            return styleHints;
        }

        /// <summary>
        /// Repalce property display hint - Replace property display hint to any other property display hint. Ex - Property display Hint to Secure Property display Hint.
        /// </summary>
        /// <param name="resource">PIDL resources</param>
        /// <param name="sourceDisplayHint">Source display hint</param>
        /// <param name="targetDisplayHint">Target display hint</param>
        public static void RepalceDisplayHint(PIDLResource resource, DisplayHint sourceDisplayHint, DisplayHint targetDisplayHint)
        {
            if (resource == null || sourceDisplayHint == null || targetDisplayHint == null)
            {
                return;
            }

            ReplaceDisplayHintInternal(resource, sourceDisplayHint, targetDisplayHint);
        }

        private static void ReplaceDisplayHintInternal(PIDLResource resource, DisplayHint sourceDisplayHint, DisplayHint targetDisplayHint)
        {
            var sourceDisplayHintIndex = resource.DisplayPages[0].Members.IndexOf(sourceDisplayHint);
            if (sourceDisplayHintIndex < 0)
            {
                // Handled this when property is under any group
                ReplacePropertyFromContainer(resource.DisplayPages.FirstOrDefault(), targetDisplayHint, sourceDisplayHint.HintId);
            }
            else
            {
                // Handled this when property is not in any group.
                resource.DisplayPages[0].Members.RemoveAt(sourceDisplayHintIndex);
                resource.DisplayPages[0].Members.Insert(sourceDisplayHintIndex, targetDisplayHint);
            }
        }

        /// <summary>
        /// Replace Property From Container - Recursively finrd source Display hint and replace with target display hint.
        /// </summary>
        /// <param name="container">Container display hint</param>
        /// <param name="targetDisplayHint">Target display hint</param>
        /// <param name="sourceDisplayHintId">Source display hint Id</param>
        private static void ReplacePropertyFromContainer(ContainerDisplayHint container, DisplayHint targetDisplayHint, string sourceDisplayHintId)
        {
            for (int i = 0; i < container.Members.Count; i++)
            {
                DisplayHint hint = container.Members[i];
                if (hint.HintId.Equals(sourceDisplayHintId))
                {
                    container.Members.RemoveAt(i);
                    container.Members.Insert(i, targetDisplayHint);
                    return;
                }

                var containerHint = hint as ContainerDisplayHint;
                if (containerHint != null)
                {
                    ReplacePropertyFromContainer(containerHint, targetDisplayHint, sourceDisplayHintId);
                }
            }
        }
    }
}