// <copyright file="PIDLResource.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Newtonsoft.Json;

    public class PIDLResource
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(Order = 0, PropertyName = "identity")]
        public Dictionary<string, string> Identity { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(Order = 1, PropertyName = "data_description")]
        public Dictionary<string, object> DataDescription { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(Order = 2, PropertyName = "dataSources")]
        public Dictionary<string, DataSource> DataSources { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(Order = 3, PropertyName = "displayDescription")]
        public List<PageDisplayHint> DisplayPages { get; set; }

        [JsonProperty(Order = 4, PropertyName = "strings")]
        public PidlResourceStrings PidlResourceStrings { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(Order = 5, PropertyName = "links")]
        public Dictionary<string, RestLink> Links { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(Order = 6, PropertyName = "clientContext")]
        public Dictionary<string, object> ClientContext { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(Order = 7, PropertyName = "linkedPidls")]
        public List<PIDLResource> LinkedPidls { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be read/write for serialization.")]
        [JsonProperty(Order = 8, PropertyName = "scenarioContext")]
        public Dictionary<string, string> ScenarioContext { get; set; }

        [JsonProperty(Order = 9, PropertyName = "clientAction")]
        public ClientAction ClientAction { get; set; }

        [JsonProperty(Order = 10, PropertyName = "initializeContext")]
        public InitializeContext InitializeContext { get; set; }

        [JsonProperty(Order = 11, PropertyName = "pidlInstanceContexts")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, ResourceActionContext> PIDLInstanceContexts { get; set; }

        [JsonProperty(Order = 12, PropertyName = "clientSettings")]
        public Dictionary<string, object> ClientSettings { get; set; }

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

        public PropertyDescription GetPropertyDescriptionByPropertyName(string key)
        {
            PropertyDescription retVal = null;
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

        public DisplayHint GetDisplayHintById(string hintId)
        {
            DisplayHint retVal = null;

            foreach (PageDisplayHint page in this.DisplayPages)
            {
                retVal = this.GetDisplayHintFromContainer(page, hintId);
                if (retVal != null)
                {
                    return retVal;
                }
            }

            return retVal;
        }

        public List<DisplayHint> GetAllDisplayHintsOfId(string hintId)
        {
            List<DisplayHint> retList = new List<DisplayHint>();
            foreach (PageDisplayHint page in this.DisplayPages)
            {
                DisplayHint retVal = this.GetDisplayHintFromContainer(page, hintId);
                if (retVal != null)
                {
                    retList.Add(retVal);
                }
            }

            return retList;
        }

        public List<DisplayHint> GetAllDisplayHints(ContainerDisplayHint container)
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

        public List<DisplayHint> GetAllDisplayHints()
        {
            List<DisplayHint> displayHints = new List<DisplayHint>();

            if (this.DisplayPages != null)
            {
                foreach (PageDisplayHint page in this.DisplayPages)
                {
                    List<DisplayHint> pageDisplayHints = this.GetAllDisplayHints(page);
                    displayHints = displayHints.Concat(pageDisplayHints).ToList();
                }
            }

            return displayHints;
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

        private DisplayHint GetDisplayHintFromCaptchaContainer(CaptchaDisplayHint container, string hintId)
        {
            DisplayHint retVal = null;

            foreach (DisplayHint hint in container.AudioMembers)
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

            foreach (DisplayHint hint in container.ImageMembers)
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

        private DisplayHint GetDisplayHintFromContainer(ContainerDisplayHint container, string hintId)
        {
            DisplayHint retVal = null;

            foreach (DisplayHint hint in container.Members)
            {
                if (hint.HintId.Equals(hintId))
                {
                    return hint;
                }

                var containerHint = hint as ContainerDisplayHint;
                if (containerHint != null)
                {
                    var captchaContainerhint = hint as CaptchaDisplayHint;
                    if (captchaContainerhint != null)
                    {
                        retVal = this.GetDisplayHintFromCaptchaContainer(captchaContainerhint, hintId);
                    }
                    else
                    {
                        retVal = this.GetDisplayHintFromContainer(containerHint, hintId);
                    }
            
                    if (retVal != null)
                    {
                        return retVal;
                    }
                }
            }

            return retVal;
        }
    }
}
