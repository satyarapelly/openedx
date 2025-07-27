// <copyright file="ResourceLifecycleStateManager.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXService.Settings;

    public sealed class ResourceLifecycleStateManager
    {
        private static readonly Lazy<ResourceLifecycleStateManager> InstanceField = new Lazy<ResourceLifecycleStateManager>(() => new ResourceLifecycleStateManager());
        
        private ServiceErrorRules serviceErrorRules;
        private ClientActionRules clientActionRules;
        private ConfigurationParser<ClientActionRules> clientActionConfigurationParser;
        private ConfigurationParser<ServiceErrorRules> serviceErrorConfigurationParser;

        private ResourceLifecycleStateManager()
        {
            this.clientActionConfigurationParser = new ConfigurationParser<ClientActionRules>();
            this.serviceErrorConfigurationParser = new ConfigurationParser<ServiceErrorRules>();

            this.clientActionConfigurationParser.Component = new ConfigurationComponentRule(
                subComponentRules: new Dictionary<string, ConfigurationComponentRule>()
                {
                    {
                        "ClientActionRule",
                        new ConfigurationComponentRule(
                            isRowInRange: (row, i) => this.clientActionConfigurationParser.MatchesColumns(row, SetOperation.AllEmpty, "PaymentName"),
                            isRowInList: row => true,
                            subComponentRules: new Dictionary<string, ConfigurationComponentRule>()
                            {
                                {
                                    "PaymentMethod",
                                    new ConfigurationComponentRule(
                                        isRowInRange: (row, i) => this.clientActionConfigurationParser.MatchesColumns(row, SetOperation.SomeFull, "PaymentMethodFamily", "PaymentMethodType", "Country"))
                                },
                                {
                                    "ClientAction",
                                    new ConfigurationComponentRule(
                                        isRowInList: row => this.clientActionConfigurationParser.MatchesColumns(row, SetOperation.AllFull, "ClientActionType"))
                                }
                            })
                    }
                });

            this.serviceErrorConfigurationParser.Component = new ConfigurationComponentRule(
                subComponentRules: new Dictionary<string, ConfigurationComponentRule>()
                {
                    {
                        "RequestType",
                        new ConfigurationComponentRule(
                            isRowInRange: (row, i) => this.serviceErrorConfigurationParser.MatchesColumns(row, SetOperation.AllEmpty, "ResourceAction"),
                            isRowInList: row => true,
                            subComponentRules: new Dictionary<string, ConfigurationComponentRule>()
                            {
                                {
                                    "ServiceErrorRule",
                                    new ConfigurationComponentRule(
                                        isRowInRange: (row, i) => this.serviceErrorConfigurationParser.MatchesColumns(row, SetOperation.AllEmpty, "PaymentMethodFamily"),
                                        isRowInList: row => true,
                                        subComponentRules: new Dictionary<string, ConfigurationComponentRule>()
                                        {
                                            {
                                                "PaymentMethod",
                                                new ConfigurationComponentRule(
                                                    isRowInRange: (row, i) => this.serviceErrorConfigurationParser.MatchesColumns(row, SetOperation.AllEmpty, "PaymentMethodFamily"))
                                            },
                                            {
                                                "ServiceError",
                                                new ConfigurationComponentRule(
                                                    isRowInRange: (row, i) => this.serviceErrorConfigurationParser.MatchesColumns(row, SetOperation.AllEmpty, "Message"),
                                                    isRowInList: row => this.serviceErrorConfigurationParser.MatchesColumns(row, SetOperation.SomeFull, "Message", "ErrorCode", "DetailsTarget"))
                                            }
                                        })
                                }
                            })
                    }
                });
        }

        public enum SetOperation
        {
            AllEmpty,
            SomeEmpty,
            AllFull,
            SomeFull,
        }

        public enum ErrorResourceAction
        {
            PostModernPI,
            UpdateModernPI,
            ResumePendingOperation,
        }

        public enum ClientActionResourceAction
        {
            AddClientActionToPI,
        }

        public enum ErrorClientAction
        {
            None,
            DirectDebitAch,
            DirectDebitSepa,
            Jumpback,
        }

        public enum ClientActionConfigColumn
        {
            PaymentName = 0,
            PaymentMethodFamily,
            PaymentMethodType,
            Country,
            UnmatchedPendingOn,
            NullPendingOn,
            UnmatchedRequestType,
            PendingOn,
            RequestType,
            Partner,
            ClientActionType,
            Context,
            PidlResourceDescriptionType,
            StaticRedirect,
            ClearMembers,
        }

        public enum ErrorConfigColumn
        {
            ResourceAction = 0,
            PaymentMethodFamily,
            PaymentMethodType,
            Country,
            ContainedIn_Partner,
            Partner,
            Message,
            ErrorCode,
            DetailsErrorCode,
            DetailsMessage,
            DetailsTarget,
            ClientAction,
        }

        public static ResourceLifecycleStateManager Instance
        {
            get { return ResourceLifecycleStateManager.InstanceField.Value; }
        }

        internal static PXServiceSettings ServiceSettings { get; private set; }

        public static string ParseConstant(string name)
        {
            string[] classFieldNames = name.Split('.');

            if (classFieldNames.Length != 2)
            {
                return name;
            }
            else
            {
                Type classType = typeof(V7.Constants).GetNestedType(classFieldNames[0], System.Reflection.BindingFlags.NonPublic);
                FieldInfo field = classType.GetField(classFieldNames[1], BindingFlags.Static | BindingFlags.NonPublic);

                return field.GetValue(null) as string;
            }
        }

        public static void Initialize(PXServiceSettings settings, string errorConfigFilePath, string clientActionConfigFilePath)
        {
            ResourceLifecycleStateManager.ServiceSettings = settings;

            ResourceLifecycleStateManager.Instance.PopulateErrorRules(errorConfigFilePath);
            ResourceLifecycleStateManager.Instance.PopulateClientActionRules(clientActionConfigFilePath);
        }

        public async Task SetErrorAsync(ErrorResourceAction action, ErrorResourceState state)
        {
            PaymentTypeErrorFilter filter = this.serviceErrorRules.GetMatchingPaymentTypeErrorFilter(action, state);

            await filter.ErrorDetails.UpdateErrorAsync(state);
        }

        public void SetClientAction(ClientActionResourceAction action, ClientActionResourceState state)
        {
            int ruleIndex = this.clientActionRules.GetMatchingPaymentMethodIndex(state);

            ClientActionFilter filter = this.clientActionRules.GetMatchingClientActionFilter(state, ruleIndex);

            filter.UpdateClientAction(state);
        }

        private void PopulateClientActionRules(string path)
        {
            this.clientActionRules = this.clientActionConfigurationParser.Parse(path);
        }

        private void PopulateErrorRules(string path)
        {
            this.serviceErrorRules = this.serviceErrorConfigurationParser.Parse(path);
        }

        public struct ClientActionResourceState
        {
            public ClientActionResourceState(PaymentInstrument pi, string accountId, string billableAccountId, string country, string partner, string language, string classicProduct, bool completePrerequisites, string email, string pidlBaseUrl, string requestType, EventTraceActivity traceActivityId)
            {
                this.PaymentInstrument = pi;
                this.AccountId = accountId;
                this.BillableAccountId = billableAccountId;
                this.PaymentMethodFamily = pi.PaymentMethod.PaymentMethodFamily;
                this.PaymentMethodType = pi.PaymentMethod.PaymentMethodType;
                this.Country = country;
                this.Partner = partner;
                this.Language = language;
                this.ClassicProduct = classicProduct;
                this.CompletePrerequisites = completePrerequisites;
                this.EmailAddress = email;
                this.PidlBaseUrl = pidlBaseUrl;
                this.RequestType = requestType;
                this.TraceActivityId = traceActivityId;
            }

            public string AccountId { get; private set; }

            public string BillableAccountId { get; private set; }

            public string ClassicProduct { get; private set; }

            public bool CompletePrerequisites { get; private set; }

            public string Country { get; private set; }

            public string EmailAddress { get; private set; }

            public string Language { get; private set; }

            public string Partner { get; private set; }
            
            public PaymentInstrument PaymentInstrument { get; private set; }

            public string PaymentMethodFamily { get; private set; }

            public string PaymentMethodType { get; private set; }

            public string PidlBaseUrl { get; private set; }

            public string RequestType { get; private set; }

            public EventTraceActivity TraceActivityId { get; private set; }

            public static bool operator ==(ClientActionResourceState state1, ClientActionResourceState state2)
            {
                return state1.Equals(state2);
            }

            public static bool operator !=(ClientActionResourceState state1, ClientActionResourceState state2)
            {
                return !state1.Equals(state2);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ClientActionResourceState))
                {
                    return false;
                }

                ClientActionResourceState other = (ClientActionResourceState)obj;

                return other.AccountId == this.AccountId && other.BillableAccountId == this.BillableAccountId && other.ClassicProduct == this.ClassicProduct && 
                    other.CompletePrerequisites == this.CompletePrerequisites && other.Country == this.Country && other.EmailAddress == this.EmailAddress && 
                    other.Language == this.Language && other.Partner == this.Partner && other.PaymentInstrument.Equals(this.PaymentInstrument) && 
                    other.PaymentMethodFamily == this.PaymentMethodFamily && other.PaymentMethodType == this.PaymentMethodType && other.PidlBaseUrl == this.PidlBaseUrl && 
                    other.RequestType == this.RequestType && other.TraceActivityId.Equals(this.TraceActivityId);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        public struct ErrorResourceState
        {
            public ErrorResourceState(ref ServiceErrorResponseException ex, string accountId, string billableAccountId, string family, string type, string country, string partner, string language, string classicProduct, bool completePrerequisites, PaymentInstrument pi, string piid, EventTraceActivity traceActivityId)
            {
                this.ResponseException = ex;
                this.AccountId = accountId;
                this.BillableAccountId = billableAccountId;
                this.PaymentMethodFamily = family;
                this.PaymentMethodType = SanitizePaymentMethodTypeInput(type);
                this.Country = country;
                this.Partner = partner;
                this.Language = language;
                this.ClassicProduct = classicProduct;
                this.CompletePrerequisites = completePrerequisites;
                this.PI = pi;
                this.Piid = piid;
                this.TraceActivityId = traceActivityId;
            }

            public string AccountId { get; private set; }

            public string BillableAccountId { get; private set; }

            public string ClassicProduct { get; private set; }

            public bool CompletePrerequisites { get; private set; }

            public string Country { get; private set; }

            public string Language { get; private set; }

            public string Partner { get; private set; }

            public string PaymentMethodFamily { get; private set; }

            public string PaymentMethodType { get; private set; }

            public PaymentInstrument PI { get; private set; }

            public string Piid { get; private set; }

            public ServiceErrorResponseException ResponseException { get; private set; }

            public EventTraceActivity TraceActivityId { get; private set; }

            public static bool operator ==(ErrorResourceState state1, ErrorResourceState state2)
            {
                return state1.Equals(state2);
            }

            public static bool operator !=(ErrorResourceState state1, ErrorResourceState state2)
            {
                return !state1.Equals(state2);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ErrorResourceState))
                {
                    return false;
                }

                ErrorResourceState other = (ErrorResourceState)obj;

                return other.AccountId == this.AccountId && other.BillableAccountId == this.BillableAccountId && other.ClassicProduct == this.ClassicProduct &&
                    other.CompletePrerequisites == this.CompletePrerequisites && other.Country == this.Country && other.Language == this.Language && other.Partner == this.Partner && 
                    other.PaymentMethodFamily == this.PaymentMethodFamily && other.PaymentMethodType == this.PaymentMethodType &&
                    other.PI.Equals(this.PI) && other.Piid == this.Piid && other.ResponseException.Equals(this.ResponseException) && other.TraceActivityId.Equals(this.TraceActivityId);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            private static string SanitizePaymentMethodTypeInput(string type)
            {
                if (string.IsNullOrEmpty(type))
                {
                    return ResourceLifecycleStateManager.ResourceLifecycleConstants.PaymentTypeEmpty;
                }
                else
                {
                    return type;
                }
            }
        }

        internal static class ResourceLifecycleConstants
        {
            internal const string ErrorConfigFilePath = @"App_Data\PIServiceErrorConfig.csv";
            internal const string ClientActionConfigFilePath = @"App_Data\ClientActionConfig.csv";

            // there are some rules that explicitely restrict to null payment type, so to define a key for this rule, we make an arbitrary value and sanitize input so null input will match to it
            internal const string PaymentTypeEmpty = "empty";
        }
    }
}