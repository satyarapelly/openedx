// <copyright file="ClientActionRules.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public class ClientActionRules : ConfigurationObject
    {
        public ClientActionRules(List<string[]> rows, ParsedConfigurationComponent parsedComponent, Dictionary<string, int> columnNames)
        {
            this.FamilyFilter = new Dictionary<string, int>();
            this.FamilyUnfiltered = new HashSet<int>();
            this.TypeFilter = new Dictionary<string, int>();
            this.TypeUnfiltered = new HashSet<int>();
            this.CountryFilter = new Dictionary<string, int>();
            this.CountryUnfiltered = new HashSet<int>();
            this.ClientActionFilters = new List<List<ClientActionFilter>>();
            this.FilterExceptions = new List<Tuple<bool, string, string>>();

            foreach (ParsedConfigurationComponent rule in parsedComponent["ClientActionRule"])
            {
                // only 1 exception group per rule so no need for component
                Tuple<bool, string, string> filterException = new Tuple<bool, string, string>(
                    rows[rule.Range.Item1][(int)ResourceLifecycleStateManager.ClientActionConfigColumn.NullPendingOn] == "TRUE" ? true : false,
                    rows[rule.Range.Item1][(int)ResourceLifecycleStateManager.ClientActionConfigColumn.UnmatchedPendingOn],
                    rows[rule.Range.Item1][(int)ResourceLifecycleStateManager.ClientActionConfigColumn.UnmatchedRequestType]);

                this.FilterExceptions.Add(filterException);

                List<ClientActionFilter> filters = new List<ClientActionFilter>();

                foreach (ParsedConfigurationComponent clientAction in rule["ClientAction"])
                {
                    filters.Add(ClientActionFilter.ConstructFromConfiguration<ClientActionFilter>(rows, clientAction, columnNames));
                }

                this.ClientActionFilters.Add(filters);

                ParsedConfigurationComponent paymentMethod = rule["PaymentMethod"].First();

                ClientActionRules.ParsePaymentMethodFilterColumn(rows, paymentMethod, (int)ResourceLifecycleStateManager.ClientActionConfigColumn.PaymentMethodFamily, this.FamilyFilter, this.FamilyUnfiltered, this.ClientActionFilters.Count - 1);
                ClientActionRules.ParsePaymentMethodFilterColumn(rows, paymentMethod, (int)ResourceLifecycleStateManager.ClientActionConfigColumn.PaymentMethodType, this.TypeFilter, this.TypeUnfiltered, this.ClientActionFilters.Count - 1, true);
                ClientActionRules.ParsePaymentMethodFilterColumn(rows, paymentMethod, (int)ResourceLifecycleStateManager.ClientActionConfigColumn.Country, this.CountryFilter, this.CountryUnfiltered, this.ClientActionFilters.Count - 1);
            }
        }

        public Dictionary<string, int> FamilyFilter { get; private set; }

        public HashSet<int> FamilyUnfiltered { get; private set; }

        public Dictionary<string, int> TypeFilter { get; private set; }

        public HashSet<int> TypeUnfiltered { get; private set; }

        public Dictionary<string, int> CountryFilter { get; private set; }

        public HashSet<int> CountryUnfiltered { get; private set; }

        public List<List<ClientActionFilter>> ClientActionFilters { get; private set; }

        // exception on null PendingOn, exception message when PendingOn fails to match, exception message when RequestType fails to match
        public List<Tuple<bool, string, string>> FilterExceptions { get; private set; }

        public static HashSet<int> GetFilteredMatchingClientActionRules(string stateKey, Dictionary<string, int> filteredRuleIds, HashSet<int> unfilteredRuleIds, HashSet<int> setConstraint = null)
        {
            HashSet<int> matchingRules = new HashSet<int>();

            if (filteredRuleIds.ContainsKey(stateKey))
            {
                matchingRules.Add(filteredRuleIds[stateKey]);
            }
            else
            {
                matchingRules = new HashSet<int>(unfilteredRuleIds);
            }

            if (setConstraint != null)
            {
                matchingRules.IntersectWith(setConstraint);
            }

            return matchingRules;
        }

        public ClientActionFilter GetMatchingClientActionFilter(ResourceLifecycleStateManager.ClientActionResourceState state, int ruleIndex)
        {
            ClientActionFilter filter = null;

            LinkedList<ClientActionFilter> clientActionFilters = new LinkedList<ClientActionFilter>(this.ClientActionFilters[ruleIndex]);
            Tuple<bool, string, string> filterExceptions = this.FilterExceptions[ruleIndex];

            if (string.IsNullOrEmpty(state.PaymentInstrument.PaymentInstrumentDetails.PendingOn))
            {
                if (filterExceptions.Item1)
                {
                    throw TraceCore.TraceException<IntegrationException>(
                        state.TraceActivityId,
                        new IntegrationException(
                            PXCommon.Constants.ServiceNames.InstrumentManagementService,
                            "The state of the PI is set to pending but the pendingOn is null",
                            V7.Constants.PXServiceIntegrationErrorCodes.InvalidPendingOnType));
                }
            }

            ClientActionFilter.FilterClientActionTypes(
                clientActionFilters,
                node => !string.IsNullOrEmpty(node.Value.PendingOn) &&
                    ((node.Value.PendingOnContainedIn && !string.Equals(state.PaymentInstrument.PaymentInstrumentDetails.PendingOn, node.Value.PendingOn, StringComparison.OrdinalIgnoreCase)) || (!node.Value.PendingOnContainedIn && string.Equals(state.PaymentInstrument.PaymentInstrumentDetails.PendingOn, node.Value.PendingOn, StringComparison.OrdinalIgnoreCase))),
                filterExceptions.Item2,
                state.PaymentInstrument.PaymentInstrumentDetails.PendingOn,
                state.TraceActivityId);

            ClientActionFilter.FilterClientActionTypes(
                clientActionFilters,
                    node => !string.IsNullOrEmpty(node.Value.RequestType) && !string.Equals(state.RequestType, node.Value.RequestType, StringComparison.OrdinalIgnoreCase),
                filterExceptions.Item3,
                state.RequestType,
                state.TraceActivityId);

            ClientActionFilter.FilterClientActionTypes(
                clientActionFilters,
                node => !string.IsNullOrEmpty(node.Value.Partner) &&
                    ((node.Value.PartnerContainedIn && !string.Equals(state.Partner, node.Value.Partner, StringComparison.OrdinalIgnoreCase)) || (!node.Value.PartnerContainedIn && string.Equals(state.Partner, node.Value.Partner, StringComparison.OrdinalIgnoreCase))),
                null,
                null,
                state.TraceActivityId);

            if (clientActionFilters.Count > 0)
            {
                filter = clientActionFilters.First();
            }

            return filter;
        }

        public int GetMatchingPaymentMethodIndex(ResourceLifecycleStateManager.ClientActionResourceState state)
        {
            HashSet<int> matchingRules = ClientActionRules.GetFilteredMatchingClientActionRules(state.PaymentMethodFamily, this.FamilyFilter, this.FamilyUnfiltered);
            matchingRules = ClientActionRules.GetFilteredMatchingClientActionRules(state.PaymentMethodType, this.TypeFilter, this.TypeUnfiltered, matchingRules);
            matchingRules = ClientActionRules.GetFilteredMatchingClientActionRules(state.Country, this.CountryFilter, this.CountryUnfiltered, matchingRules);

            return matchingRules.First();
        }

        private static void ParsePaymentMethodFilterColumn(List<string[]> rows, ParsedConfigurationComponent paymentMethod, int column, Dictionary<string, int> filter, HashSet<int> unfiltered, int filterIndex, bool parseConstant = false)
        {
            List<string> columnValues = new List<string>();

            foreach (string[] row in rows.GetRange(paymentMethod.Range.Item1, paymentMethod.Range.Item2 - paymentMethod.Range.Item1 + 1))
            {
                if (string.IsNullOrEmpty(row[column]))
                {
                    break;
                }
                else
                {
                    if (parseConstant)
                    {
                        columnValues.Add(ResourceLifecycleStateManager.ParseConstant(row[column]));
                    }
                    else
                    {
                        columnValues.Add(row[column]);
                    }
                }
            }

            if (columnValues.Count > 0)
            {
                foreach (string columnValue in columnValues)
                {
                    filter[columnValue] = filterIndex;
                }
            }
            else
            {
                unfiltered.Add(filterIndex);
            }
        }
    }
}