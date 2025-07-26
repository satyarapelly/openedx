// <copyright file="DisplayDescriptionFactory.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;

    /// <summary>
    /// This abstract class implements the public interfaces of a DisplayDescriptionFactory. All concrete instances derived from this class
    /// is expected to implement the abstract methods depending on the policy of how the Display Descriptions are read
    /// </summary>
    /// <typeparam name="TDisplayDescriptionStore">The structure which stores the underlying DisplayDescriptionStore</typeparam>
    internal abstract class DisplayDescriptionFactory<TDisplayDescriptionStore>
    {
        private Dictionary<string, TDisplayDescriptionStore> partnerDisplayDescriptionsMap = new Dictionary<string, TDisplayDescriptionStore>(StringComparer.OrdinalIgnoreCase);

        protected Dictionary<string, TDisplayDescriptionStore> DisplayDescriptionMap
        {
            get
            {
                return this.partnerDisplayDescriptionsMap;
            }
        }

        internal List<PageDisplayHint> GetDisplayPages(
            string partnerName,
            string descriptionType,
            string id,
            string country,
            string operation,
            Dictionary<string, string> context = null,
            string displayDescriptionId = null,
            string scenario = null,
            SubmitLink submitLink = null,
            List<string> exposedFlightFeatures = null)
        {
            List<DisplayHint> displayHints = null;
            List<PageDisplayHint> displayPages = new List<PageDisplayHint>();
            PIDLResourcesDisplaySequences pidlResourceDisplaySequences = this.GetPIDLResourceDisplaySequences(partnerName, descriptionType, id, country, operation, displayDescriptionId, scenario);

            // If there were no Display sequence found for the partner then return the default/defaulttemplate display description for that specific pidl resource
            if (pidlResourceDisplaySequences == null)
            {
                partnerName = TemplateHelper.GetDefaultTemplateOrPartner(partnerName);
                if (string.Equals(partnerName, Constants.TemplateName.DefaultTemplate, StringComparison.OrdinalIgnoreCase))
                {
                    pidlResourceDisplaySequences = this.GetPIDLResourceDisplaySequences(partnerName, descriptionType, id, country, operation, displayDescriptionId, scenario);
                }
                else
                {
                    // We need to add the scenario as a parameter here. Consider the instance when the partner is using the PSS and passing the template name in the JSON as the non-template physical partner name.
                    // For example, the operation validateInstance, which in most cases fetches the pidlResourceDisplaySequenceId, will only be able to fetch the pidlResourceDisplaySequenceId when the scenario is passed as a parameter.
                    // Therefore, in such cases, it is important to have the default partner use the scenario as a parameter.
                    pidlResourceDisplaySequences = this.GetPIDLResourceDisplaySequences(partnerName, descriptionType, id, country, operation, scenario: scenario);
                }
            }

            // Display description might or might not exist for a PIDL resource.
            if (pidlResourceDisplaySequences != null)
            {
                foreach (var displaySequenceId in pidlResourceDisplaySequences.DisplaySequenceIds)
                {
                    List<PageDisplayHint> displaySequencePages = null;
                    displayHints = this.GetDisplayHints(partnerName, displaySequenceId, country, operation, context, scenario, exposedFlightFeatures, descriptionType, id).ToList();

                    if (displayHints.Count <= 0)
                    {
                        throw new PIDLConfigException(string.Format("There were no display description found for Sequence Id:", displaySequenceId), Constants.ErrorCodes.PIDLConfigMissingDisplayDescriptions);
                    }

                    displaySequencePages = ConvertDisplayDescriptionToPages(displayHints);
                    displayPages.AddRange(displaySequencePages);
                }
            }

            // If there are no display pages returned for a given resource then return null
            if (displayPages.Count < 1)
            {
                return null;
            }

            if (exposedFlightFeatures?.Contains(Flighting.Features.PXEnableCSVSubmitLinks, StringComparer.OrdinalIgnoreCase) ?? false)
            {
                // add submitlink to content's action context. Context will be overridden by older AddSubmitLinks function when not flighted
                // but we still need to flight this in case a link gets mistakenly added that the older function correctly does not seek to override
                foreach (string contentHintId in submitLink?.GetAllowedContentHintIds() ?? Enumerable.Empty<string>())
                {
                    ContentDisplayHint displayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<ButtonDisplayHint>(contentHintId, displayPages);

                    if (displayHint == null)
                    {
                        displayHint = PIDLResourceDisplayHintFactory.Instance.GetDisplayHintById<HyperlinkDisplayHint>(contentHintId, displayPages);
                    }

                    if (displayHint?.Action != null)
                    {
                        displayHint.Action.Context = submitLink;
                    }
                }
            }

            return displayPages;
        }

        /// <summary>
        /// Gets a Display hint matching a Hintid and Hint type from a hierarchy of Display Hints
        /// </summary>
        /// <typeparam name="TDisplayHintType">The type of Display hint that needs to be retrieved</typeparam>
        /// <param name="hintId">The HintId for the display hint</param>
        /// <param name="displayPages">The list of display pages from where to search the Display Hints</param>
        /// <returns>Returns an instance of  <see cref="DisplayHint"/>matching a Hint Id</returns>
        internal TDisplayHintType GetDisplayHintById<TDisplayHintType>(string hintId, List<PageDisplayHint> displayPages) where TDisplayHintType : DisplayHint
        {
            TDisplayHintType foundDisplayHint = null;

            if (displayPages == null)
            {
                return null;
            }

            foreach (var page in displayPages)
            {
                foundDisplayHint = this.GetDisplayHintByIdRecursive<TDisplayHintType>(hintId, page.Members);
                if (foundDisplayHint != null)
                {
                    return foundDisplayHint;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a display hint matching a hint id and Hnint type from a hierarchy of display Hints under specified parent display hint id.
        /// Added for the scenario below. In the page, there are multiple elements using the same displayId (hint id) "cancelButton". The function added here is to solve the problem by finding certain element under specified parent displayId.
        /// Bug 11106783: [PIDL SDK][Webblends][Paypal][Alipay][CUP]Address Profile Click on Cancel button redirects to the Add PI page in the emulator were as in store we get redirected to the Address Profile page
        /// </summary>
        /// <typeparam name="TDisplayHintType">The type of Display hint that needs to be retrieved</typeparam>
        /// <typeparam name="ParentTDisplayHintType">The type of Display hint of parent Hint</typeparam>
        /// <param name="hintId">The HintId for the display hint</param>
        /// <param name="parentId">The Parent HintId for the display hint</param>
        /// <param name="displayPages">The list of display pages from where to search the Display Hints</param>
        /// <returns>Returns an instance of  <see cref="DisplayHint"/>matching a Hint Id</returns>
        internal TDisplayHintType GetDisplayHintById<TDisplayHintType, ParentTDisplayHintType>(string hintId, string parentId, List<PageDisplayHint> displayPages)
            where TDisplayHintType : DisplayHint
            where ParentTDisplayHintType : DisplayHint
        {
            TDisplayHintType foundDisplayHint = null;
            ParentTDisplayHintType parentDisplayHint = null;

            if (displayPages == null)
            {
                return null;
            }

            foreach (var page in displayPages)
            {
                parentDisplayHint = this.GetDisplayHintByIdRecursive<ParentTDisplayHintType>(parentId, page.Members);
                if (parentDisplayHint != null)
                {
                    break;
                }
            }

            if (parentDisplayHint != null)
            {
                List<DisplayHint> list = new List<DisplayHint> { parentDisplayHint };
                foundDisplayHint = this.GetDisplayHintByIdRecursive<TDisplayHintType>(hintId, list);
            }

            return foundDisplayHint;
        }

        /// <summary>
        /// Remove Display hint matching a Hintid and Hint type from a hierarchy of Display Hints
        /// </summary>
        /// <typeparam name="TDisplayHintType">The type of Display hint that needs to be retrieved</typeparam>
        /// <param name="hintId">The HintId for the display hint</param>
        /// <param name="displayPages">The list of display pages from where to remove a display description</param>
        internal void RemoveDisplayHintById<TDisplayHintType>(string hintId, List<PageDisplayHint> displayPages) where TDisplayHintType : DisplayHint
        {
            if (displayPages == null)
            {
                return;
            }

            foreach (var page in displayPages)
            {
                this.RemoveDisplayHintByIdRecursive<TDisplayHintType>(hintId, page.Members);
            }
        }

        internal IEnumerable<DisplayHint> GetDisplayHints(
            string partnerName,
            string displayHintId,
            string country,
            string operation,
            Dictionary<string, string> context,
            string scenario = null,
            List<string> flightNames = null,
            string pidlResourceType = null,
            string pidlResourceIdentity = null)
        {
            this.ValidatePartnerName(partnerName);
            DisplayDescriptionStore displayDescriptionStore = this.ResolveDisplayDescriptionStore(partnerName);

            if (displayHintId.IndexOf('.') > -1)
            {
                // This means that the displayHintId is actually is an id of another PIDLResource or
                // an array of PIDLResource Ids that need to be expanded into its/their constituent 
                // display descriptions recursively
                string[] pidlResourceIds = displayHintId.Split(new char[] { '|' });
                foreach (string resourceId in pidlResourceIds)
                {
                    string[] resourceIdParts = resourceId.Split(new char[] { '.' });
                    resourceIdParts[2] = resourceIdParts[2].Replace(Constants.ConfigSpecialStrings.Operation, operation);
                    resourceIdParts[3] = resourceIdParts[3].Replace(Constants.ConfigSpecialStrings.CountryId, country);

                    PIDLResourcesDisplaySequences pidlResourceDisplaySequences =
                        this.GetPIDLResourceDisplaySequences(partnerName, resourceIdParts[0], resourceIdParts[1], resourceIdParts[3], resourceIdParts[2], null, scenario);

                    // Throw a config exception if display descriptions are not configured for a composed PIDL Resource
                    if (pidlResourceDisplaySequences == null)
                    {
                        throw new PIDLConfigException(string.Format("Display descriptions are not configured for {0}", displayHintId), Constants.ErrorCodes.PIDLConfigMissingDisplayDescriptions);
                    }

                    foreach (var displaySequenceId in pidlResourceDisplaySequences.DisplaySequenceIds)
                    {
                        foreach (var displayHint in PIDLResourceDisplayHintFactory.Instance.GetDisplayHints(partnerName, displaySequenceId, country, operation, context, scenario, flightNames, pidlResourceType, pidlResourceIdentity))
                        {
                            yield return displayHint;
                        }
                    }
                }

                yield break;
            }

            // Check if the provided displayHintId is a display sequence
            if (displayDescriptionStore.DisplaySequences.ContainsKey(displayHintId))
            {
                foreach (var hintId in displayDescriptionStore.GetDisplaySequence(displayHintId, country, flightNames))
                {
                    foreach (var displayHint in this.GetDisplayHints(partnerName, hintId, country, operation, context, scenario, flightNames, pidlResourceType, pidlResourceIdentity))
                    {
                        if (displayHint != null)
                        {
                            yield return displayHint;
                        }
                    }
                }

                yield break;
            }

            // Check if the provided displayHintId is a Container Display Hint
            ContainerDisplayHint containerDisplayHint = displayDescriptionStore.GetContainerDisplayHint(displayHintId, country);
            if (containerDisplayHint != null)
            {
                ContainerDisplayHint newGroupDisplayHint = DisplayDescriptionStore.CreateContainerDisplayHintFromTemplate(containerDisplayHint);
                newGroupDisplayHint.AddDisplayHints(this.GetDisplayHints(partnerName, newGroupDisplayHint.DisplaySequenceId, country, operation, context, scenario, flightNames, pidlResourceType, pidlResourceIdentity));

                Dictionary<string, string> displayTags = displayDescriptionStore.GetPropertyDisplayDescriptionTags(displayHintId, country);

                if (displayTags != null)
                {
                    newGroupDisplayHint.AddDisplayTags(displayTags);
                }

                Dictionary<string, string> conditionalFields = displayDescriptionStore.GetPropertyDisplayDescriptionConditionalFields(displayHintId);

                if (conditionalFields != null)
                {
                    newGroupDisplayHint.AddConditionalFields(conditionalFields);
                }

                Dictionary<string, DisplayTransformation> displayTransformations = displayDescriptionStore.GetDisplayTransformations(displayHintId, country);

                if (displayTransformations != null)
                {
                    newGroupDisplayHint.AddDisplayTransformations(displayTransformations);
                }

                if (string.Equals(newGroupDisplayHint.ContainerDisplayType, HintType.Page.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    PageDisplayHint pageGroupHint = newGroupDisplayHint as PageDisplayHint;

                    // Split one page into multiple single input field pages
                    if (pageGroupHint != null && pageGroupHint.Extend.GetValueOrDefault())
                    {
                        int addressCountryIndex = pageGroupHint.Members.FindIndex(hint => hint.HintId == "addressCountry");
                        if ((string.Equals(partnerName, GlobalConstants.PartnerNames.Xbox, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(partnerName, GlobalConstants.PartnerNames.AmcXbox, StringComparison.OrdinalIgnoreCase))
                            && string.Equals(pidlResourceType, Constants.DescriptionTypes.PaymentMethodDescription, StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrEmpty(pidlResourceIdentity) && pidlResourceIdentity.StartsWith(Constants.PaymentMethodFamilyNames.CreditCard, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(operation, Constants.PidlOperationTypes.Add, StringComparison.OrdinalIgnoreCase)
                            && addressCountryIndex != -1
                            && PXCommon.Constants.PartnerGroups.SMDEnabledPartners.Contains(partnerName, StringComparer.OrdinalIgnoreCase)
                            && !string.Equals(scenario, Constants.ScenarioNames.FixedCountrySelection, StringComparison.OrdinalIgnoreCase))
                        {
                            var addressCountry = pageGroupHint.Members[addressCountryIndex];

                            int propertyBeginIndex = pageGroupHint.Members.FindIndex(member => string.Equals(member.DisplayHintType, "property", StringComparison.OrdinalIgnoreCase));
                            if (propertyBeginIndex != -1)
                            {
                                addressCountry.IsHidden = false;
                                pageGroupHint.Members.RemoveAt(addressCountryIndex);
                                pageGroupHint.Members.Insert(propertyBeginIndex, addressCountry);
                            }
                        }

                        int inputFieldsCounter = pageGroupHint.Members.Count() - 1;
                        int pageCounter = 1;

                        // Common header for all split pages
                        DisplayHint header = pageGroupHint.Members[0];

                        // Button group for pages in the middle
                        // Also pages except last page and first page(only if FirstButtonGroup is defined) use this button group
                        ContainerDisplayHint extendButtonGroup = displayDescriptionStore.GetContainerDisplayHint(pageGroupHint.ExtendButtonGroup, country);
                        ContainerDisplayHint splitPageButtonGroup = DisplayDescriptionStore.CreateContainerDisplayHintFromTemplate(extendButtonGroup);
                        splitPageButtonGroup.AddDisplayHints(this.GetDisplayHints(partnerName, pageGroupHint.ExtendButtonGroup, country, operation, context, null, flightNames, pidlResourceType, pidlResourceIdentity));

                        // Button group for last page
                        DisplayHint lastPageButtonGroup = pageGroupHint.Members[inputFieldsCounter];

                        // Counter starts from 1 since 0 is header
                        for (int i = 1; i < inputFieldsCounter; i++)
                        {
                            ContainerDisplayHint tempGroupDisplayHint = DisplayDescriptionStore.CreateContainerDisplayHintFromTemplate(containerDisplayHint);
                            tempGroupDisplayHint.HintId += "Details" + pageCounter.ToString();
                            tempGroupDisplayHint.AddDisplayHint(header);

                            // If first element in extend page is hidden or non-input field, add it to first page and continue adding until hit a non-hidden input field
                            // This check only applies to the first page.
                            while (i < inputFieldsCounter
                                && (pageGroupHint.Members[i].IsHidden.GetValueOrDefault() || !string.Equals(pageGroupHint.Members[i].DisplayHintType, "property", StringComparison.OrdinalIgnoreCase)))
                            {
                                tempGroupDisplayHint.AddDisplayHint(pageGroupHint.Members[i++]);
                            }

                            // This is added for enhanced boundary check, it will never be hit unless an empty page is defined in csv
                            if (i >= inputFieldsCounter)
                            {
                                break;
                            }

                            tempGroupDisplayHint.AddDisplayHint(pageGroupHint.Members[i]);

                            // If next element is a hidden element, or non-property element (ex text), put it in the same page
                            while (i < inputFieldsCounter - 1
                                && (pageGroupHint.Members[i + 1].IsHidden.GetValueOrDefault() || !string.Equals(pageGroupHint.Members[i + 1].DisplayHintType, "property", StringComparison.OrdinalIgnoreCase)))
                            {
                                tempGroupDisplayHint.AddDisplayHint(pageGroupHint.Members[++i]);
                            }

                            // If first page requires a special button group, append it to first page.
                            // Otherwise, use the common split button group.
                            if (pageCounter == 1 && pageGroupHint.FirstButtonGroup != null)
                            {
                                ContainerDisplayHint buttonGroup = displayDescriptionStore.GetContainerDisplayHint(pageGroupHint.FirstButtonGroup, country);
                                ContainerDisplayHint firstPageButtonGroup = DisplayDescriptionStore.CreateContainerDisplayHintFromTemplate(buttonGroup);
                                firstPageButtonGroup.AddDisplayHints(this.GetDisplayHints(partnerName, pageGroupHint.FirstButtonGroup, country, operation, context, null, flightNames, pidlResourceType, pidlResourceIdentity));
                                tempGroupDisplayHint.AddDisplayHint(firstPageButtonGroup);
                            }
                            else
                            {
                                tempGroupDisplayHint.AddDisplayHint((i == inputFieldsCounter - 1) ? lastPageButtonGroup : splitPageButtonGroup);
                            }

                            pageCounter++;
                            yield return tempGroupDisplayHint;
                        }
                    }
                    else
                    {
                        yield return newGroupDisplayHint;
                    }
                }
                else
                {
                    yield return newGroupDisplayHint;
                }

                yield break;
            }

            // Check if the provided displayHintId is a Property Display Hint
            DisplayHint propertyDisplayHint = displayDescriptionStore.GetPropertyDisplayHint(displayHintId, country, flightNames);
            if (propertyDisplayHint != null)
            {
                DisplayHint displayHint = PropertyDisplayHintFactory.CreateDisplayHintFromTemplate(
                    partnerName,
                    operation,
                    propertyDisplayHint,
                    context,
                    flightNames);

                string propertyName = displayHint.PropertyName;

                // Hides the property if it's optional for Webblends Inline and Xbox
                if (!string.IsNullOrEmpty(propertyName)
                     && (string.Equals(partnerName, Constants.PidlConfig.XboxPartnerName, StringComparison.OrdinalIgnoreCase)
                     || (string.Equals(partnerName, Constants.PidlConfig.WebblendsInlinePartnerName, StringComparison.OrdinalIgnoreCase)
                         && (!string.Equals(scenario, Constants.ScenarioNames.DisplayOptionalFields, StringComparison.OrdinalIgnoreCase))))
                     && context.ContainsKey(Constants.HiddenOptionalFields.ContextKey) && context[Constants.HiddenOptionalFields.ContextKey].Contains(propertyName))
                {
                    displayHint.IsHidden = true;
                }

                // Read any Display description tags if present
                Dictionary<string, string> displayTags = displayDescriptionStore.GetPropertyDisplayDescriptionTags(displayHintId, country);

                if (displayTags != null)
                {
                    displayHint.AddDisplayTags(displayTags);
                }

                Dictionary<string, string> conditionalFields = displayDescriptionStore.GetPropertyDisplayDescriptionConditionalFields(displayHintId);

                if (conditionalFields != null)
                {
                    displayHint.AddConditionalFields(conditionalFields);
                }

                Dictionary<string, DisplayTransformation> displayTransformations = displayDescriptionStore.GetDisplayTransformations(displayHintId, country);

                if (displayTransformations != null)
                {
                    displayHint.AddDisplayTransformations(displayTransformations);
                }

                PropertyDisplayHint castPropertyDisplayHint = displayHint as PropertyDisplayHint;

                // Read and add any GroupHints to each SelectOptionDescription that has a DisplayContentHintId
                if (castPropertyDisplayHint != null && castPropertyDisplayHint.PossibleOptions != null)
                {
                    foreach (SelectOptionDescription selectOptionDescription in castPropertyDisplayHint.PossibleOptions.Values)
                    {
                        if (!string.IsNullOrEmpty(selectOptionDescription.DisplayContentHintId))
                        {
                            ContainerDisplayHint groupDisplayHintTemplate = displayDescriptionStore.GetContainerDisplayHint(selectOptionDescription.DisplayContentHintId, country);
                            GroupDisplayHint groupDisplayHint = DisplayDescriptionStore.CreateContainerDisplayHintFromTemplate(groupDisplayHintTemplate) as GroupDisplayHint;
                            groupDisplayHint.AddDisplayHints(this.GetDisplayHints(partnerName, groupDisplayHint.DisplaySequenceId, country, operation, context, scenario, flightNames, pidlResourceType, pidlResourceIdentity));
                            
                            if (groupDisplayHint != null)
                            {
                                selectOptionDescription.DisplayContent = groupDisplayHint;
                            }
                            else
                            {
                                throw new PIDLConfigException(
                                    string.Format("No Display Hint found for Id \"{0}\" in country \"{1}\" ", selectOptionDescription.DisplayContentHintId, country),
                                    Constants.ErrorCodes.PIDLConfigUnknownDisplayHintId);
                            }
                        }
                    }
                }

                yield return displayHint;

                yield break;
            }

            throw new PIDLConfigException(
                string.Format("No Display Hint found for Id \"{0}\" in country \"{1}\" ", displayHintId, country),
                Constants.ErrorCodes.PIDLConfigUnknownDisplayHintId);
        }

        internal PropertyDisplayErrorMessageMap GetPropertyDisplayErrorMessages(string propertyHintId, string countryId, string partnerName)
        {
            DisplayDescriptionStore displayDescriptionStore = this.ResolveDisplayDescriptionStore(partnerName);

            if (string.IsNullOrWhiteSpace(propertyHintId))
            {
                throw new ArgumentException("Parameter \"propertyHintId\" in GetPropertyDisplayMessages is null or whitespaces.");
            }

            // processor could be empty (e.g. where a validation does not require processor-specific specialization)
            if (countryId == null)
            {
                throw new ArgumentNullException("countryId");
            }

            if (!displayDescriptionStore.PropertyDisplayMessages.ContainsKey(propertyHintId))
            {
                return null;
            }

            string countryKey = countryId;
            if (!displayDescriptionStore.PropertyDisplayMessages[propertyHintId].ContainsKey(countryId))
            {
                countryKey = GlobalConstants.Defaults.CountryKey;
                if (!displayDescriptionStore.PropertyDisplayMessages[propertyHintId].ContainsKey(countryKey))
                {
                    return null;
                }
            }

            return displayDescriptionStore.PropertyDisplayMessages[propertyHintId][countryKey];
        }

        internal List<DisplayStringMap> GetPidlResourceDisplayStringMap(string partnerName, string descriptionType, string id, string country, string operation, string scenario = null)
        {
            PIDLResourcesDisplaySequences pidlResourceDisplaySequences = this.GetPIDLResourceDisplaySequences(partnerName, descriptionType, id, country, operation, scenario: scenario);

            // If there were no Display sequence found for the partner then return the default/defaulttemplate display description for that specific pidl resource
            if (pidlResourceDisplaySequences == null)
            {
                partnerName = TemplateHelper.GetDefaultTemplateOrPartner(partnerName);                
                pidlResourceDisplaySequences = this.GetPIDLResourceDisplaySequences(partnerName, descriptionType, id, country, operation);
            }

            if (pidlResourceDisplaySequences == null)
            {
                return null;
            }

            List<DisplayStringMap> displayStringMapList = null;
            List<string> displayStringSequenceIds = pidlResourceDisplaySequences.DisplayStringsSequenceIds;

            if (pidlResourceDisplaySequences == null)
            {
                return null;
            }

            if (displayStringSequenceIds == null || displayStringSequenceIds.Count == 0)
            {
                return null;
            }

            foreach (var displayStringSequenceId in displayStringSequenceIds)
            {
                DisplayDescriptionStore displayDescriptionStore = this.ResolveDisplayDescriptionStore(partnerName);

                if (string.IsNullOrEmpty(displayStringSequenceId))
                {
                    continue;
                }

                List<string> displayStringIds = displayDescriptionStore.GetDisplayStringSequence(displayStringSequenceId, country);

                foreach (var displayStringId in displayStringIds)
                {
                    DisplayStringMap templateStringMap;
                    templateStringMap = displayDescriptionStore.GetDisplayStringMap(displayStringId, country);

                    if (displayStringMapList == null)
                    {
                        displayStringMapList = new List<DisplayStringMap>();
                    }

                    displayStringMapList.Add(new DisplayStringMap(templateStringMap));
                }
            }

            return displayStringMapList;
        }

        protected static string GetDisplayDescriptionFolderPath()
        {
            string displayDescriptionFolderPath;

            if (WebHostingUtility.IsApplicationSelfHosted())
            {
                string locationBeforeShadowCopy = typeof(Microsoft.Commerce.Payments.PidlFactory.V7.PIDLResourceFactory).Assembly.CodeBase;
                UriBuilder uri = new UriBuilder(new Uri(locationBeforeShadowCopy));
                string locationWithoutUriPrefixes = Uri.UnescapeDataString(uri.Path);
                string dir = Path.GetDirectoryName(locationWithoutUriPrefixes);
                displayDescriptionFolderPath = Path.Combine(dir, Constants.PidlConfig.DisplayDescriptionFolderRootPath);
            }
            else
            {
                displayDescriptionFolderPath = System.Web.HttpContext.Current.Server.MapPath(GlobalConstants.FolderNames.WebAppData + Constants.PidlConfig.DisplayDescriptionFolderRootPath);
            }

            return displayDescriptionFolderPath;
        }

        protected abstract DisplayDescriptionStore ResolveDisplayDescriptionStore(string partnerName);

        private static List<PageDisplayHint> ConvertDisplayDescriptionToPages(List<DisplayHint> displayDescriptions)
        {
            List<PageDisplayHint> displayPages = new List<PageDisplayHint>();
            bool foundDisplayPage = false;
            PageDisplayHint firstPage = displayDescriptions[0] as PageDisplayHint;

            // Check to see if the first display description is a page. In that case we would enforce the rule that all the display descriptions
            // in the first level are pages. If none of them are pages then a default page is injected.
            if (firstPage != null)
            {
                foundDisplayPage = true;
            }

            foreach (var hint in displayDescriptions)
            {
                PageDisplayHint page = hint as PageDisplayHint;

                if (page != null)
                {
                    displayPages.Add(page);
                }
                else
                {
                    // In this case a non page display description was found when a page was expected.
                    if (foundDisplayPage)
                    {
                        throw new PIDLConfigException("All the display descriptions need to be nested inside a page", Constants.ErrorCodes.PIDLConfigInvalidPageConfiguration);
                    }

                    continue;
                }
            }

            // If no display pages were found then inject it as a root
            if (!foundDisplayPage)
            {
                PageDisplayHint page = new PageDisplayHint()
                {
                    HintId = Constants.PidlConfig.DisplayDescriptionRootPageId
                };

                page.AddDisplayHints(displayDescriptions);

                displayPages.Add(page);
            }

            return displayPages;
        }

        private PIDLResourcesDisplaySequences GetPIDLResourceDisplaySequences(
            string partnerName, 
            string descriptionType, 
            string id, 
            string country, 
            string operation, 
            string displayDescriptionId = null,
            string scenario = null)
        {
            this.ValidatePartnerName(partnerName);

            if (string.IsNullOrWhiteSpace(descriptionType))
            {
                throw new ArgumentException(
                    string.Format("Parameter DescriptionType \"{0}\" in GetPIDLResourceDisplayHint is null or whitespaces.", descriptionType));
            }

            // id and country could be empty (e.g. "ad.." which is address description with no specific id or country)
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            if (country == null)
            {
                throw new ArgumentNullException("country");
            }

            DisplayDescriptionStore displayDescriptionStore = this.ResolveDisplayDescriptionStore(partnerName);

            // Try to find a country specific description template first.  If such a template is not found,
            // fall-back to a generic Display Hint template.
            PIDLResourcesDisplaySequences retVal = null;
            if (!displayDescriptionStore.PidlResourceDisplaySequences.ContainsKey(descriptionType))
            {
                return null;
            }

            string idKey = id;
            if (!displayDescriptionStore.PidlResourceDisplaySequences[descriptionType].ContainsKey(idKey))
            {
                idKey = GlobalConstants.Defaults.InfoDescriptorIdKey;
                if (!displayDescriptionStore.PidlResourceDisplaySequences[descriptionType].ContainsKey(idKey))
                {
                    if (string.CompareOrdinal(id, Constants.PaymentMethodId.PayPal) == 0)
                    {
                        // PIDL is empty for Payment Method Description of PayPal. So return empty PIDLResourceDisplayHint for PayPal.
                        return new PIDLResourcesDisplaySequences();
                    }

                    return null;
                }
            }

            if (!displayDescriptionStore.PidlResourceDisplaySequences[descriptionType][idKey].ContainsKey(operation))
            {
                return null;
            }

            string countryKey = country;

            // TODO: Bug 1682897:Add one more column into PIDLResourcesDisplaySequences to differentiate the mapping by DataDescription identity or mapping by both DataDescription and DisplayDescription identity
            if (displayDescriptionId != null)
            {
                countryKey = displayDescriptionId;
            }

            if (!displayDescriptionStore.PidlResourceDisplaySequences[descriptionType][idKey][operation].ContainsKey(countryKey))
            {
                countryKey = GlobalConstants.Defaults.CountryKey;
                if (!displayDescriptionStore.PidlResourceDisplaySequences[descriptionType][idKey][operation].ContainsKey(countryKey))
                {
                    return null;
                }
            }

            string scenarioKey = string.IsNullOrWhiteSpace(scenario) 
                ? string.Empty 
                : scenario;

            if (!displayDescriptionStore.PidlResourceDisplaySequences[descriptionType][idKey][operation][countryKey].ContainsKey(scenarioKey))
            {
                scenarioKey = GlobalConstants.Defaults.ScenarioKey;
                if (!displayDescriptionStore.PidlResourceDisplaySequences[descriptionType][idKey][operation][countryKey].ContainsKey(scenarioKey))
                {
                    return null;
                }
            }

            retVal = displayDescriptionStore.PidlResourceDisplaySequences[descriptionType][idKey][operation][countryKey][scenarioKey];

            return retVal;
        }

        private void RemoveDisplayHintByIdRecursive<TDisplayHintType>(string hintId, List<DisplayHint> displayHints) where TDisplayHintType : DisplayHint
        {
            if (displayHints == null || displayHints.Count == 0)
            {
                return;
            }

            if (hintId == null)
            {
                throw new ArgumentNullException("hintId");
            }

            for (int i = displayHints.Count - 1; i > -1; i--)
            {
                if (displayHints[i] is ContainerDisplayHint)
                {
                    var containerDisplayHint = (ContainerDisplayHint)displayHints[i];
                    if (containerDisplayHint.Members != null)
                    {
                        if (containerDisplayHint.Members.Count > 0)
                        {
                            this.RemoveDisplayHintByIdRecursive<TDisplayHintType>(hintId, containerDisplayHint.Members);
                        }
                    }
                }

                if (string.Equals(hintId, displayHints[i].HintId, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (displayHints[i].GetType() == typeof(TDisplayHintType))
                    {
                        displayHints.RemoveAt(i);
                    }
                }
            }

            return;
        }

        private TDisplayHintType GetDisplayHintByIdRecursive<TDisplayHintType>(string hintId, List<DisplayHint> displayHints) where TDisplayHintType : DisplayHint
        {
            if (displayHints == null || displayHints.Count == 0)
            {
                return null;
            }

            if (hintId == null)
            {
                throw new ArgumentNullException("hintId");
            }

            foreach (var displayHint in displayHints)
            {
                if (string.Equals(hintId, displayHint.HintId, StringComparison.OrdinalIgnoreCase))
                {
                    if (displayHint.GetType() == typeof(TDisplayHintType))
                    {
                        return (TDisplayHintType)displayHint;
                    }
                    else
                    {
                        continue;
                    }
                }

                var containerDisplayHint = displayHint as ContainerDisplayHint;

                if (containerDisplayHint != null)
                {
                    if (containerDisplayHint.Members != null)
                    {
                        if (containerDisplayHint.Members.Count > 0)
                        {
                            TDisplayHintType foundDisplayHint = this.GetDisplayHintByIdRecursive<TDisplayHintType>(hintId, containerDisplayHint.Members);
                            if (foundDisplayHint != null)
                            {
                                return foundDisplayHint;
                            }
                        }
                    }
                }
            }

            return null;
        }

        private void ValidatePartnerName(string partnerName)
        {
            if (string.IsNullOrWhiteSpace(partnerName))
            {
                throw new ArgumentException(
                    string.Format("Parameter partnerName \"{0}\" is null or whitespaces.", partnerName));
            }

            if (!this.partnerDisplayDescriptionsMap.ContainsKey(partnerName))
            {
                throw new PIDLArgumentException(
                    string.Format("Invalid Partner Name."), Constants.ErrorCodes.PIDLPartnerNameIsNotValid);
            }
        }
    }
}
