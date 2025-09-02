// <copyright file="PIDLResourceDisplayHintFactory.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;

    /// <summary>
    /// This class acts as the main source of Display Hints for PIDL resources
    /// </summary>
    internal sealed class PIDLResourceDisplayHintFactory : DisplayDescriptionFactory<DisplayDescriptionStore>
    {
        private static PIDLResourceDisplayHintFactory instanceField = new PIDLResourceDisplayHintFactory();

        private PIDLResourceDisplayHintFactory()
        {
            this.Initialize();
        }

        internal static PIDLResourceDisplayHintFactory Instance
        {
            get
            {
                return instanceField;
            }
        }

        /// <summary>
        /// Each page/group has some members which contains buttons or groups or any other hints. 
        /// If the display hint is a button, we need to store it in a list for adding accessibility name later based on the total button count in the page. 
        /// If the display hint is a page/group, iterate through that hint's members if we can find the button in it.
        /// If we found a group, need to iterate that group next as we might find buttons in it.
        /// We need to take the buttons in their order of appearences, so we need to complete iterating the first group completely before proceeding to the next one.
        /// </summary>
        /// <param name="page">Display page</param>
        /// <returns>Returns all the buttons in the page in their order of appearence.</returns>
        public static List<ButtonDisplayHint> GetButtonDisplayHints(PageDisplayHint page)
        {
            List<ButtonDisplayHint> buttonList = new List<ButtonDisplayHint>();
            List<DisplayHint> displayHintQueue = new List<DisplayHint>();

            if (page == null || page?.Members?.Count == 0)
            {
                return buttonList;
            }

            displayHintQueue.Add(page);

            while (displayHintQueue.Count > 0)
            {
                DisplayHint displayHint = displayHintQueue.ElementAt(0);
                displayHintQueue.RemoveAt(0);
                ContainerDisplayHint containerDisplayHint = displayHint as ContainerDisplayHint;
                ButtonDisplayHint buttonDisplayHint = displayHint as ButtonDisplayHint;

                // Here priority represents which hint needs to be processed next.
                if (containerDisplayHint?.Members?.Count > 0)
                {
                    int priority = 0;
                    foreach (DisplayHint innerMember in containerDisplayHint.Members)
                    {
                        displayHintQueue.Insert(priority++, innerMember);
                    }
                }
                else if (buttonDisplayHint != null)
                {
                    buttonList.Add(buttonDisplayHint);
                }
            }

            return buttonList;
        }

        /// <summary>
        /// Searches for and retrieves a button display hint from the provided display page based on the specified action type.
        /// </summary>
        /// <param name="pidlDisplayPage">The display page containing various display hints.</param>
        /// <param name="displayHintActionType">The action type to match against the button display hint's action type.</param>
        /// <returns>Returns the matching button display hint if found; otherwise, returns null.</returns>
        public static ButtonDisplayHint GetButtonDisplayHintByActionType(PageDisplayHint pidlDisplayPage, string displayHintActionType)
        {
            if (pidlDisplayPage == null || string.IsNullOrEmpty(displayHintActionType))
            {
                return null;
            }

            return GetButtonDisplayHints(pidlDisplayPage)
                .FirstOrDefault(buttonDisplayHint => buttonDisplayHint?.Action != null &&
                                                     string.Equals(buttonDisplayHint.Action.ActionType, displayHintActionType, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// This method returns all the display hints of a container, which includes the options of selectOptionDescription
        /// </summary>
        /// <param name="container">A Container display hint</param>
        /// <returns>Return all the display hints as a list in the sequence of appearence</returns>
        public static List<DisplayHint> GetAllDisplayHints(ContainerDisplayHint container)
        {
            List<DisplayHint> displayHints = new List<DisplayHint>();
            List<DisplayHint> displayHintQueue = new List<DisplayHint>();
            if (container != null)
            {
                displayHintQueue.Add(container);
            }

            while (displayHintQueue.Count > 0)
            {
                DisplayHint displayHint = displayHintQueue.First();
                displayHintQueue.RemoveAt(0);

                if (displayHint != null)
                {
                    displayHints.Add(displayHint);
                }

                ContainerDisplayHint containerDisplayHint = displayHint as ContainerDisplayHint;
                PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;

                // Here priority represents which hint needs to be processed next.
                int priority = 0;
                if (containerDisplayHint != null && containerDisplayHint.Members != null)
                {
                    foreach (DisplayHint innerMember in containerDisplayHint.Members)
                    {
                        displayHintQueue.Insert(priority++, innerMember);
                    }
                }
                else if (propertyDisplayHint != null && propertyDisplayHint.PossibleOptions != null)
                {
                    foreach (KeyValuePair<string, SelectOptionDescription> propertyHint in propertyDisplayHint.PossibleOptions)
                    {
                        SelectOptionDescription selectOptionDescription = propertyHint.Value as SelectOptionDescription;
                        displayHintQueue.Insert(priority++, selectOptionDescription.DisplayContent);
                    }
                }
            }

            return displayHints;
        }

        public static List<DisplayHint> GetAllDisplayHints(PIDLResource resource, bool useClientAction = false)
        {
            List<DisplayHint> displayHints = new List<DisplayHint>();

            if (resource != null && resource.DisplayPages != null)
            {
                foreach (PageDisplayHint page in resource.DisplayPages)
                {
                    List<DisplayHint> pageDisplayHints = GetAllDisplayHints(page);
                    displayHints = displayHints.Concat(pageDisplayHints).ToList();
                }
            }
            else if (useClientAction && resource != null && resource.ClientAction != null && resource.ClientAction.ActionType == PXCommon.ClientActionType.Pidl && resource.ClientAction.Context != null)
            {
                var clientActionPidlResources = resource.ClientAction.Context as List<PIDLResource>;
                if (clientActionPidlResources != null)
                {
                    foreach (PIDLResource pidlResource in clientActionPidlResources)
                    {
                        List<DisplayHint> resourceDisplayHints = GetAllDisplayHints(pidlResource, useClientAction);
                        displayHints.AddRange(resourceDisplayHints);
                    }
                }
            }

            return displayHints;
        }

        public static List<DisplayHint> GetAllDisplayHintsOfId(ContainerDisplayHint container, string hintId, bool usePrefix = true)
        {
            List<DisplayHint> displayHints = new List<DisplayHint>();
            List<DisplayHint> displayHintQueue = new List<DisplayHint>();
            if (container != null)
            {
                displayHintQueue.Add(container);
            }

            while (displayHintQueue.Count > 0)
            {
                DisplayHint displayHint = displayHintQueue.First();
                displayHintQueue.RemoveAt(0);

                if (IsDisplayHintMatch(displayHint, hintId, usePrefix))
                {
                    displayHints.Add(displayHint);
                }

                ContainerDisplayHint containerDisplayHint = displayHint as ContainerDisplayHint;
                PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;

                // Here priority represents which hint needs to be processed next.
                int priority = 0;
                if (containerDisplayHint != null && containerDisplayHint.Members != null)
                {
                    foreach (DisplayHint innerMember in containerDisplayHint.Members)
                    {
                        displayHintQueue.Insert(priority++, innerMember);
                    }
                }
                else if (propertyDisplayHint != null && propertyDisplayHint.PossibleOptions != null)
                {
                    foreach (KeyValuePair<string, SelectOptionDescription> propertyHint in propertyDisplayHint.PossibleOptions)
                    {
                        SelectOptionDescription selectOptionDescription = propertyHint.Value as SelectOptionDescription;
                        displayHintQueue.Insert(priority++, selectOptionDescription.DisplayContent);
                    }
                }
            }

            return displayHints;
        }

        public static List<DisplayHint> GetAllDisplayHintsOfId(PIDLResource resource, string hindId, bool usePrefix = true)
        {
            List<DisplayHint> displayHints = new List<DisplayHint>();

            if (resource != null && resource.DisplayPages != null)
            {
                foreach (PageDisplayHint page in resource.DisplayPages)
                {
                    List<DisplayHint> pageDisplayHints = GetAllDisplayHintsOfId(page, hindId, usePrefix);
                    displayHints = displayHints.Concat(pageDisplayHints).ToList();
                }
            }

            return displayHints;
        }

        public static DisplayHint GetDisplayHintById(ContainerDisplayHint container, string hintId, bool usePrefix = true)
        {
            List<DisplayHint> displayHintQueue = new List<DisplayHint>();
            if (container != null)
            {
                displayHintQueue.Add(container);
            }

            while (displayHintQueue.Count > 0)
            {
                DisplayHint displayHint = displayHintQueue.First();
                displayHintQueue.RemoveAt(0);

                if (IsDisplayHintMatch(displayHint, hintId, usePrefix))
                {
                    return displayHint;
                }

                ContainerDisplayHint containerDisplayHint = displayHint as ContainerDisplayHint;
                PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;

                // Here priority represents which hint needs to be processed next.
                int priority = 0;
                if (containerDisplayHint != null && containerDisplayHint.Members != null)
                {
                    foreach (DisplayHint innerMember in containerDisplayHint.Members)
                    {
                        displayHintQueue.Insert(priority++, innerMember);
                    }
                }
                else if (propertyDisplayHint != null && propertyDisplayHint.PossibleOptions != null)
                {
                    foreach (KeyValuePair<string, SelectOptionDescription> propertyHint in propertyDisplayHint.PossibleOptions)
                    {
                        SelectOptionDescription selectOptionDescription = propertyHint.Value as SelectOptionDescription;
                        displayHintQueue.Insert(priority++, selectOptionDescription.DisplayContent);
                    }
                }
            }

            return null;
        }

        public static bool IsDisplayHintMatch(DisplayHint displayHint, string hintId, bool usePrefix = true)
        {
            if (displayHint == null)
            {
                return false;
            }

            if (usePrefix)
            {
                string[] hintIdParts = displayHint.HintId?.Split('_');
                if (hintIdParts.Length > 1 && string.Equals(hintIdParts[0] + "_", hintId))
                {
                    return true;
                }
            }
            
            return string.Equals(hintId, displayHint.HintId);
        }

        protected override DisplayDescriptionStore ResolveDisplayDescriptionStore(string partnerName)
        {
            return this.DisplayDescriptionMap[partnerName];
        }

        private void Initialize()
        {
            string[] partnerDirectories = null;
            string physicalDisplayDescriptionRootFolder = GetDisplayDescriptionFolderPath();

            partnerDirectories = Directory.GetDirectories(physicalDisplayDescriptionRootFolder);

            foreach (var partnerDirectory in partnerDirectories)
            {
                string partnerName = new DirectoryInfo(partnerDirectory).Name;
                string displayDescriptionAbsolutePath = Path.Combine(physicalDisplayDescriptionRootFolder, partnerName);
                DisplayDescriptionStore displayDescriptionStore = new DisplayDescriptionStore(displayDescriptionAbsolutePath);
                displayDescriptionStore.Initialize();
                this.DisplayDescriptionMap[partnerName] = displayDescriptionStore;
            }
        }
    }
}
