// <copyright file="ClientActionFilter.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;

    public class ClientActionFilter : ConfigurationObject
    {
        public ClientActionFilter(List<string[]> rows, ParsedConfigurationComponent parsedComponent, Dictionary<string, int> columnNames)
        {
            string[] row = rows[parsedComponent.Range.Item1];

            if (!string.IsNullOrEmpty(row[(int)ResourceLifecycleStateManager.ClientActionConfigColumn.PendingOn]))
            {
                if (row[(int)ResourceLifecycleStateManager.ClientActionConfigColumn.PendingOn].StartsWith("!"))
                {
                    this.PendingOnContainedIn = false;
                    this.PendingOn = ResourceLifecycleStateManager.ParseConstant(row[(int)ResourceLifecycleStateManager.ClientActionConfigColumn.PendingOn].Substring(1));
                }
                else
                {
                    this.PendingOnContainedIn = true;
                    this.PendingOn = ResourceLifecycleStateManager.ParseConstant(row[(int)ResourceLifecycleStateManager.ClientActionConfigColumn.PendingOn]);
                }
            }
            else
            {
                this.PendingOnContainedIn = false;
                this.PendingOn = null;
            }

            if (!string.IsNullOrEmpty(row[(int)ResourceLifecycleStateManager.ClientActionConfigColumn.RequestType]))
            {
                this.RequestType = ResourceLifecycleStateManager.ParseConstant(row[(int)ResourceLifecycleStateManager.ClientActionConfigColumn.RequestType]);
            }
            else
            {
                this.RequestType = null;
            }

            if (!string.IsNullOrEmpty(row[(int)ResourceLifecycleStateManager.ClientActionConfigColumn.Partner]))
            {
                if (row[(int)ResourceLifecycleStateManager.ClientActionConfigColumn.Partner].StartsWith("!"))
                {
                    this.PartnerContainedIn = false;
                    this.Partner = ResourceLifecycleStateManager.ParseConstant(row[(int)ResourceLifecycleStateManager.ClientActionConfigColumn.Partner].Substring(1));
                }
                else
                {
                    this.PartnerContainedIn = true;
                    this.Partner = ResourceLifecycleStateManager.ParseConstant(row[(int)ResourceLifecycleStateManager.ClientActionConfigColumn.Partner]);
                }
            }
            else
            {
                this.PartnerContainedIn = false;
                this.Partner = null;
            }

            this.ClientActionType = (ClientActionFilterType)Enum.Parse(typeof(ClientActionFilterType), row[(int)ResourceLifecycleStateManager.ClientActionConfigColumn.ClientActionType]);

            this.Context = (ClientActionContext)Enum.Parse(typeof(ClientActionContext), row[(int)ResourceLifecycleStateManager.ClientActionConfigColumn.Context]);

            this.PidlResourceDescriptionType = string.IsNullOrEmpty(row[(int)ResourceLifecycleStateManager.ClientActionConfigColumn.PidlResourceDescriptionType]) ? null : ResourceLifecycleStateManager.ParseConstant(row[(int)ResourceLifecycleStateManager.ClientActionConfigColumn.PidlResourceDescriptionType]);

            this.IsStaticRedirect = row[(int)ResourceLifecycleStateManager.ClientActionConfigColumn.StaticRedirect] == "TRUE" ? true : false;

            this.ClearMembers = row[(int)ResourceLifecycleStateManager.ClientActionConfigColumn.ClearMembers] == "TRUE" ? true : false;
        }

        public enum ClientActionFilterType
        {
            Pidl,
            Redirect,
            Digitization,
        }

        public enum ClientActionContext
        {
            RedirectUrl,
            RedirectServiceLink,
            GetSmsChallengeDescriptionForPI,
            GetUpdateAgreementChallengeDescriptionForPI,
            GetStaticPidlDescriptions,
            GetPicvChallengeDescriptionForPI,
            Digitization,
        }

        public string PendingOn { get; private set; }

        public bool PendingOnContainedIn { get; private set; }

        public string RequestType { get; private set; }

        public string Partner { get; private set; }

        public bool PartnerContainedIn { get; private set; }

        public ClientActionFilterType ClientActionType { get; private set; }

        public ClientActionContext Context { get; private set; }

        public string PidlResourceDescriptionType { get; private set; }

        public bool IsStaticRedirect { get; private set; }

        public bool ClearMembers { get; private set; }

        public static void FilterClientActionTypes(LinkedList<ClientActionFilter> clientActionFilters, Func<LinkedListNode<ClientActionFilter>, bool> removeFilterPredicate, string exceptionFormat, string exceptionPropertyValue, EventTraceActivity traceActivityId)
        {
            if (clientActionFilters.Count > 0)
            {
                LinkedListNode<ClientActionFilter> currentNode = clientActionFilters.First;

                while (currentNode != null)
                {
                    LinkedListNode<ClientActionFilter> nextNode = currentNode.Next;

                    if (removeFilterPredicate(currentNode))
                    {
                        clientActionFilters.Remove(currentNode);
                    }

                    currentNode = nextNode;
                }

                if (clientActionFilters.Count == 0)
                {
                    if (!string.IsNullOrEmpty(exceptionFormat))
                    {
                        throw TraceCore.TraceException<IntegrationException>(
                            traceActivityId,
                            new IntegrationException(
                                PXCommon.Constants.ServiceNames.InstrumentManagementService,
                                string.Format(exceptionFormat, exceptionPropertyValue),
                                V7.Constants.PXServiceIntegrationErrorCodes.InvalidPendingOnType));
                    }
                }
            }
        }

        public void UpdateClientAction(ResourceLifecycleStateManager.ClientActionResourceState state)
        {
            ClientAction clientAction = null;

            switch (this.ClientActionType)
            {
                case ClientActionFilterType.Pidl:
                    clientAction = new ClientAction(PXCommon.ClientActionType.Pidl);

                    switch (this.Context)
                    {
                        case ClientActionContext.GetSmsChallengeDescriptionForPI:
                            clientAction.Context = PIDLResourceFactory.Instance.GetSmsChallengeDescriptionForPI(state.PaymentInstrument, state.Language, state.Partner, state.ClassicProduct, state.BillableAccountId, state.EmailAddress, state.CompletePrerequisites, state.Country);
                            break;
                        case ClientActionContext.GetUpdateAgreementChallengeDescriptionForPI:
                            clientAction.Context = PIDLResourceFactory.Instance.GetUpdateAgreementChallengeDescriptionForPI(state.PaymentInstrument, this.PidlResourceDescriptionType, state.Language, state.Partner);
                            break;
                        case ClientActionContext.GetStaticPidlDescriptions:
                            clientAction.Context = PIDLResourceFactory.Instance.GetStaticPidlDescriptions(this.PidlResourceDescriptionType, state.Language, state.Partner);
                            break;
                        case ClientActionContext.GetPicvChallengeDescriptionForPI:
                            clientAction.Context = PIDLResourceFactory.Instance.GetPicvChallengeDescriptionForPI(state.PaymentInstrument, this.PidlResourceDescriptionType, state.Language, state.Partner, state.ClassicProduct, state.BillableAccountId);
                            break;
                    }

                    state.PaymentInstrument.ClientAction = clientAction;
                    break;
                case ClientActionFilterType.Redirect:
                    if (!string.IsNullOrEmpty(state.PaymentInstrument.PaymentInstrumentDetails.RedirectUrl))
                    {
                        clientAction = new ClientAction(PXCommon.ClientActionType.Redirect);

                        switch (this.Context)
                        {
                            case ClientActionContext.RedirectUrl:
                                clientAction.Context = state.PaymentInstrument.PaymentInstrumentDetails.RedirectUrl;
                                break;
                            case ClientActionContext.RedirectServiceLink:
                                RedirectionServiceLink redirectLink = new RedirectionServiceLink { BaseUrl = state.PaymentInstrument.PaymentInstrumentDetails.RedirectUrl };
                                redirectLink.RuParameters.Add("id", state.PaymentInstrument.PaymentInstrumentId);
                                redirectLink.RuParameters.Add("family", state.PaymentInstrument.PaymentMethod.PaymentMethodFamily);
                                redirectLink.RuParameters.Add("type", state.PaymentInstrument.PaymentMethod.PaymentMethodType);
                                redirectLink.RuParameters.Add("pendingOn", state.PaymentInstrument.PaymentInstrumentDetails.PendingOn);
                                redirectLink.RuParameters.Add("picvRequired", state.PaymentInstrument.PaymentInstrumentDetails.PicvRequired.ToString());

                                clientAction.Context = redirectLink;
                                break;
                        }

                        if (!V7.Constants.InlinePartners.Contains(state.Partner.ToLowerInvariant()))
                        {
                            if (this.IsStaticRedirect)
                            {
                                // currently guaranteed to have context of type RedirectServiceLink anytime redirect uses static call
                                clientAction.RedirectPidl = PIDLResourceFactory.Instance.GetStaticPidlDescriptions(this.PidlResourceDescriptionType, state.Language, state.Partner, (RedirectionServiceLink)clientAction.Context, state.PaymentInstrument);
                            }
                            else
                            {
                                clientAction.RedirectPidl = PIDLResourceFactory.Instance.GetRedirectPidlForPI(state.PaymentInstrument, this.PidlResourceDescriptionType, state.Language, state.Partner, state.CompletePrerequisites, state.Country);
                            }
                        }
                        else if (string.Equals(state.Partner, V7.Constants.PartnerName.WebblendsInline, StringComparison.InvariantCultureIgnoreCase) ||
                            string.Equals(state.Partner, V7.Constants.PartnerName.WebPay, StringComparison.InvariantCultureIgnoreCase) ||
                            string.Equals(state.Partner, V7.Constants.PartnerName.OXOWebDirect, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (this.ClearMembers)
                            {
                                List<PIDLResource> redirectPidl = null;

                                if (this.IsStaticRedirect)
                                {
                                    // currently guaranteed to have context of type RedirectServiceLink anytime redirect uses static call
                                    redirectPidl = PIDLResourceFactory.Instance.GetStaticPidlDescriptions(this.PidlResourceDescriptionType, state.Language, state.Partner, (RedirectionServiceLink)clientAction.Context, state.PaymentInstrument);
                                }
                                else
                                {
                                    redirectPidl = PIDLResourceFactory.Instance.GetRedirectPidlForPI(state.PaymentInstrument, this.PidlResourceDescriptionType, state.Language, state.Partner, state.CompletePrerequisites, state.Country);
                                }

                                redirectPidl[0].DisplayPages[0].Members.Clear();
                                clientAction.RedirectPidl = redirectPidl;
                            }
                        }

                        state.PaymentInstrument.ClientAction = clientAction;
                    }

                    break;
                case ClientActionFilterType.Digitization:
                    PidlDigitizationResourceFactory.AddClientActionForPI(state.PaymentInstrument, state.AccountId, state.Language, state.TraceActivityId, state.Partner, state.PidlBaseUrl);
                    break;
            }
        }
    }
}