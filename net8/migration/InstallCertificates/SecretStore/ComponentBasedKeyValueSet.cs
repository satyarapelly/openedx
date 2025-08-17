// <copyright file="ComponentBasedKeyValueSet.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tools.InstallCertificates
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    
    public class ComponentBasedKeyValueSet
    {
        private const string KeyFormat = "C:{0}:K:{1}";

        private const string ComponentNodeName = "Component";

        private const string ComponentAttributeName = "name";

        private const string ItemNodeName = "Item";

        private const string KeyAttributeName = "key";

        private const string ValueAttributeName = "value";

        private Dictionary<string, string> items = null;

        private ComponentBasedKeyValueSet()
        {
            this.items = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public static ComponentBasedKeyValueSet Deserialize(string serializedData)
        {
            ComponentBasedKeyValueSet keyValueCollection = new ComponentBasedKeyValueSet();
            XElement keyValueXml = XElement.Parse(serializedData, LoadOptions.None);
            foreach (XElement component in keyValueXml.Elements(ComponentBasedKeyValueSet.ComponentNodeName))
            {
                XAttribute componentNameAttribute = component.Attribute(ComponentBasedKeyValueSet.ComponentAttributeName);
                if (componentNameAttribute != null && !string.IsNullOrEmpty(componentNameAttribute.Value))
                {
                    foreach (XElement item in component.Elements(ComponentBasedKeyValueSet.ItemNodeName))
                    {
                        XAttribute keyAttribute = item.Attribute(ComponentBasedKeyValueSet.KeyAttributeName);
                        XAttribute valueAttribute = item.Attribute(ComponentBasedKeyValueSet.ValueAttributeName);

                        if (keyAttribute != null &&
                            !string.IsNullOrEmpty(keyAttribute.Value) &&
                            valueAttribute != null &&
                            !string.IsNullOrEmpty(valueAttribute.Value))
                        {
                            string dictionaryKey = string.Format(ComponentBasedKeyValueSet.KeyFormat, componentNameAttribute.Value, keyAttribute.Value);
                            keyValueCollection.items.Add(dictionaryKey, valueAttribute.Value);
                        }
                    }
                }
            }
            
            return keyValueCollection;
        }

        public bool TryGetValue(string componentName, string key, out string value)
        {
            if (this.items == null)
            {
                value = null;
                return false;
            }

            string dictionaryKey = string.Format(ComponentBasedKeyValueSet.KeyFormat, componentName, key);
            return this.items.TryGetValue(dictionaryKey, out value);
        }
    }
}
