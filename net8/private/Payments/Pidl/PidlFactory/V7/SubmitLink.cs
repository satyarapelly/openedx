// <copyright file="SubmitLink.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;

    public class SubmitLink : RestLink
    {
        private static Dictionary<string, List<string>> submitLinkContentHintIds = new Dictionary<string, List<string>>()
        {
            {
                // address and profile submit links all use the same buttons/hyperlinks.
                // Other submit links use other button ids to be defined later.
                // One arbitrary requirement is that the default submit link allowedDisplayHintIds persist for overrides
                string.Empty, new List<string>()
                {
                    Constants.ButtonDisplayHintIds.SubmitButton,
                    Constants.ButtonDisplayHintIds.SubmitButtonHidden,
                    Constants.ButtonDisplayHintIds.SaveButton,
                    Constants.ButtonDisplayHintIds.SaveButtonHidden,
                    Constants.ButtonDisplayHintIds.SaveNextButton,
                    Constants.ButtonDisplayHintIds.PaypalSaveNextButton,
                    Constants.ButtonDisplayHintIds.SaveContinueButton,
                }
            },
        };

        public SubmitLink(string resourceType, string resourceIdentity, string resourceOperation, string href, string method, string headerApi, bool headerCorrelationId, bool headerTrackingId, IEnumerable<string> errorCodeExpressions, string allowedContentIdsGroup)
        {
            this.ResourceType = resourceType;
            this.ResourceIdentity = resourceIdentity;
            this.ResourceOperation = resourceOperation;
            this.SubmitLinkHref = href;
            this.SubmitLinkMethod = method;
            this.HeaderApiVersion = headerApi;
            this.HeaderCorrelationId = headerCorrelationId;
            this.HeaderTrackingId = headerTrackingId;
            this.SubmitLinkErrorCodeExpressions = errorCodeExpressions;
            this.AllowedContentHintIdsGroup = allowedContentIdsGroup;
        }

        public enum EndpointResourceType
        {
            Default,
            OverrideToV3,
        }

        public enum AllowedContentIdsGroupType
        {
            Default,
        }

        [JsonIgnore]
        public string ResourceType { get; private set; }

        [JsonIgnore]
        public string ResourceIdentity { get; private set; }

        [JsonIgnore]
        public string ResourceOperation { get; private set; }

        [JsonIgnore]
        public string SubmitLinkHref { get; private set; }

        [JsonIgnore]
        public string SubmitLinkMethod { get; private set; }

        [JsonIgnore]
        public string HeaderApiVersion { get; private set; }

        [JsonIgnore]
        public bool HeaderCorrelationId { get; private set; }

        [JsonIgnore]
        public bool HeaderTrackingId { get; private set; }

        [JsonIgnore]
        public IEnumerable<string> SubmitLinkErrorCodeExpressions { get; private set; }

        [JsonIgnore]
        public string AllowedContentHintIdsGroup { get; private set; }

        [JsonIgnore]
        public string ProfileId { get; set; }

        public static void ReadFromConfig(string filePath, out Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, SubmitLink>>>>> submitLinksConfigs)
        {
            using (PIDLConfigParser parser = new PIDLConfigParser(
                filePath,
                new ColumnDefinition[]
                {
                    new ColumnDefinition("PIDLResourceType",           ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("PIDLResourceIdentity",       ColumnConstraint.Required, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Operation",                  ColumnConstraint.Required, ColumnFormat.Text),
                    new ColumnDefinition("Scenario",                   ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("EndpointResourceType",       ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("Href",                       ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("Method",                     ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("HeaderApi",                  ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("HeaderCorrelationId",        ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("HeaderTrackingId",           ColumnConstraint.Optional, ColumnFormat.Text),
                    new ColumnDefinition("ErrorCodeExpressions",       ColumnConstraint.Optional, ColumnFormat.AlphaNumeric),
                    new ColumnDefinition("AllowedContentIdsGroup",     ColumnConstraint.Optional, ColumnFormat.Text),
                },
                true))
            {
                submitLinksConfigs = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, SubmitLink>>>>>(StringComparer.CurrentCultureIgnoreCase);

                while (!parser.EndOfData)
                {
                    string[] cells = parser.ReadValidatedFields();
                    string pidlResourceType = ValidateAndParseKey(cells[SubmitLinksCellIndexDescription.PIDLResourceType], "PIDLResourceType", typeof(Constants.DescriptionTypes), parser.LineNumber);
                    string pidlResourceIdentity = cells[SubmitLinksCellIndexDescription.PIDLResourceIdentity]; // allowed values are not defined in a constant class so we can't validate
                    string operation = ValidateAndParseKey(cells[SubmitLinksCellIndexDescription.Operation], "Operation", typeof(Constants.PidlOperationTypes), parser.LineNumber);

                    string scenario = string.IsNullOrEmpty(cells[SubmitLinksCellIndexDescription.Scenario]) ? string.Empty
                        : cells[SubmitLinksCellIndexDescription.Scenario];
                    string endpointResourceType = string.IsNullOrEmpty(cells[SubmitLinksCellIndexDescription.EndpointResourceType]) ? string.Empty
                        : cells[SubmitLinksCellIndexDescription.EndpointResourceType];
                    string href = string.IsNullOrEmpty(cells[SubmitLinksCellIndexDescription.Href]) ? null
                        : cells[SubmitLinksCellIndexDescription.Href];
                    string method = string.IsNullOrEmpty(cells[SubmitLinksCellIndexDescription.Method]) ? null
                        : cells[SubmitLinksCellIndexDescription.Method];
                    string headerApi = cells[SubmitLinksCellIndexDescription.HeaderApi];
                    bool headerCorrelationId = string.Equals(cells[SubmitLinksCellIndexDescription.HeaderCorrelationId], "true", StringComparison.OrdinalIgnoreCase);
                    bool headerTrackingId = string.Equals(cells[SubmitLinksCellIndexDescription.HeaderTrackingId], "true", StringComparison.OrdinalIgnoreCase);
                    string[] errorCodeExpressions = cells[SubmitLinksCellIndexDescription.ErrorCodeExpressions]
                        .Split(new string[] { Constants.ConfigSpecialStrings.CollectionDelimiter }, StringSplitOptions.None)
                        .Where(e => !string.IsNullOrEmpty(e)).ToArray();
                    string allowedContentIdsGroup = string.IsNullOrEmpty(cells[SubmitLinksCellIndexDescription.AllowedContentIdsGroup]) ? string.Empty
                        : cells[SubmitLinksCellIndexDescription.AllowedContentIdsGroup];

                    if (method != null && !(method.Equals(GlobalConstants.HttpMethods.Post) || method.Equals(GlobalConstants.HttpMethods.Patch) || method.Equals(GlobalConstants.HttpMethods.Put)))
                    {
                        throw new PIDLConfigException(
                            Constants.SubmitLinksFilePaths.SubmitLinksCSV,
                            parser.LineNumber,
                            string.Format("Value of the Method does not match any accepted value."),
                            Constants.ErrorCodes.PIDLConfigFileColumnIsMalformed);
                    }

                    if (!string.IsNullOrEmpty(endpointResourceType) && !Enum.GetNames(typeof(EndpointResourceType)).Contains(endpointResourceType))
                    {
                        throw new PIDLConfigException(
                            Constants.SubmitLinksFilePaths.SubmitLinksCSV,
                            parser.LineNumber,
                            string.Format("Value of the EndpointResourceType does not match any accepted value."),
                            Constants.ErrorCodes.PIDLConfigFileColumnIsMalformed);
                    }

                    if (!string.IsNullOrEmpty(allowedContentIdsGroup) && !Enum.GetNames(typeof(AllowedContentIdsGroupType)).Contains(allowedContentIdsGroup))
                    {
                        throw new PIDLConfigException(
                            Constants.SubmitLinksFilePaths.SubmitLinksCSV,
                            parser.LineNumber,
                            string.Format("Value of the AllowedContentIdsGroup does not match any accepted value."),
                            Constants.ErrorCodes.PIDLConfigFileColumnIsMalformed);
                    }

                    if (!submitLinksConfigs.ContainsKey(pidlResourceType))
                    {
                        submitLinksConfigs[pidlResourceType] = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, SubmitLink>>>>(StringComparer.CurrentCultureIgnoreCase);
                    }

                    if (!submitLinksConfigs[pidlResourceType].ContainsKey(pidlResourceIdentity))
                    {
                        submitLinksConfigs[pidlResourceType][pidlResourceIdentity] = new Dictionary<string, Dictionary<string, Dictionary<string, SubmitLink>>>(StringComparer.CurrentCultureIgnoreCase);
                    }

                    if (!submitLinksConfigs[pidlResourceType][pidlResourceIdentity].ContainsKey(operation))
                    {
                        submitLinksConfigs[pidlResourceType][pidlResourceIdentity][operation] = new Dictionary<string, Dictionary<string, SubmitLink>>(StringComparer.CurrentCultureIgnoreCase);
                    }
                    
                    if (!submitLinksConfigs[pidlResourceType][pidlResourceIdentity][operation].ContainsKey(scenario))
                    {
                        submitLinksConfigs[pidlResourceType][pidlResourceIdentity][operation][scenario] = new Dictionary<string, SubmitLink>(StringComparer.CurrentCultureIgnoreCase);
                    }

                    if (!submitLinksConfigs[pidlResourceType][pidlResourceIdentity][operation][scenario].ContainsKey(endpointResourceType))
                    {
                        submitLinksConfigs[pidlResourceType][pidlResourceIdentity][operation][scenario][endpointResourceType] = new SubmitLink(pidlResourceType, pidlResourceIdentity, operation, href, method, headerApi, headerCorrelationId, headerTrackingId, errorCodeExpressions, allowedContentIdsGroup);
                    }
                    else
                    {
                        throw new PIDLConfigException(
                            filePath,
                            parser.LineNumber,
                            string.Format("An additional submit link was found for the data description key [{0}][{1}][{2}][{3}]", pidlResourceType, pidlResourceIdentity, operation, endpointResourceType),
                            Constants.ErrorCodes.PIDLConfigPIDLResourceIdIsMalformed);
                    }
                }
            }
        }

        public SubmitLink DeepCopy()
        {
            return new SubmitLink(this.ResourceType, this.ResourceIdentity, this.ResourceOperation, this.SubmitLinkHref, this.SubmitLinkMethod, this.HeaderApiVersion, this.HeaderCorrelationId, this.HeaderTrackingId, this.SubmitLinkErrorCodeExpressions, this.AllowedContentHintIdsGroup);
        }

        public void ConstructRestLink(Dictionary<string, string> extraHeaders = null)
        {
            this.Href = this.SubmitLinkHref;

            if (!string.IsNullOrEmpty(this.ProfileId))
            {
                this.Href = string.Format(this.SubmitLinkHref, this.ProfileId);
            }

            this.Method = this.SubmitLinkMethod;

            if (!string.IsNullOrEmpty(this.HeaderApiVersion))
            {
                this.AddHeader(Constants.CustomHeaders.ApiVersion, this.HeaderApiVersion);
            }

            if (this.HeaderCorrelationId)
            {
                this.AddHeader(Constants.CustomHeaders.MsCorrelationId, Guid.NewGuid().ToString());
            }

            if (this.HeaderTrackingId)
            {
                this.AddHeader(Constants.CustomHeaders.MsTrackingId, Guid.NewGuid().ToString());
            }

            if (this.SubmitLinkErrorCodeExpressions?.Any() ?? false)
            {
                this.SetErrorCodeExpressions(this.SubmitLinkErrorCodeExpressions.ToArray());
            }

            foreach (KeyValuePair<string, string> header in extraHeaders ?? Enumerable.Empty<KeyValuePair<string, string>>())
            {
                this.AddHeader(header.Key, header.Value);
            }
        }

        public IEnumerable<string> GetAllowedContentHintIds()
        {
            return SubmitLink.submitLinkContentHintIds[this.AllowedContentHintIdsGroup];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("[Href: {0}; Method: {1}; First ErrorCodeExpressions: {2}]", this.SubmitLinkHref, this.SubmitLinkMethod, this.SubmitLinkErrorCodeExpressions.FirstOrDefault());

            return sb.ToString();
        }

        private static string ValidateAndParseKey(string key, string columnName, Type constantClassType, long lineNumber)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            if (constantClassType == null)
            {
                throw new ArgumentNullException(nameof(constantClassType));
            }

            if (!constantClassType
                .GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static)
                .Any(field => string.Equals(key, field.GetValue(null) as string)))
            {
                throw new PIDLConfigException(
                            Constants.SubmitLinksFilePaths.SubmitLinksCSV,
                            lineNumber,
                            string.Format("The value {0} of the column {1} on line {2} does not match any accepted value allowed in {3}.", key, columnName, lineNumber, constantClassType.Name),
                            Constants.ErrorCodes.PIDLConfigFileColumnIsMalformed);
            }

            return key;
        }

        private static class SubmitLinksCellIndexDescription
        {
            public const int PIDLResourceType = 0;
            public const int PIDLResourceIdentity = 1;
            public const int Operation = 2;
            public const int Scenario = 3;
            public const int EndpointResourceType = 4;
            public const int Href = 5;
            public const int Method = 6;
            public const int HeaderApi = 7;
            public const int HeaderCorrelationId = 8;
            public const int HeaderTrackingId = 9;
            public const int ErrorCodeExpressions = 10;
            public const int AllowedContentIdsGroup = 11;
        }
    }
}
