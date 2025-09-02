// <copyright file="PIDLDataDeserializer.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class PIDLDataDeserializer : JsonConverter
    {
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(PIDLData).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            PIDLData pidlData = new PIDLData();
            serializer.Populate(reader, pidlData);

            Dictionary<string, object> typeInfo = GetTypeInfo(pidlData);
            this.ValidateTypeConformance(pidlData, typeInfo, serializer);

            return pidlData;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private static Dictionary<string, object> GetTypeInfo(PIDLData retObject)
        {
            List<PIDLResource> pidl = null;
            try
            {
                if (retObject.ContainsKey("address_type"))
                {
                    string country = (string)retObject["address_country"];
                    string type = (string)retObject["address_type"];
                    pidl = PIDLResourceFactory.Instance.GetAddressDescriptions(country, type, GlobalConstants.Defaults.Language);
                }
                else if (retObject.ContainsKey("payment_method_type"))
                {
                    string country = (string)retObject["payment_method_country"];
                    string family = (string)retObject["payment_method_family"];
                    string type = (string)retObject["payment_method_type"];
                    pidl = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(null, country, family, "xbox", type, GlobalConstants.Defaults.Language);
                }
                else if (retObject.ContainsKey("profile_type"))
                {
                    string country = (string)retObject["profile_country"];
                    string operation = (string)retObject["profile_operation"];
                    string type = (string)retObject["profile_type"];
                    pidl = PIDLResourceFactory.Instance.GetProfileDescriptions(country, type, operation, GlobalConstants.Defaults.Language);
                }
                else if (retObject.ContainsKey("challenge_type"))
                {
                    string type = (string)retObject["challenge_type"];
                    pidl = PIDLResourceFactory.Instance.GetChallengeDescriptions(type, GlobalConstants.Defaults.Language);
                }
                else if (retObject.ContainsKey("data_type"))
                {
                    string type = (string)retObject["data_type"];
                    string country = (string)retObject["data_country"];
                    pidl = PIDLResourceFactory.Instance.GetMiscellaneousDescriptions(country, type, GlobalConstants.Defaults.Language);
                }
                else
                {
                    throw new JsonSerializationException("Data being deserialized is not one of the supported PIDL types");
                }
            }
            catch (KeyNotFoundException keyEx)
            {
                throw new JsonSerializationException("Expected key was not found", keyEx);
            }
            catch (PIDLException pidlEx)
            {
                throw new JsonSerializationException("Could not get PIDL for the data being deserialized", pidlEx);
            }

            // Get<DataType>Descriptions call returns a list with more than one element only when the query 
            // parameters are not specific (e.g. when family = credit_card and type is not specified)
            // However, that is not the case here and the list will be of size 1.
            return pidl[0].DataDescription;
        }

        private void ValidateTypeConformance(PIDLData pidlData, Dictionary<string, object> typeInfo, JsonSerializer serializer)
        {
            StringBuilder unknownFields = new StringBuilder();
            foreach (string fieldName in pidlData.Keys)
            {
                if (!typeInfo.ContainsKey(fieldName))
                {
                    unknownFields.Append(fieldName).Append(" ");
                }
            }

            if (unknownFields.Length > 0)
            {
                throw new JsonSerializationException(
                    string.Format("PIDLData has unknown field/s: {0}", unknownFields.ToString()));
            }

            foreach (string fieldName in typeInfo.Keys)
            {
                PropertyDescription fieldInfo = typeInfo[fieldName] as PropertyDescription;
                if (fieldInfo != null)
                {
                    if (pidlData.ContainsKey(fieldName))
                    {
                        string fieldValue = pidlData[fieldName].ToString();
                        if (fieldInfo.PropertyType == "number" && string.IsNullOrWhiteSpace(fieldInfo.TokenSet))
                        {
                            int num = 0;
                            if (!int.TryParse(fieldValue, out num))
                            {
                                throw new JsonSerializationException(
                                    string.Format("PIDLData field \"{0}\" has value \"{1}\" which is not a number", fieldName, fieldValue));
                            }
                        }

                        if (fieldInfo.Validation != null && string.IsNullOrWhiteSpace(fieldInfo.TokenSet))
                        {
                            Regex validator = new Regex(fieldInfo.Validation.Regex);
                            if (!validator.IsMatch(fieldValue))
                            {
                                throw new JsonSerializationException(
                                    string.Format("PIDLData field \"{0}\" has value \"{1}\" which does not match validation regex \"{2}\"", fieldName, fieldValue, fieldInfo.Validation.Regex));
                            }
                        }

                        if (fieldInfo.PossibleValues != null)
                        {
                            if (!fieldInfo.PossibleValues.ContainsKey(fieldValue))
                            {
                                throw new JsonSerializationException(
                                    string.Format("PIDLData field \"{0}\" has value \"{1}\" which is not one of the allowed values", fieldName, fieldValue));
                            }
                        }
                    }
                    else
                    {
                        if (fieldInfo.IsOptional ?? false)
                        {
                            continue;
                        }
                        else
                        {
                            throw new JsonSerializationException(
                                string.Format("PIDLData is missing required field: {0}", fieldName));
                        }
                    }
                }
                else
                {
                    PIDLData nestedData = ((JObject)pidlData[fieldName]).ToObject<PIDLData>();
                    pidlData[fieldName] = nestedData; 
                    Dictionary<string, object> typeInfoOfNestedData = GetTypeInfo(nestedData);
                    this.ValidateTypeConformance(nestedData, typeInfoOfNestedData, serializer); 
                }
            }
        }
    }
}
