// <copyright file="PidlResourceExtensions.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model 
{
    using System.Collections.Generic;
    using System.Linq;
    using Pidl;

    /// <summary>
    /// Contains helper functions related to finding DisplayHints in Pidls.  For example, tests often need to find a 
    /// DisplayHint by its HintId, or assert that a DisplayHint of a particular type exists on a particular page or 
    /// find a Page or a Group that contains a particular DisplayHint etc.  This class contains such helper functions.
    /// </summary>
    public static class PidlResourceExtensions
    {
        /// <summary>
        /// Finds all PropertyDescriptions in the current instance of a PidlResource
        /// </summary>
        /// <param name="pidl">Pidl to find property descriptions in</param>
        /// <param name="path">Dot separated path to a property description.  e.g. "details.address.address_line_1"</param>
        /// <returns>An IEnumerable of all property descriptions in the current Pidl.</returns>
        public static PropertyDescription TryGetPropertyDescription(this PIDLResource pidl, string path)
        {
            PropertyDescription retVal = null;
            string[] keys = path.Split(new char[] { '.' });
            for (int i = 0; i < keys.Count() && pidl != null; i++)
            {
                object value = null;
                pidl.DataDescription.TryGetValue(keys[i], out value);

                if (i < keys.Count() - 1)
                {
                    var innerPidlList = value as List<PIDLResource>;
                    pidl = innerPidlList[0];
                }
                else
                {
                    retVal = value as PropertyDescription;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Finds all DisplayHints in the current instance of a PidlResource
        /// </summary>
        /// <param name="pidl">Pidl to find display hints in</param>
        /// <returns>An IEnumerable of all display hints in the current Pidl.</returns>
        public static IEnumerable<DisplayHint> DisplayHints(this PIDLResource pidl)
        {
            foreach (var page in pidl.DisplayPages)
            {
                foreach (var displayHint in page.DisplayHints())
                {
                    yield return displayHint;
                }
            }
        }

        /// <summary>
        /// Finds all DisplayHints in the current instance of a ContainerDisplayHint
        /// </summary>
        /// <param name="container">Container to find display hints in</param>
        /// <returns>An IEnumerable of all display hints in the current Pidl.</returns>
        public static IEnumerable<DisplayHint> DisplayHints(this ContainerDisplayHint container)
        {
            foreach (var displayHint in container.Members)
            {
                yield return displayHint;

                var subContainer = displayHint as ContainerDisplayHint;
                if (subContainer != null)
                {
                    foreach (var displayHintInSubContainer in subContainer.DisplayHints())
                    {
                        yield return displayHintInSubContainer;
                    }
                }
            }
        }
    }
}
