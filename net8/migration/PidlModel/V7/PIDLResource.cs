// <copyright file="PIDLResource.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class PIDLResource
    {
        private Dictionary<string, DataSource> dataSources;
        private Dictionary<string, object> dataDescription;
        private Dictionary<string, string> identity;
        private Dictionary<string, RestLink> links;
        private Dictionary<string, object> clientContext;
        private List<PageDisplayHint> pidlResourceDisplayPages;
        private List<PIDLResource> linkedPidls;
        private Dictionary<string, string> scenarioContext;
        private Dictionary<string, object> clientSettings;

        // this ctor is to return an empty PIDLResource. For example, TaxIdsController will need to return an empty
        // PIDLResource in cases where TaxIds are not required for a country.
        public PIDLResource()
        {
        }

        public PIDLResource(Dictionary<string, string> identityTable)
        {
            this.dataSources = null;
            this.clientSettings = null;
            this.dataDescription = new Dictionary<string, object>();
            this.identity = identityTable;
            string descriptionType = null;

            // Adding the try catch in order to add session id field without needing to add a description_type 
            try
            {
                descriptionType = identityTable[Constants.DescriptionIdentityFields.DescriptionType];
            }
            catch
            {
                descriptionType = null;
            }

            foreach (string key in this.Identity.Keys)
            {
                if (string.Compare(key, Constants.DescriptionIdentityFields.DescriptionType, true) == 0)
                {
                    continue;
                }

                // When a PIDL based UI tries to POST user data to an API, the API needs to know the identity of the PIDL.  
                // This is important because that specific PIDL is the data contract for the data being posted up to the API.
                // Type is "hidden" because the user does not have to see a UI input field.
                string payloadKey = GetPayloadKey(descriptionType, key);
                this.DataDescription[payloadKey] = new PropertyDescription()
                {
                    PropertyType = "clientData",
                    DataType = "hidden",
                    PropertyDescriptionType = "hidden",
                    IsUpdatable = true,
                    DefaultValue = this.Identity[key]
                };
            }
        }

        [JsonProperty(Order = 0, PropertyName = "identity")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, string> Identity
        {
            get
            {
                return this.identity;
            }

            set
            {
                this.identity = value;
            }
        }

        [JsonProperty(Order = 1, PropertyName = "data_description")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, object> DataDescription
        {
            get
            {
                return this.dataDescription;
            }

            set
            {
                this.dataDescription = value;
            }
        }

        [JsonProperty(Order = 2, PropertyName = "dataSources")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, DataSource> DataSources
        {
            get
            {
                return this.dataSources;
            }

            set
            {
                this.dataSources = value;
            }
        }

        [JsonProperty(Order = 3, PropertyName = "displayDescription")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<PageDisplayHint> DisplayPages
        {
            get
            {
                return this.pidlResourceDisplayPages;
            }

            set
            {
                if (value != null)
                {
                    if (this.pidlResourceDisplayPages == null)
                    {
                        this.pidlResourceDisplayPages = new List<PageDisplayHint>();
                    }

                    this.pidlResourceDisplayPages.AddRange(value);
                }
            }
        }

        [JsonProperty(Order = 4, PropertyName = "strings")]
        public PidlResourceStrings PidlResourceStrings { get; set; }

        [JsonProperty(Order = 5, PropertyName = "links")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, RestLink> Links
        {
            get
            {
                return this.links;
            }

            set
            {
                this.links = value;
            }
        }

        // ClientContext includes both user and device context info
        [JsonProperty(Order = 6, PropertyName = "clientContext")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, object> ClientContext
        {
            get
            {
                return this.clientContext;
            }

            set
            {
                this.clientContext = value;
            }
        }

        [JsonProperty(Order = 7, PropertyName = "linkedPidls")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<PIDLResource> LinkedPidls
        {
            get
            {
                return this.linkedPidls;
            }

            set
            {
                this.linkedPidls = value;
            }
        }

        [JsonProperty(Order = 8, PropertyName = "scenarioContext")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, string> ScenarioContext
        {
            get
            {
                return this.scenarioContext;
            }

            set
            {
                this.scenarioContext = value;
            }
        }

        [JsonProperty(Order = 9, PropertyName = "clientAction")]
        public ClientAction ClientAction { get; set; }

        [JsonProperty(Order = 10, PropertyName = "initializeContext")]
        public InitializeContext InitializeContext { get; set; }

        [JsonProperty(Order = 11, PropertyName = "pidlInstanceContexts")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, ResourceActionContext> PIDLInstanceContexts { get; set; }

        [JsonProperty(Order = 12, PropertyName = "clientSettings")]
        public Dictionary<string, object> ClientSettings
        {
            get
            {
                return this.clientSettings;
            }
        }

        public static void PopulatePIDLResource(dynamic pidlDocument, List<PIDLResource> pidlResourceList)
        {
            dynamic pidlDynamicObject = JsonConvert.DeserializeObject(Convert.ToString(pidlDocument));
            foreach (dynamic item in pidlDynamicObject)
            {
                string pidlDocumentString = Convert.ToString(item);
                var pidlResource = JsonConvert.DeserializeObject<PIDLResource>(pidlDocumentString);
                var displayHint = new List<PageDisplayHint>();
                if (item.displayDescription != null)
                {
                    var displayHintObject = PIDLResource.PopulatePageDisplayHint(item.displayDescription, displayHint);
                    pidlResource.DisplayPages.Clear();
                    pidlResource.DisplayPages.AddRange(displayHintObject);
                }
                else
                {
                    pidlResource.DisplayPages = null;
                }

                ////For Linked Pidls
                if (item.linkedPidls != null)
                {
                    var linkedPidls = new List<PIDLResource>();

                    PIDLResource.PopulatePIDLResource(item.linkedPidls, linkedPidls);

                    if (linkedPidls.Count > 0)
                    {
                        pidlResource.LinkedPidls.Clear();
                        pidlResource.LinkedPidls.AddRange(linkedPidls);
                    }
                }

                pidlResourceList.Add(pidlResource);
            }
        }

        /// <summary>
        /// Adds a client setting to the PIDL resource.
        /// </summary>
        /// <param name="key">The key for the client setting.</param>
        /// <param name="value">The value for the client setting.</param>
        public void AddClientSetting(string key, object value)
        {
            if (this.clientSettings == null)
            {
                this.clientSettings = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            this.clientSettings.Add(key, value);
        }

        public void AddLinkedPidl(PIDLResource pidl)
        {
            if (this.linkedPidls == null)
            {
                this.linkedPidls = new List<PIDLResource>();
            }

            this.linkedPidls.Add(pidl);
        }

        public void AddDataSource(string name, DataSource value)
        {
            if (this.dataSources == null)
            {
                this.dataSources = new Dictionary<string, DataSource>(StringComparer.CurrentCultureIgnoreCase);
            }

            this.dataSources.Add(name, value);
        }

        public void RemoveDataSource()
        {
            if (this.dataSources != null)
            {
                this.dataSources = null;
            }
        }

        public void AddDisplayPages(IEnumerable<PageDisplayHint> displayPages)
        {
            if (this.pidlResourceDisplayPages == null)
            {
                this.pidlResourceDisplayPages = new List<PageDisplayHint>();
            }

            this.pidlResourceDisplayPages.AddRange(displayPages);
        }

        public void InsertDisplayPageAtIndex(int index, PageDisplayHint displayPage)
        {
            if (this.pidlResourceDisplayPages == null)
            {
                this.pidlResourceDisplayPages = new List<PageDisplayHint>();
            }

            if (index >= 0 && index <= this.pidlResourceDisplayPages.Count)
            {
                this.pidlResourceDisplayPages.Insert(index, displayPage);
            }
        }

        public void AddLink(string name, RestLink url)
        {
            if (this.links == null)
            {
                this.links = new Dictionary<string, RestLink>(StringComparer.CurrentCultureIgnoreCase);
            }

            this.links.Add(name, url);
        }

        public void AddIdentity(string name, string value)
        {
            if (this.identity == null)
            {
                this.identity = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            }

            this.identity.Add(name, value);
        }

        public void InitClientContext(Dictionary<string, object> inputContext)
        {
            if (inputContext == null)
            {
                this.clientContext = null;
                return;
            }

            if (this.clientContext == null)
            {
                this.clientContext = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            this.clientContext.Clear();
            foreach (var clientContextKeyValuePair in inputContext)
            {
                this.clientContext.Add(clientContextKeyValuePair.Key, clientContextKeyValuePair.Value);
            }
        }

        public void InitScenarioContext()
        {
            if (this.scenarioContext == null)
            {
                this.scenarioContext = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            }
        }

        public void MakePrimaryResource()
        {
            this.InitScenarioContext();
            this.scenarioContext[Constants.ScenarioContextsFields.ResourceType] = Constants.ResourceTypes.Primary;
        }

        public void MakeSecondaryResource()
        {
            this.InitScenarioContext();
            this.scenarioContext[Constants.ScenarioContextsFields.ResourceType] = Constants.ResourceTypes.Secondary;
        }

        public void SetErrorHandlingToThrow()
        {
            this.InitScenarioContext();
            this.scenarioContext[Constants.ScenarioContextsFields.TerminatingErrorHandling] = Constants.TerminatingErrorHandlingMethods.Throw;
        }

        public void SetErrorHandlingToIgnore()
        {
            this.InitScenarioContext();
            this.scenarioContext[Constants.ScenarioContextsFields.TerminatingErrorHandling] = Constants.TerminatingErrorHandlingMethods.Ignore;
        }

        public void RemoveEmptyPidlContainerHints()
        {
            if (this.DisplayPages != null)
            {
                foreach (PageDisplayHint page in this.DisplayPages)
                {
                    this.RemoveEmptyPidlContainerHints(page);
                }
            }
        }

        public void RemoveDataDescription(string path, string key, string descriptionType = null)
        {
            // Extend this function to accept full path and remove DataDescription in sub Pidls
            // for tw tax id, path is additionalData, key is country, descriptionType is taxid
            // for departmental purchase, path is default_address, key is address_line1, descriptionType is null
            // for consumer profile, path is null, key is country, descriptionType is profile
            Dictionary<string, object> targetDataDescription = this.DataDescription;

            if (!string.IsNullOrEmpty(path))
            {
                string[] paths = path.Split('.');
                for (int i = 0; i < paths.Length; i++)
                {
                    if (!targetDataDescription.ContainsKey(paths[i]))
                    {
                        return;
                    }

                    List<PIDLResource> subPidls = targetDataDescription[paths[i]] as List<PIDLResource>;
                    if (subPidls == null || subPidls.Count == 0)
                    {
                        return;
                    }

                    targetDataDescription = subPidls[0].DataDescription;

                    if (targetDataDescription == null || targetDataDescription.Count == 0)
                    {
                        return;
                    }
                }
            }

            string keyToRemove = string.IsNullOrEmpty(descriptionType) ? key : GetPayloadKey(descriptionType, key);
            targetDataDescription.Remove(keyToRemove);
        }

        /// <summary>
        /// Overloading of the existing method. It removes the key from DataDescription without need of the path input.
        /// </summary>
        /// <param name="key">Keyname to remove from DataDescription</param>
        public void RemoveDataDescription(string key)
        {
            // Note: Incase where the multiple occurrences of key are present in DataDescription at different depth/level,
            // then it will select the first one it encounters.

            // Selects the key from DataDecriptions
            JToken targetDataDescription = JToken.FromObject(this.DataDescription);
            JToken keyToken = targetDataDescription.SelectToken("$.." + key);
            
            if (keyToken != null && !string.IsNullOrEmpty(keyToken.Path))
            {
                string path = keyToken.Path;
                List<string> paths = path.Split('.').ToList();

                if (paths.Count > 1)
                {
                    // Remove the last item as it is key name
                    paths.RemoveAt(paths.Count - 1);

                    // Remove the data_description & array indexing like [0] from path
                    // Converts address[0].data_decription.email -> address.email
                    paths.RemoveAll(p => string.Equals(Constants.PidlPropertyNames.DataDescription, p, StringComparison.OrdinalIgnoreCase));

                    path = string.Join(".", paths);
                    path = Regex.Replace(path, @"\[\d+\]", string.Empty);

                    // We required to do the above cleaning so, we can call the below method with expected path
                    this.RemoveDataDescription(path, key);
                }
                else
                {
                    // the key present at root level
                    this.RemoveDataDescription(null, key);
                }
            }
        }

        public void UpdateDataDescription(string path, string key, PropertyDescription updatedPropertyDescription)
        {
            var targetDataDescription = this.GetTargetDataDescription(path);
            if (targetDataDescription != null && targetDataDescription.ContainsKey(key))
            {
                targetDataDescription[key] = updatedPropertyDescription;
            }
        }

        public Dictionary<string, object> GetTargetDataDescription(string path)
        {
            Dictionary<string, object> targetDataDescription = this.DataDescription;

            if (!string.IsNullOrEmpty(path) && targetDataDescription != null)
            {
                string[] paths = path.Split('.');
                for (int i = 0; i < paths.Length; i++)
                {
                    if (!targetDataDescription.ContainsKey(paths[i]))
                    {
                        return targetDataDescription;
                    }

                    List<PIDLResource> subPidls = targetDataDescription[paths[i]] as List<PIDLResource>;
                    if (subPidls == null || subPidls.Count == 0)
                    {
                        return targetDataDescription;
                    }

                    targetDataDescription = subPidls[0].DataDescription;

                    if (targetDataDescription == null || targetDataDescription.Count == 0)
                    {
                        return targetDataDescription;
                    }
                }
            }

            return targetDataDescription;
        }

        public void SetPropertyState(string propertyName, bool enabled, IList<string> possibleOptionKeysToRemove = null)
        {
            foreach (DisplayHint member in this.GetDisplayHints() ?? Enumerable.Empty<DisplayHint>())
            {
                if (string.Equals(member.PropertyName, propertyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    PropertyDisplayHint countryDisplayHint = member as PropertyDisplayHint;
                    countryDisplayHint.IsDisabled = !enabled;
                    if (possibleOptionKeysToRemove != null && possibleOptionKeysToRemove.Count > 0)
                    {
                        Dictionary<string, SelectOptionDescription> possibleOptions = countryDisplayHint.PossibleOptions;
                        foreach (string possibleOptionKey in possibleOptionKeysToRemove)
                        {
                            if (possibleOptions.ContainsKey(possibleOptionKey))
                            {
                                possibleOptions.Remove(possibleOptionKey);
                            }
                        }

                        countryDisplayHint.SetPossibleOptions(possibleOptions);
                    }
                }
            }
        }

        public void UpdateDisplayHintPossibleOptions(string propertyName, IList<string> possibleOptionKeysToRetain)
        {
            foreach (DisplayHint member in this.GetDisplayHints() ?? Enumerable.Empty<DisplayHint>())
            {
                if (string.Equals(member.PropertyName, propertyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    PropertyDisplayHint displayHint = member as PropertyDisplayHint;
                    if (displayHint != null && displayHint.PossibleOptions != null && possibleOptionKeysToRetain != null && possibleOptionKeysToRetain.Count > 0)
                    {
                        var possibleOptions = displayHint.PossibleOptions;
                        foreach (string possibleOptionKey in possibleOptions.Keys.ToList())
                        {
                            if (!possibleOptionKeysToRetain.Contains(possibleOptionKey))
                            {
                                possibleOptions.Remove(possibleOptionKey);
                            }
                        }

                        displayHint.SetPossibleOptions(possibleOptions);
                    }
                }
            }
        }

        public void UpdateIsOptionalProperty(string[] propertyNames, bool isOptional)
        {
            foreach (string propertyName in propertyNames)
            {
                this.UpdateIsOptionalProperty(propertyName, isOptional);
            }
        }

        public void UpdateIsOptionalProperty(string propertyName, bool isOptional)
        {
            PropertyDescription propertyDescription = this.GetPropertyDescriptionByPropertyName(propertyName);
            if (propertyDescription != null)
            {
                propertyDescription.IsOptional = isOptional;
            }
        }

        public void UpdateIsKeyProperty(string propertyName, bool isKey)
        {
            PropertyDescription propertyDescription = this.GetPropertyDescriptionByPropertyName(propertyName);
            if (propertyDescription != null)
            {
                propertyDescription.IsKey = isKey;
            }
        }

        public bool RemoveDisplayTransformations()
        {
            bool removed = false;
            foreach (DisplayHint member in this.GetDisplayHints() ?? Enumerable.Empty<DisplayHint>())
            {
                removed = removed || member.RemoveDisplayTransformations();
            }

            return removed;
        }

        public void AddDisplayTag(string propertyName, string tagKey, string tagValue)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            DisplayHint displayHint = this.GetDisplayHintByPropertyName(propertyName);
            if (displayHint != null)
            {
                displayHint.AddDisplayTag(tagKey, tagValue);
            }
        }

        public void SetMaxLength(string propertyName, int maxLength)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            DisplayHint displayHint = this.GetDisplayHintByPropertyName(propertyName);
            PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;
            if (propertyDisplayHint != null)
            {
                propertyDisplayHint.MaxLength = maxLength;
            }
        }

        public void SetMinLength(string propertyName, int minLength)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            DisplayHint displayHint = this.GetDisplayHintByPropertyName(propertyName);
            PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;
            if (propertyDisplayHint != null)
            {
                propertyDisplayHint.MinLength = minLength;
            }
        }

        public void SetResolutionPolicy(string propertyName, string resolutionPolicy)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            DisplayHint displayHint = this.GetDisplayHintByPropertyName(propertyName);
            PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;
            if (propertyDisplayHint != null)
            {
                propertyDisplayHint.ResolutionPolicy = resolutionPolicy;
            }
        }

        public void ShowDisplayName(string hintId, bool state)
        {
            DisplayHint displayHint = this.GetDisplayHintById(hintId);
            PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;
            if (propertyDisplayHint != null)
            {
                propertyDisplayHint.ShowDisplayName = state.ToString().ToLower();
                return;
            }

            GroupDisplayHint groupDisplayHint = displayHint as GroupDisplayHint;
            if (groupDisplayHint != null)
            {
                groupDisplayHint.ShowDisplayName = state.ToString().ToLower();
                return;
            }
        }

        public void RemoveDisplayHintById(string hintId)
        {
            if (this.DisplayPages != null)
            {
                foreach (PageDisplayHint page in this.DisplayPages)
                {
                    this.RemoveDisplayHintFromContainer(page, hintId);
                }
            }
        }

        public void SetDisplayName(string hintId, string displayName)
        {
            DisplayHint displayHint = this.GetDisplayHintById(hintId);
            PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;
            if (propertyDisplayHint != null)
            {
                propertyDisplayHint.DisplayName = displayName;
                return;
            }

            GroupDisplayHint groupDisplayHint = displayHint as GroupDisplayHint;
            if (groupDisplayHint != null)
            {
                groupDisplayHint.DisplayName = displayName;
                return;
            }
        }

        public void SetDisplaySelectionText(string hintId, string displaySelectionText)
        {
            PropertyDisplayHint propertyDisplayHint = this.GetDisplayHintById(hintId) as PropertyDisplayHint;
            if (propertyDisplayHint != null)
            {
                propertyDisplayHint.DisplaySelectionText = displaySelectionText;
                return;
            }
        }

        public IEnumerable<DisplayHint> GetAllDisplayHints()
        {
            foreach (PageDisplayHint page in this.DisplayPages ?? Enumerable.Empty<PageDisplayHint>())
            {
                foreach (DisplayHint containedDisplayHint in this.GetAllDisplayHints(page))
                {
                    yield return containedDisplayHint;
                }
            }
        }

        public List<DisplayHint> GetAllDisplayHintsOfId(string hintId)
        {
            List<DisplayHint> retList = new List<DisplayHint>();
            foreach (PageDisplayHint page in this.DisplayPages ?? Enumerable.Empty<PageDisplayHint>())
            {
                DisplayHint retVal = this.GetDisplayHintFromContainer(page, hintId);
                if (retVal != null)
                {
                    retList.Add(retVal);
                }
            }

            return retList;
        }

        public DisplayHint GetDisplayHintOrPageById(string hintId)
        {
            DisplayHint retVal = null;

            foreach (PageDisplayHint page in this.DisplayPages)
            {
                if (page.HintId.Equals(hintId))
                {
                    return page;
                }

                retVal = this.GetDisplayHintFromContainer(page, hintId);
                if (retVal != null)
                {
                    return retVal;
                }
            }

            return retVal;
        }

        public DisplayHint GetDisplayHintById(string hintId)
        {
            DisplayHint retVal = null;

            if (this.DisplayPages != null)
            {
                foreach (PageDisplayHint page in this.DisplayPages)
                {
                    retVal = this.GetDisplayHintFromContainer(page, hintId);
                    if (retVal != null)
                    {
                        return retVal;
                    }
                }
            }

            return retVal;
        }

        public DisplayHint GetFirstEmptyPidlContainer()
        {
            DisplayHint retVal = null;

            // One container might contains more than one PidlContainer, which is a placeholder for linked pidl
            // If there are more than one PidlContainer, link linked pidl to them one by one in sequence.
            // The following code checks PidlContainer one by one. 
            // If there is already a LinkedPidlIdentity to that PidlContainer (already linked), continue to find next PidlContainer.
            if (this.DisplayPages != null)
            {
                foreach (PageDisplayHint page in this.DisplayPages)
                {
                    retVal = this.GetDisplayHintFromContainer(page, Constants.DisplayHintIds.PidlContainer);
                    if (retVal != null)
                    {
                        PidlContainerDisplayHint pidlContainer = retVal as PidlContainerDisplayHint;
                        if (pidlContainer != null && pidlContainer.LinkedPidlIdentity == null)
                        {
                            return retVal;
                        }
                    }
                }
            }

            return retVal;
        }

        public ContainerDisplayHint GetPidlContainerDisplayHintbyDisplayId(string hintId)
        {
            ContainerDisplayHint retVal = null;

            if (this.DisplayPages != null)
            {
                foreach (PageDisplayHint page in this.DisplayPages)
                {
                    retVal = this.GetContainerDisplayHintFromContainer(page, hintId);
                    if (retVal != null)
                    {
                        return retVal;
                    }
                }
            }

            return retVal;
        }

        public GroupDisplayHint GetParentGroupForDisplayHint(string displayHintId)
        {
            GroupDisplayHint retVal = null;

            if (this.DisplayPages != null)
            {
                foreach (PageDisplayHint page in this.DisplayPages)
                {
                    retVal = this.GetParentGroupForDisplayHintFromContainer(page, displayHintId);
                    if (retVal != null)
                    {
                        return retVal;
                    }
                }
            }

            return retVal;
        }

        public DisplayHint GetDisplayHintByPropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || this.DisplayPages == null)
            {
                return null;
            }

            DisplayHint retVal = null;

            foreach (PageDisplayHint page in this.DisplayPages)
            {
                retVal = this.GetDisplayHintFromContainerByPropertyName(page, propertyName);
                if (retVal != null)
                {
                    return retVal;
                }
            }

            return retVal;
        }

        public string GetElementTypeByPropertyDisplayHint(PropertyDisplayHint propertyDisplayHint)
        {
            if (propertyDisplayHint == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(propertyDisplayHint.SelectType) && string.Equals(propertyDisplayHint.SelectType, Constants.ElementTypes.ButtonList, StringComparison.OrdinalIgnoreCase))
            {
                return Constants.ElementTypes.ButtonList;
            }

            if (propertyDisplayHint.PossibleOptions != null)
            {
                return Constants.ElementTypes.Dropdown;
            }

            if (this.DataDescription != null)
            {
                PropertyDescription propertyDescription = this.GetPropertyDescriptionByPropertyName(propertyDisplayHint.PropertyName);
                if (propertyDescription != null && !string.IsNullOrEmpty(propertyDescription.DataType))
                {
                    if (string.Equals(propertyDescription.DataType, "string", StringComparison.OrdinalIgnoreCase))
                    {
                        return Constants.ElementTypes.Textbox;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Recursively removes first occurence of PropertyDescription for a given key in DataDescription
        /// </summary>
        /// <param name="propertyName">The key in the DataDescription to be removed</param>
        /// <returns>True if the key was found and removed, false otherwise</returns>
        public bool RemoveFirstDataDescriptionByPropertyName(string propertyName)
        {
            bool isRemoved = false;

            foreach (string currentPropertyName in this.DataDescription.Keys)
            {
                PropertyDescription propertyDescription = this.DataDescription[currentPropertyName] as PropertyDescription;

                if (propertyDescription == null)
                {
                    List<PIDLResource> subPidls = this.DataDescription[currentPropertyName] as List<PIDLResource>;

                    if (subPidls != null)
                    {
                        foreach (PIDLResource subPidl in subPidls)
                        {
                            isRemoved = subPidl.RemoveFirstDataDescriptionByPropertyName(propertyName);

                            if (isRemoved)
                            {
                                break;
                            }
                        }
                    }
                }
                else if (string.Equals(propertyName, currentPropertyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.DataDescription.Remove(currentPropertyName);

                    isRemoved = true;
                }

                if (isRemoved)
                {
                    break;
                }
            }

            return isRemoved;
        }

        public void RemoveResolutionRegex()
        {
            foreach (PropertyDescription propertyDescription in this.GetPropertyDescriptions() ?? Enumerable.Empty<PropertyDescription>())
            {
                if (propertyDescription.Validation != null && propertyDescription.Validation.ResolutionRegex != null)
                {
                    propertyDescription.Validation.ResolutionRegex = null;
                }
            }
        }

        public void RemoveResourceActionContext()
        {
            PropertyDisplayHint selectHint = this.GetDisplayHintById("paymentMethod") as PropertyDisplayHint;
            if (selectHint != null && selectHint.SelectType.Equals("buttonList") && selectHint.PossibleOptions != null && selectHint.PossibleOptions.Count > 0)
            {
                Dictionary<string, SelectOptionDescription> newPossibleOptions = new Dictionary<string, SelectOptionDescription>();
                foreach (KeyValuePair<string, SelectOptionDescription> option in selectHint.PossibleOptions)
                {
                    SelectOptionDescription newOption = new SelectOptionDescription { DisplayText = option.Value.DisplayText };
                    ActionContext context = option.Value.PidlAction.Context as ActionContext;
                    newOption.PidlAction = new DisplayHintAction(
                        "success",
                        false,
                        new ActionContext
                        {
                            Id = context.Id,
                            PaymentMethodFamily = context.PaymentMethodFamily,
                            PaymentMethodType = context.PaymentMethodType
                        },
                        null);
                    newOption.AccessibilityTag = option.Value.AccessibilityTag;

                    newPossibleOptions.Add(option.Key, newOption);
                }

                selectHint.SetPossibleOptions(newPossibleOptions);
            }
        }

        /// <summary>
        /// Returns a List of PropertyDescriptions from a given Identity
        /// </summary>
        /// <param name="resourceIdentity">The Idenity Key for a PIDL</param>
        /// <param name="key">The Identity Key that needs to be searched</param>
        /// <returns>List of Property Descriptions matching a PIDL Identity</returns>
        public IEnumerable<PropertyDescription> GetPropertyDescriptionOfIdentity(string resourceIdentity, string key)
        {
            if ((this.Identity == null) || (!this.Identity.ContainsKey(Constants.DescriptionIdentityFields.DescriptionType)))
            {
                yield break;
            }

            string descriptionType = this.Identity[Constants.DescriptionIdentityFields.DescriptionType];

            if (string.Compare(descriptionType, resourceIdentity, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                string propertyDescriptionType = GetPayloadKey(descriptionType, key);

                foreach (var dataDescription in this.DataDescription.Keys)
                {
                    if (this.DataDescription[dataDescription] is PropertyDescription)
                    {
                        var propertyDescription = this.DataDescription[dataDescription] as PropertyDescription;
                        if (string.Compare(dataDescription, propertyDescriptionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            // Property Found, hence yield return
                            yield return propertyDescription;
                        }
                    }
                    else if (this.DataDescription[dataDescription] is List<PIDLResource>)
                    {
                        // This is a Recursive PIDL hence traverse it recursively
                        var pidlResourceList = this.DataDescription[dataDescription] as List<PIDLResource>;
                        foreach (var resource in pidlResourceList)
                        {
                            // For each recursive search yield return
                            foreach (var r in resource.GetPropertyDescriptionOfIdentity(resourceIdentity, key))
                            {
                                yield return r;
                            }
                        }
                    }
                    else
                    {
                        // Continue processing rest of the elements
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if data descriptions contains a given key
        /// </summary>
        /// <param name="key">The Identity Key that needs to be searched</param>
        /// <returns>returns true if found, else returns false</returns>
        public bool HasDataDescriptionWithKey(string key)
        {
            if (this.DataDescription?.Keys != null)
            {
                foreach (string propertyName in this.DataDescription.Keys)
                {
                    if (string.Equals(propertyName, key, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    List<PIDLResource> subPidl = this.DataDescription[propertyName] as List<PIDLResource>;
                    if (subPidl != null)
                    {
                        foreach (var resource in subPidl)
                        {
                            if (resource.HasDataDescriptionWithKey(key))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a PropertyDescription from a given key
        /// </summary>
        /// <param name="key">The Identity Key that needs to be searched</param>
        /// <returns>Property Description matching a given key</returns>
        public PropertyDescription GetPropertyDescriptionByPropertyName(string key)
        {
            PropertyDescription retVal = null;

            if (this.DataDescription?.Keys != null)
            {
                foreach (string propertyName in this.DataDescription.Keys)
                {
                    PropertyDescription propertyDescription = this.DataDescription[propertyName] as PropertyDescription;

                    // If it can be convert to PropertyDescription, it must be a sub-pidl
                    if (propertyDescription == null)
                    {
                        List<PIDLResource> subPidl = this.DataDescription[propertyName] as List<PIDLResource>;
                        if (subPidl != null)
                        {
                            foreach (var resource in subPidl)
                            {
                                retVal = resource.GetPropertyDescriptionByPropertyName(key);
                                if (retVal != null)
                                {
                                    return retVal;
                                }
                            }
                        }
                    }
                    else if (string.Equals(propertyName, key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return propertyDescription;
                    }
                }
            }

            return retVal;
        }

        public PropertyDescription GetPropertyDescriptionByPropertyNameWithFullPath(string key)
        {
            string[] paths = key.Split('.');
            Dictionary<string, object> targetDataDescription = this.DataDescription;

            if (paths.Length > 1)
            {
                for (int i = 0; i < paths.Length - 1; i++)
                {
                    if (!targetDataDescription.ContainsKey(paths[i]))
                    {
                        return null;
                    }

                    List<PIDLResource> subPidls = targetDataDescription[paths[i]] as List<PIDLResource>;
                    if (subPidls == null || subPidls.Count == 0 || subPidls[0].DataDescription == null || subPidls[0].DataDescription.Count == 0)
                    {
                        return null;
                    }

                    targetDataDescription = subPidls[0].DataDescription;
                }
            }

            PropertyDescription propertyDescription = targetDataDescription[paths[paths.Length - 1]] as PropertyDescription;
            return propertyDescription;
        }

        public IEnumerable<DisplayHint> GetDisplayHints()
        {
            foreach (PageDisplayHint page in this.DisplayPages ?? Enumerable.Empty<PageDisplayHint>())
            {
                foreach (Tuple<DisplayHint, ContainerDisplayHint> memberAndContainer in this.GetDisplayHintsFromContainer(page) ?? Enumerable.Empty<Tuple<DisplayHint, ContainerDisplayHint>>())
                {
                    yield return memberAndContainer.Item1;
                }
            }
        }

        public void UpdateDataSourceHeaders(string dataSourceName, string headerName, string headerValue)
        {
            DataSource dataSource;
            if (this.DataSources.TryGetValue(dataSourceName, out dataSource))
            {
                string existingHeaderValue;
                if (dataSource.Headers.TryGetValue(headerName, out existingHeaderValue))
                {
                    string newFlightValue = GetNewHeadervalue(existingHeaderValue, headerValue);

                    if (!string.IsNullOrWhiteSpace(newFlightValue))
                    {
                        dataSource.Headers.Remove(headerName);
                        dataSource.Headers.Add(headerName, newFlightValue);
                    }
                }
                else
                {
                    dataSource.Headers.Add(headerName, headerValue);
                }
            }
        }

        public DisplayHint GetDisplayHintFromContainer(ContainerDisplayHint container, string hintId)
        {
            DisplayHint retVal = null;

            if (container?.HintId == hintId)
            {
                return container;
            }

            foreach (DisplayHint hint in container?.Members ?? Enumerable.Empty<DisplayHint>())
            {
                if (hint.HintId.Equals(hintId))
                {
                    return hint;
                }

                var containerHint = hint as ContainerDisplayHint;
                if (containerHint != null)
                {
                    retVal = this.GetDisplayHintFromContainer(containerHint, hintId);
                    if (retVal != null)
                    {
                        return retVal;
                    }
                }
            }

            return retVal;
        }

        public GroupDisplayHint GetParentGroupForDisplayHintFromContainer(ContainerDisplayHint container, string displayHintId)
        {
            if (string.IsNullOrEmpty(displayHintId))
            {
                return null;
            }

            GroupDisplayHint parentGroupHint  = null;
            
            foreach (DisplayHint hint in container?.Members ?? Enumerable.Empty<DisplayHint>())
            {
                parentGroupHint = hint as GroupDisplayHint;
                if (parentGroupHint?.Members?.Any(x => x.HintId == displayHintId) == true)
                {
                    return parentGroupHint;
                }

                var containerHint = hint as ContainerDisplayHint;
                if (containerHint != null)
                {
                    parentGroupHint = this.GetParentGroupForDisplayHintFromContainer(containerHint, displayHintId);
                    if (parentGroupHint != null)
                    {
                        return parentGroupHint;
                    }
                }
            }

            return parentGroupHint;
        }

        public void RemoveDisplayHintFromContainer(ContainerDisplayHint container, string hintId)
        {
            foreach (DisplayHint hint in container.Members)
            {
                if (hint.HintId.Equals(hintId))
                {
                    container.Members.Remove(hint);
                    return;
                }

                var containerHint = hint as ContainerDisplayHint;
                if (containerHint != null)
                {
                    this.RemoveDisplayHintFromContainer(containerHint, hintId);
                }
            }
        }

        public void AppendDataSourceQueryParam(string dataSourceName, string queryParam, string queryParamValue)
        {
            DataSource dataSource;
            if (this.DataSources.TryGetValue(dataSourceName, out dataSource)
                && !string.IsNullOrEmpty(dataSource.Href)
                && !string.IsNullOrEmpty(queryParam)
                && !string.IsNullOrEmpty(queryParamValue))
            {
                dataSource.Href += (dataSource.Href.IndexOf('?') > 0 ? "&" : "?") + $"{queryParam}={queryParamValue}";
            }
        }

        public void UpdatePropertyType(string[] propertyNames, string propertyType)
        {
            foreach (string propertyName in propertyNames)
            {
                this.UpdatePropertyType(propertyName, propertyType);
            }
        }

        public void SetVisibilityOfDisplayHint(string hintId, bool state)
        {
            DisplayHint displayHint = this.GetDisplayHintById(hintId);
            if (displayHint != null)
            {
                displayHint.IsHidden = state;
            }
        }

        public void HideDisplayHintsById(List<string> hintIds)
        {
            foreach (string hintId in hintIds)
            {
                this.SetVisibilityOfDisplayHint(hintId, true);
            }
        }

        public void UpdateDefaultValueForProperty(string propertyName, string defaultValue)
        {
            PropertyDescription property = this.GetPropertyDescriptionByPropertyName(propertyName);
            if (property != null)
            {
                property.DefaultValue = defaultValue;
            }
        }

        public void UpdatePropertyType(string propertyName, string propertyType)
        {
            // Changes the 'PropertyType' property into the required value
            PropertyDescription propertyDescription = this.GetPropertyDescriptionByPropertyName(propertyName);
            if (propertyDescription != null)
            {
                propertyDescription.PropertyType = propertyType;
            }
        }

        public IEnumerable<DisplayHint> GetAllDisplayHints(ContainerDisplayHint container)
        {
            yield return container;

            foreach (DisplayHint displayHint in container.Members ?? Enumerable.Empty<DisplayHint>())
            {
                ContainerDisplayHint containerDisplayHint = displayHint as ContainerDisplayHint;

                if (containerDisplayHint != null)
                {
                    foreach (DisplayHint containedDisplayHint in this.GetAllDisplayHints(containerDisplayHint))
                    {
                        yield return containedDisplayHint;
                    }
                }
                else
                {
                    yield return displayHint;
                }
            }
        }

        private static ContainerDisplayHint GetContainerDisplayHint(string displayType, string dataMemberString = null)
        {
            if (displayType == HintType.Group.ToString().ToLower())
            {
                if (dataMemberString == null)
                {
                    return new GroupDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<GroupDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == HintType.Captcha.ToString().ToLower())
            {
                if (dataMemberString == null)
                {
                    return new CaptchaDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<CaptchaDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == HintType.TextGroup.ToString().ToLower())
            {
                if (dataMemberString == null)
                {
                    return new TextGroupDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<TextGroupDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == HintType.Page.ToString().ToLower())
            {
                if (dataMemberString == null)
                {
                    return new PageDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<PageDisplayHint>(dataMemberString);
                }
            }

            return null;
        }

        private static ContentDisplayHint GetContentDisplayHint(string displayType, string dataMemberString = null)
        {
            if (displayType == "button")
            {
                if (dataMemberString == null)
                {
                    return new ButtonDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<ButtonDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == "expression")
            {
                if (dataMemberString == null)
                {
                    return new ExpressionDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<ExpressionDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == "heading")
            {
                if (dataMemberString == null)
                {
                    return new HeadingDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<HeadingDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == "hyperlink")
            {
                if (dataMemberString == null)
                {
                    return new HyperlinkDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<HyperlinkDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == "iframe")
            {
                if (dataMemberString == null)
                {
                    return new IFrameDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<IFrameDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == "challengeiframe")
            {
                if (dataMemberString == null)
                {
                    return new ChallengeIFrameDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<ChallengeIFrameDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == "subheading")
            {
                if (dataMemberString == null)
                {
                    return new SubheadingDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<SubheadingDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == "text")
            {
                if (dataMemberString == null)
                {
                    return new TextDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<TextDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == "title")
            {
                if (dataMemberString == null)
                {
                    return new TitleDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<TitleDisplayHint>(dataMemberString);
                }
            }

            return null;
        }

        private static DisplayHint GetDisplayHint(string displayType, string dataMemberString = null)
        {
            if (displayType == HintType.Property.ToString().ToLower())
            {
                if (dataMemberString == null)
                {
                    return new PropertyDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<PropertyDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == HintType.Logo.ToString().ToLower())
            {
                if (dataMemberString == null)
                {
                    return new LogoDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<LogoDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == HintType.Audio.ToString().ToLower())
            {
                return JsonConvert.DeserializeObject<AudioDisplayHint>(dataMemberString);
            }
            else if (displayType == HintType.Image.ToString().ToLower())
            {
                if (dataMemberString == null)
                {
                    return new ImageDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<ImageDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == HintType.PidlContainer.ToString().ToLower())
            {
                if (dataMemberString == null)
                {
                    return new PidlContainerDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<PidlContainerDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == HintType.PrefillControl.ToString().ToLower())
            {
                if (dataMemberString == null)
                {
                    return new PrefillControlDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<PrefillControlDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == HintType.SecureProperty.ToString().ToLower())
            {
                return JsonConvert.DeserializeObject<SecurePropertyDisplayHint>(dataMemberString);
            }
            else if (displayType == HintType.Separator.ToString().ToLower())
            {
                if (dataMemberString == null)
                {
                    return new SeparatorDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<SeparatorDisplayHint>(dataMemberString);
                }
            }
            else if (displayType == HintType.Spinner.ToString().ToLower())
            {
                return JsonConvert.DeserializeObject<SpinnerDisplayHint>(dataMemberString);
            }
            else if (displayType == HintType.WebView.ToString().ToLower())
            {
                if (dataMemberString == null)
                {
                    return new WebViewDisplayHint();
                }
                else
                {
                    return JsonConvert.DeserializeObject<WebViewDisplayHint>(dataMemberString);
                }
            }
            else
            {
                return GetContentDisplayHint(displayType, dataMemberString);
            }
        }

        private static List<PageDisplayHint> PopulatePageDisplayHint(dynamic displayHintTemplate, List<PageDisplayHint> displayHints)
        {
            foreach (dynamic member in displayHintTemplate)
            {
                string memberString = Convert.ToString(member);
                var displayHint = JsonConvert.DeserializeObject<PageDisplayHint>(memberString);
                if (member.members != null && member.members.Count > 0)
                {
                    displayHint?.Members?.Clear();
                    foreach (dynamic memberItem in member.members)
                    {
                        string displayType = Convert.ToString(memberItem.displayType);
                        var grContainerDisplayHint = GetContainerDisplayHint(displayType);
                        if (grContainerDisplayHint == null)
                        {
                            var grContentDisplayHint = GetContentDisplayHint(displayType, Convert.ToString(memberItem));
                            if (grContentDisplayHint == null)
                            {
                                var grDisplayHint = GetDisplayHint(displayType, Convert.ToString(memberItem));
                                var data = PopulateGenericDisplayHint<DisplayHint>(memberItem, grDisplayHint);
                                if (data != null)
                                {
                                    displayHint?.AddDisplayHint(data);
                                }
                            }
                            else
                            {
                                var data = PopulateGenericContentDisplayHint<ContentDisplayHint>(memberItem, grContentDisplayHint);
                                if (data != null)
                                {
                                    displayHint?.AddDisplayHint(data);
                                }
                            }
                        }
                        else
                        {
                            if (displayType == HintType.Captcha.ToString().ToLower())
                            {
                                var captchaContainerDisplayHint = JsonConvert.DeserializeObject<CaptchaDisplayHint>(Convert.ToString(memberItem));
                                var data = SetCaptchaDisplayHint<CaptchaDisplayHint>(captchaContainerDisplayHint, memberItem);
                                if (data != null)
                                {
                                    displayHint?.AddDisplayHint(data);
                                }
                            }
                            else
                            {
                                var data = PopulateGenericContainerDisplayHint<ContainerDisplayHint>(memberItem, grContainerDisplayHint);
                                if (data != null)
                                {
                                    displayHint?.AddDisplayHint(data);
                                }
                            }
                        }
                    }
                }

                if (displayHint != null)
                {
                    displayHints.Add(displayHint);
                }
            }

            return displayHints;
        }

        private static T SetCaptchaDisplayHint<T>(T displayHint, dynamic members) where T : CaptchaDisplayHint
        {
            string memberString = Convert.ToString(members);
            dynamic dataMember = JsonConvert.DeserializeObject(memberString);

            string displayType = Convert.ToString(members.displayType);

            if ((members.members != null && members.members.Count > 0)
               || (members.imageMembers != null && members.imageMembers.Count > 0)
               || (members.audioMembers != null && members.audioMembers.Count > 0))
            {
                if (members.members != null && members.members.Count > 0)
                {
                    dataMember.members = null;
                    string dataMemberString = Convert.ToString(dataMember);

                    var data = PopulateGenericContainerDisplayHint<ContainerDisplayHint>(members.members, GetContainerDisplayHint(displayType, dataMemberString));

                    if (data != null)
                    {
                        if (displayHint.HintId != null)
                        {
                            displayHint.AddDisplayHint(data);
                        }
                        else
                        {
                            displayHint = data;
                        }
                    }

                    SetDisplayHint(displayHint, members, displayType, dataMemberString);
                }

                // Only Applicable for Captcha
                if (members.imageMembers != null && members.imageMembers.Count > 0)
                {
                    dynamic childCaptchaMembers = members.imageMembers;
                    var memberList = new List<DisplayHint>();
                    if (childCaptchaMembers != null)
                    {
                        if (childCaptchaMembers.Count > 0)
                        {
                            foreach (dynamic childMember in childCaptchaMembers)
                            {
                                string memberDataString = Convert.ToString(childMember);
                                dynamic dataMemberObj = JsonConvert.DeserializeObject(memberDataString);
                                string childDisplayType = Convert.ToString(childMember.displayType);
                                if (childDisplayType == HintType.Group.ToString().ToLower())
                                {
                                    dataMemberObj.members = null;
                                    string dataMemberStr = Convert.ToString(dataMemberObj);

                                    var data = PopulateGenericContainerDisplayHint<ContainerDisplayHint>(childMember.members, GetContainerDisplayHint(childDisplayType, dataMemberStr));
                                    if (data != null)
                                    {
                                        memberList.Add(data);
                                    }
                                }
                                else
                                {
                                    SetCaptchaMemberDisplayHint(memberList, childMember);
                                }
                            }
                        }
                    }

                    if (memberList.Count > 0)
                    {
                        if (displayHint?.ImageMembers != null && displayHint?.ImageMembers?.Count() > 0)
                        {
                            displayHint.ImageMembers.Clear();
                        }

                        displayHint.AddImageDisplayHints(memberList);
                    }
                }

                if (members.audioMembers != null && members.audioMembers.Count > 0)
                {
                    dynamic childCaptchaMembers = members.audioMembers;
                    var memberList = new List<DisplayHint>();
                    if (childCaptchaMembers != null)
                    {
                        if (childCaptchaMembers.Count > 0)
                        {
                            foreach (dynamic childMember in childCaptchaMembers)
                            {
                                string memberDataString = Convert.ToString(childMember);
                                dynamic dataMemberObj = JsonConvert.DeserializeObject(memberDataString);
                                string childDisplayType = Convert.ToString(childMember.displayType);
                                if (childDisplayType == HintType.Group.ToString().ToLower())
                                {
                                    dataMemberObj.members = null;
                                    string dataMemberStr = Convert.ToString(dataMemberObj);
                                    var data = PopulateGenericContainerDisplayHint<ContainerDisplayHint>(childMember.members, GetContainerDisplayHint(childDisplayType, dataMemberStr));
                                    if (data != null)
                                    {
                                        memberList.Add(data);
                                    }
                                }
                                else
                                {
                                    SetCaptchaMemberDisplayHint(memberList, childMember);
                                }
                            }
                        }
                    }

                    if (memberList.Count > 0)
                    {
                        if (displayHint?.AudioMembers != null && displayHint?.AudioMembers?.Count() > 0)
                        {
                            displayHint.AudioMembers.Clear();
                        }

                        displayHint.AddAudioDisplayHints(memberList);
                    }
                }
            }
            else
            {
                string dataMemberString = Convert.ToString(dataMember);
                SetDisplayHint(displayHint, members, displayType, dataMemberString);
            }

            return displayHint;
        }

        private static void SetCaptchaMemberDisplayHint(List<DisplayHint> displayHints, dynamic member)
        {
            string memberString = Convert.ToString(member);
            dynamic dataMember = JsonConvert.DeserializeObject(memberString);
            dataMember.members = null;
            string dataMemberString = Convert.ToString(dataMember);
            string displayType = Convert.ToString(member.displayType);

            var displayHintObject = GetContentDisplayHint(displayType, dataMemberString) ?? GetDisplayHint(displayType, dataMemberString);

            if (displayHintObject != null)
            {
                if (member.displayHelp != null)
                {
                    var grDisplayHint = new List<DisplayHint>();
                    foreach (dynamic help in member.displayHelp)
                    {
                        string displayHelp = Convert.ToString(help);
                        var dHelp = GetDisplayHint(Convert.ToString(help.displayType), displayHelp);
                        if (dHelp != null)
                        {
                            grDisplayHint.Add(dHelp);
                        }
                    }

                    displayHintObject.HelpDisplayDescriptions = grDisplayHint;
                }

                displayHints.Add(displayHintObject);
            }
        }

        private static void SetDisplayHint<T>(T displayHint, dynamic members, string displayType, string dataMemberString) where T : ContainerDisplayHint
        {
            var displayHintObject = GetDisplayHint(displayType, dataMemberString) ?? GetContentDisplayHint(displayType, dataMemberString);

            if (displayHintObject != null)
            {
                if (members.displayHelp != null)
                {
                    var grDisplayHint = new List<DisplayHint>();
                    foreach (dynamic help in members.displayHelp)
                    {
                        string displayHelp = Convert.ToString(help);
                        var dHelp = GetDisplayHint(Convert.ToString(help.displayType), displayHelp);
                        if (dHelp != null)
                        {
                            grDisplayHint.Add(dHelp);
                        }
                    }

                    displayHintObject.HelpDisplayDescriptions = grDisplayHint;
                }

                displayHint.AddDisplayHint(displayHintObject);
            }
        }

        private static T PopulateGenericContainerDisplayHint<T>(dynamic members, T displayHint) where T : ContainerDisplayHint
        {
            if (members != null)
            {
                if (members.Count > 0)
                {
                    int count = 0;
                    foreach (dynamic member in members)
                    {
                        PIDLResource.SetContainerDisplayHint<T>(displayHint, member);
                        count++;
                        if (count == members.Count)
                        {
                            return displayHint;
                        }
                    }
                }
                else
                {
                    return PIDLResource.SetContainerDisplayHint<T>(displayHint, members);
                }
            }

            return null;
        }

        private static T SetContainerDisplayHint<T>(T displayHint, dynamic members) where T : ContainerDisplayHint
        {
            string memberString = Convert.ToString(members);
            dynamic dataMember = JsonConvert.DeserializeObject(memberString);
            dataMember.members = null;
            string dataMemberString = Convert.ToString(dataMember);
            string displayType = Convert.ToString(members.displayType);
            if (members.members != null)
            {
                var data = PIDLResource.PopulateGenericContainerDisplayHint<ContainerDisplayHint>(members.members, GetContainerDisplayHint(displayType, dataMemberString));
                if (data != null)
                {
                    if (displayHint.HintId != null)
                    {
                        displayHint.AddDisplayHint(data);
                    }
                    else
                    {
                        displayHint = data;
                    }
                }
            }

            SetDisplayHint(displayHint, members, displayType, dataMemberString);

            return displayHint;
        }

        private static T PopulateGenericContentDisplayHint<T>(dynamic members, T displayHint) where T : ContentDisplayHint
        {
            if (members != null)
            {
                if (members.Count > 0)
                {
                    int count = 0;
                    foreach (dynamic member in members)
                    {
                        PIDLResource.SetContentDisplayHint<T>(displayHint, member);
                        count++;
                        if (count == members.Count)
                        {
                            return displayHint;
                        }
                    }
                }
                else
                {
                    return PIDLResource.SetContentDisplayHint<T>(displayHint, members);
                }
            }

            return null;
        }

        private static T SetContentDisplayHint<T>(T displayHint, dynamic members) where T : ContentDisplayHint
        {
            string memberString = Convert.ToString(members);
            dynamic dataMember = JsonConvert.DeserializeObject(memberString);
            dataMember.members = null;
            string dataMemberString = Convert.ToString(dataMember);
            string displayType = Convert.ToString(members.displayType);

            var displayHintObject = GetContentDisplayHint(displayType, dataMemberString) ?? GetDisplayHint(displayType, dataMemberString);

            if (displayHintObject != null)
            {
                if (members.displayHelp != null)
                {
                    var grDisplayHint = new List<DisplayHint>();
                    foreach (dynamic help in members.displayHelp)
                    {
                        string displayHelp = Convert.ToString(help);
                        var dHelp = GetDisplayHint(Convert.ToString(help.displayType), displayHelp);
                        if (dHelp != null)
                        {
                            grDisplayHint.Add(dHelp);
                        }
                    }

                    displayHintObject.HelpDisplayDescriptions = grDisplayHint;
                }

                displayHint = (T)displayHintObject;
            }

            return displayHint;
        }

        private static T PopulateGenericDisplayHint<T>(dynamic members, T displayHint) where T : DisplayHint
        {
            if (members != null)
            {
                if (members.Count > 0)
                {
                    int count = 0;
                    foreach (dynamic member in members)
                    {
                        PIDLResource.SetDisplayHint<T>(displayHint, member);
                        count++;
                        if (count == members.Count)
                        {
                            return displayHint;
                        }
                    }
                }
                else
                {
                    return PIDLResource.SetDisplayHint<T>(displayHint, members);
                }
            }

            return null;
        }

        private static T SetDisplayHint<T>(T displayHint, dynamic members) where T : DisplayHint
        {
            string memberString = Convert.ToString(members);
            dynamic dataMember = JsonConvert.DeserializeObject(memberString);
            dataMember.members = null;
            string dataMemberString = Convert.ToString(dataMember);
            string displayType = Convert.ToString(members.displayType);

            var displayHintObject = GetDisplayHint(displayType, dataMemberString) ?? GetContentDisplayHint(displayType, dataMemberString);

            if (displayHintObject != null)
            {
                if (members.displayHelp != null)
                {
                    var grDisplayHint = new List<DisplayHint>();
                    foreach (dynamic help in members.displayHelp)
                    {
                        string displayHelp = Convert.ToString(help);
                        var dHelp = GetDisplayHint(Convert.ToString(help.displayType), displayHelp);
                        if (dHelp != null)
                        {
                            grDisplayHint.Add(dHelp);
                        }
                    }

                    displayHintObject.HelpDisplayDescriptions = grDisplayHint;
                }

                displayHint = (T)displayHintObject;
            }

            return displayHint;
        }

        private static string GetNewHeadervalue(string existingHeaderValue, string additionalHeaderValue)
        {
            string newFlightValue = null;

            if (string.IsNullOrWhiteSpace(existingHeaderValue))
            {
                if (!string.IsNullOrWhiteSpace(additionalHeaderValue))
                {
                    newFlightValue = additionalHeaderValue;
                }
            }
            else if (string.IsNullOrWhiteSpace(additionalHeaderValue)
                || existingHeaderValue.Split(new char[] { ',' }).Contains(additionalHeaderValue, StringComparer.OrdinalIgnoreCase))
            {
                newFlightValue = existingHeaderValue;
            }
            else
            {
                newFlightValue = string.Join(",", existingHeaderValue, additionalHeaderValue);
            }

            return newFlightValue;
        }

        private static string GetPayloadKey(string descriptionType, string key)
        {
            if (Constants.DescriptionTypesWithDirectName.Contains(descriptionType) || descriptionType == null)
            {
                return key;
            }
            else
            {
                return string.Format("{0}{1}", descriptionType, key.First().ToString().ToUpper() + key.Substring(1));
            }
        }

        private IEnumerable<PropertyDescription> GetPropertyDescriptions()
        {
            foreach (object dataDescription in this.DataDescription.Values ?? Enumerable.Empty<object>())
            {
                if (dataDescription is PropertyDescription)
                {
                    PropertyDescription propertyDescription = dataDescription as PropertyDescription;
                    yield return propertyDescription;
                }
                else if (dataDescription is List<PIDLResource>)
                {
                    List<PIDLResource> pidlResourceList = dataDescription as List<PIDLResource>;
                    foreach (PIDLResource pidlResource in pidlResourceList ?? Enumerable.Empty<PIDLResource>())
                    {
                        foreach (PropertyDescription innerPropertyDescription in pidlResource.GetPropertyDescriptions() ?? Enumerable.Empty<PropertyDescription>())
                        {
                            yield return innerPropertyDescription;
                        }
                    }
                }
            }
        }

        private IEnumerable<Tuple<DisplayHint, ContainerDisplayHint>> GetDisplayHintsFromContainer(ContainerDisplayHint container)
        {
            foreach (DisplayHint member in container.Members ?? Enumerable.Empty<DisplayHint>())
            {
                ContainerDisplayHint innerContainer = member as ContainerDisplayHint;
                if (innerContainer != null)
                {
                    foreach (Tuple<DisplayHint, ContainerDisplayHint> containerMember in this.GetDisplayHintsFromContainer(innerContainer) ?? Enumerable.Empty<Tuple<DisplayHint, ContainerDisplayHint>>())
                    {
                        yield return containerMember;
                    }
                }
                else
                {
                    yield return new Tuple<DisplayHint, ContainerDisplayHint>(member, container);
                }
            }
        }

        private ContainerDisplayHint GetContainerDisplayHintFromContainer(ContainerDisplayHint container, string hintId)
        {
            ContainerDisplayHint retVal = null;

            foreach (DisplayHint hint in container.Members)
            {
                var containerHint = hint as ContainerDisplayHint;
                if (containerHint != null)
                {
                    if (containerHint.HintId.Equals(hintId))
                    {
                        return containerHint;
                    }

                    retVal = this.GetContainerDisplayHintFromContainer(containerHint, hintId);
                    if (retVal != null)
                    {
                        return retVal;
                    }
                }
            }

            return retVal;
        }

        private DisplayHint GetDisplayHintFromContainerByPropertyName(ContainerDisplayHint container, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            DisplayHint retVal = null;

            foreach (DisplayHint hint in container.Members)
            {
                if (propertyName.Equals(hint.PropertyName))
                {
                    return hint;
                }

                var containerHint = hint as ContainerDisplayHint;
                if (containerHint != null)
                {
                    retVal = this.GetDisplayHintFromContainerByPropertyName(containerHint, propertyName);
                    if (retVal != null)
                    {
                        return retVal;
                    }
                }
            }

            return retVal;
        }

        private void RemoveEmptyPidlContainerHints(ContainerDisplayHint container)
        {
            List<DisplayHint> pidlContainers = container.Members.FindAll(x => x is PidlContainerDisplayHint);
            foreach (PidlContainerDisplayHint hint in pidlContainers)
            {
                if (hint.LinkedPidlIdentity == null)
                {
                    container.Members.Remove(hint);
                }
            }

            foreach (ContainerDisplayHint hint in container.Members.FindAll(x => x is ContainerDisplayHint))
            {
                this.RemoveEmptyPidlContainerHints(hint);
            }
        }
    }
}