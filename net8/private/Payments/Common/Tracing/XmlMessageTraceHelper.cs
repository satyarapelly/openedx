// <copyright file="XmlMessageTraceHelper.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public static class XmlMessageTraceHelper
    {
        private static ConcurrentDictionary<Type, XmlSerializer> xmlAttributeOverridesCache = new ConcurrentDictionary<Type, XmlSerializer>();

        public static string XmlSerializeForTracing(object message, EventTraceActivity traceActivityId)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    XmlSerializer xs = GetXmlMessageTraceSerializer(message.GetType());
                    using (XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8))
                    {
                        xs.Serialize((XmlWriter)xmlTextWriter, message);
                        return Encoding.UTF8.GetString(memoryStream.ToArray());
                    }
                }
            }
            catch (Exception)
            {
            }

            return message.ToString();
        }

        private static XmlSerializer GetXmlMessageTraceSerializer(Type type)
        {
            XmlSerializer serializer;
            if (!xmlAttributeOverridesCache.TryGetValue(type, out serializer))
            {
                XmlAttributeOverrides overrides;
                overrides = new XmlAttributeOverrides();
                HashSet<Type> exploredTypes = new HashSet<Type>();
                Queue<Type> typesToExplore = new Queue<Type>();
                typesToExplore.Enqueue(type);
                exploredTypes.Add(type);
                while (typesToExplore.Count > 0)
                {
                    Type typeToExplore = typesToExplore.Dequeue();

                    foreach (PropertyInfo propertyInfo in typeToExplore.GetProperties())
                    {
                        LookupSensitiveAttributes(typesToExplore, overrides, exploredTypes, typeToExplore, propertyInfo);
                    }

                    foreach (FieldInfo propertyInfo in typeToExplore.GetFields())
                    {
                        LookupSensitiveAttributes(typesToExplore, overrides, exploredTypes, typeToExplore, propertyInfo);
                    }
                }

                serializer = new XmlSerializer(type, overrides);
                xmlAttributeOverridesCache.TryAdd(type, serializer);
            }

            return serializer;
        }

        private static void LookupSensitiveAttributes(Queue<Type> typesToExplore, XmlAttributeOverrides overrides, HashSet<Type> exploredTypes, Type typeToExplore, MemberInfo memberInfo)
        {
            Debug.Assert(memberInfo.MemberType == MemberTypes.Property || memberInfo.MemberType == MemberTypes.Field, "Members should only be fields or properties");

            SensitiveDataAttribute attribute = memberInfo.GetCustomAttribute<SensitiveDataAttribute>();
            if (attribute != null)
            {
                overrides.Add(typeToExplore, memberInfo.Name, new XmlAttributes { XmlIgnore = true });
            }

            Type memberType = (memberInfo.MemberType == MemberTypes.Property) ? ((PropertyInfo)memberInfo).PropertyType : ((FieldInfo)memberInfo).FieldType;

            if (!memberType.IsPrimitive && !exploredTypes.Contains(memberType))
            {
                typesToExplore.Enqueue(memberType);
                exploredTypes.Add(memberType);
            }
        }
    }
}
