// <copyright file="PidlResourceExtensions.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace CIT.PidlFactory.Helpers
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// Contains helper functions related to finding DisplayHints in Pidls.  For example, tests often need to find a 
    /// DisplayHint by its HintId, or assert that a DisplayHint of a particular type exists on a particular page or 
    /// find a Page or a Group that contains a particular DisplayHint etc.  This class contains such helper functions.
    /// </summary>
    internal static class PidlResourceExtensions
    {
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

        /// <summary>
        /// Finds a DisplayHint in the current instance of a PidlResource by <paramref name="hintId"/>
        /// </summary>
        /// <param name="pidl">Current instance of a PidlResource</param>
        /// <param name="hintId">Hint Id of the DisplayHint to find</param>
        /// <param name="includeHelpDisplayDescriptions">If true, the search will also include HelpDisplayDescriptions</param>
        /// <returns></returns>
        public static DisplayHint GetDisplayHintById(this PIDLResource pidl, string hintId, bool includeHelpDisplayDescriptions)
        {
            if (!includeHelpDisplayDescriptions)
            {
                return pidl.GetDisplayHintById(hintId);
            }

            if (pidl.DisplayPages != null)
            {
                return GetDisplayHintById(pidl.DisplayPages, hintId);
            }

            return null;
        }

        private static DisplayHint GetDisplayHintById(IEnumerable<DisplayHint> displayHints, string hintId)
        {
            DisplayHint result;

            foreach (var hint in displayHints)
            {
                if (hint.HintId == hintId)
                {
                    return hint;
                }

                if (hint.HelpDisplayDescriptions != null)
                {
                    result = GetDisplayHintById(hint.HelpDisplayDescriptions, hintId);
                    if (result != null)
                    {
                        return result;
                    }
                }

                if (hint is ContainerDisplayHint)
                {
                    result = GetDisplayHintById(((ContainerDisplayHint)hint).Members, hintId);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}
