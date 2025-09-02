// <copyright file="PIDLData.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using Newtonsoft.Json.Linq;

    // This type is necessary since deserializing converters are applied based on 
    // type of the target object being requested.  Having a type of PIDLData (class below)
    // makes it clear to the deserializing converter that the payload is infact PIDL data
    // and not just any Dictionary<string, object>
    [SuppressMessage("Microsoft.Naming", "CA1710", Justification = "Needs to be this name to strongly type the input values")]
    [Serializable]
    public class PIDLData : Dictionary<string, object>
    {
        public PIDLData()
            : base()
        {
        }

        protected PIDLData(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public void RenameProperty(string oldPropertyPath, string newPorpertyName)
        {
            JObject target = null;
            string[] paths = oldPropertyPath.Split(new char[] { '.' });

            // when oldProperty is a root level property      
            if (paths.Length == 1)
            {
                object value = null;
                if (this.TryGetValue(oldPropertyPath, out value))
                {
                    this.Add(newPorpertyName, value);
                    this.Remove(oldPropertyPath);
                }
            }
            else
            {
                for (int i = 0; i < paths.Length - 1; i++)
                {
                    target = (i == 0) ? this[paths[i]] as JObject : target[paths[i]] as JObject;
                    if (target == null)
                    {
                        throw new ArgumentException("Property path is not valid");
                    }
                }

                // For now, the only scenario for TryGetValue returns a null value is address line 2
                // In this case, don't add it to the payload.
                JToken value = null;
                string oldPropertyName = paths[paths.Length - 1];
                target.TryGetValue(oldPropertyName, out value);
                if (value != null)
                {
                    target.Add(newPorpertyName, value);
                    target.Remove(oldPropertyName);
                }
            }
        }

        public string TryGetPropertyValue(string propertyPath)
        {
            if (string.IsNullOrWhiteSpace(propertyPath))
            {
                return null;
            }

            try
            {
                string[] paths = propertyPath.Split(new char[] { '.' });
                
                // Consider the case when propertyPath is a root level property
                if (paths.Length == 1)
                {
                    object propertyValue = null;
                    return this.TryGetValue(propertyPath, out propertyValue) ? propertyValue.ToString() : null;
                }

                // Retrieve the parent object
                object parentObject;
                if (!this.TryGetValue(paths[0], out parentObject))
                {
                    return null;
                }

                string subpath = string.Join(".", paths, 1, paths.Length - 1);
                JToken value = JObject.FromObject(parentObject).SelectToken(subpath);

                // The property in the propertyPath might not be present because it is optional, example: address line 2
                return value == null ? null : value.ToString();
            }
            catch (Exception)
            {
                // Eating up the exception, as retrieving the value is a best effort
            }

            return null;
        }

        public bool ContainsProperty(string propertyPath)
        {
            if (string.IsNullOrWhiteSpace(propertyPath))
            {
                return false;
            }

            try
            {
                string[] paths = propertyPath.Split(new char[] { '.' });

                // Consider the case when propertyPath is a root level property
                if (paths.Length == 1)
                {
                    return this.ContainsKey(propertyPath);
                }

                // Retrieve the parent object
                object parentObject;
                if (!this.TryGetValue(paths[0], out parentObject))
                {
                    return false;
                }

                string subpath = string.Join(".", paths, 1, paths.Length - 1);
                JToken value = JObject.FromObject(parentObject).SelectToken(subpath);

                // The property in the propertyPath might not be present because it is optional, example: address line 2
                return value != null;
            }
            catch (Exception)
            {
                // Eating up the exception, as retrieving the value is a best effort
            }

            return false;
        }

        public bool TrySetProperty(string propertyPath, string value)
        {
            try
            {
                JObject target = null;
                string[] paths = propertyPath.Split(new char[] { '.' });

                // when oldProperty is a root level property
                if (paths.Length == 1)
                {
                    object val = null;
                    if (this.TryGetValue(propertyPath, out val) && value != null)
                    {
                        this.Remove(propertyPath);
                        this.Add(propertyPath, value);
                    }
                }
                else
                {
                    for (int i = 0; i < paths.Length - 1; i++)
                    {
                        target = (i == 0) ? this[paths[i]] as JObject : target[paths[i]] as JObject;
                        if (target == null)
                        {
                            throw new ArgumentException("Property path is not valid");
                        }
                    }

                    string propertyName = paths[paths.Length - 1];
                    if (value != null)
                    {
                        target.Remove(propertyName);
                        target.Add(propertyName, value);
                    }
                }

                return true;
            }
            catch
            {
                // Eating up the exception, as setting the value is a best effort
            }

            return false;
        }

        public bool TryRemoveProperty(string propertyPath)
        {
            try
            {
                JObject target = null;
                string[] paths = propertyPath.Split(new char[] { '.' });

                // when oldProperty is a root level property
                if (paths.Length == 1)
                {
                    object val = null;
                    if (this.TryGetValue(propertyPath, out val))
                    {
                        return this.Remove(propertyPath);
                    }
                }
                else
                {
                    for (int i = 0; i < paths.Length - 1; i++)
                    {
                        target = (i == 0) ? this[paths[i]] as JObject : target[paths[i]] as JObject;
                        if (target == null)
                        {
                            throw new ArgumentException("Property path is not valid");
                        }
                    }

                    string propertyName = paths[paths.Length - 1];
                    return target.Remove(propertyName);
                }

                return true;
            }
            catch
            {
                // Eating up the exception, as setting the value is a best effort
            }

            return false;
        }

        public string TryGetPropertyValueFromPIDLData(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }

            if (this.ContainsKey(propertyName))
            {
                object propertyValue = null;
                return this.TryGetValue(propertyName, out propertyValue) ? propertyValue?.ToString() : null;
            }

            return null;
        }
    }
}