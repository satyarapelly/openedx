// <copyright file="CTPCommerceDataAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Microsoft.Commerce.Tracing;
    using Microsoft.CTP.CommerceAPI.Proxy.v201112;
    using DataModel = Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Messages = Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages;
    using Proxy = Microsoft.CTP.CommerceAPI.Proxy.v201112;

    public class CTPCommerceDataAccessor : CommerceServiceExecutor<ICommerceServiceChannel>, ICTPCommereceDataAccessor
    {
        private const string ServiceName = "CTPCommerce";

        public CTPCommerceDataAccessor(string baseUrl, X509Certificate2 authCert) : base(baseUrl, authCert) { }

        protected override string ContractName
        {
            get { return "Microsoft.CTP.CommerceAPI.v201112.ICommerceService"; }
        }

        public Messages.GetSubscriptionsResponse GetSubscriptions(Messages.GetSubscriptionsRequest request, EventTraceActivity traceActivityId)
        {
            return this.Execute<Messages.GetSubscriptionsRequest, Messages.GetSubscriptionsResponse, GetSubscriptionsRequest, GetSubscriptionsResponse>
                (
                    type: DataAccessorType.GetSubscriptions,
                    request: request,
                    constructServiceInput: ConstructGetSubscriptionsInput,
                    serviceExecute: (channel, input) =>
                    {
                        return channel.GetSubscriptions(input);
                    },
                    constructDataAccessOutput: ConstructGetSubscriptionsOutput,
                    serviceName: ServiceName,
                    traceActivityId: traceActivityId
                );
        }

        private GetSubscriptionsRequest ConstructGetSubscriptionsInput(Messages.GetSubscriptionsRequest request)
        {
            if (request == null)
                return null;

            GetSubscriptionsRequest input = new GetSubscriptionsRequest();
            if (request.Requester != null)
            {
                input.Requester = new Proxy.Identity()
                {
                    IdentityType = request.Requester.IdentityType,
                    IdentityValue = request.Requester.IdentityValue
                };
            }
            input.SubscriptionId = request.SubscriptionId;
            input.DetailLevel = (GetSubscriptionsDetailLevel)request.DetailLevel;
            input.GetSubscriptionsOfAllPartners = request.GetSubscriptionsOfAllPartners;
            input.PagingOption = request.PagingOption == null ? null : BuildPagingOption(request.PagingOption);
            input.OrderBy = request.OrderBy == null ? null : BuilidOrderBy(request.OrderBy).ToArray();
            input.SubscriptionStatus = request.SubscriptionStatus == null ? null : request.SubscriptionStatus.ToArray();
            input.IsPerpetualOffer = request.IsPerpetualOffer;

            // bdkId
            input.AccountId = request.CallerInfo != null ? request.CallerInfo.AccountId : null;
            input.InvoiceGroupId = request.InvoiceGroupId;
            return input;
        }

        private static Proxy.PagingOption BuildPagingOption(DataModel.PagingOption pagingOption)
        {
            if (pagingOption == null)
            {
                return null;
            }

            Proxy.PagingOption pagingoption = new Proxy.PagingOption();
            pagingoption.Offset = pagingOption.Offset;
            pagingoption.Limit = pagingOption.Limit;
            return pagingoption;
        }

        private static List<Proxy.GetSubscriptionsOrderBy> BuilidOrderBy(List<DataModel.GetSubscriptionsOrderBy> orderBy)
        {
            List<Proxy.GetSubscriptionsOrderBy> subsOrderBy = new List<Proxy.GetSubscriptionsOrderBy>();
            foreach (var orderby in orderBy)
            {
                Proxy.GetSubscriptionsOrderBy subsorderby = new Proxy.GetSubscriptionsOrderBy();
                subsorderby = (Proxy.GetSubscriptionsOrderBy)orderby;
                subsOrderBy.Add(subsorderby);
            }

            return subsOrderBy;
        }

        private Messages.GetSubscriptionsResponse ConstructGetSubscriptionsOutput(Proxy.GetSubscriptionsResponse serviceOutput)
        {
            if (serviceOutput == null)
                return null;

            if (serviceOutput.Ack != Proxy.AckCode.Success)
            {
                throw new DataAccessException(
                    ErrorNamespace.CTPCommerce,
                    serviceOutput.Error.ErrorCode,
                    serviceOutput.Error.Message,
                    serviceOutput.Error.Message,
                    serviceOutput.Error.Detail,
                    (serviceOutput.Ack == AckCode.RetryableFailure));
            }

            Messages.GetSubscriptionsResponse response = new Messages.GetSubscriptionsResponse();
            response.TotalCount = serviceOutput.TotalCount;
            response.SubscriptionInfoList = new List<DataModel.SubscriptionsInfo>();
            if (serviceOutput.SubscriptionInfoSet != null)
            {
                foreach (var subs in serviceOutput.SubscriptionInfoSet)
                {
                    DataModel.SubscriptionsInfo subsInfo = new DataModel.SubscriptionsInfo()
                    {
                        ActivationDate = subs.ActivationDate,
                        AllowNonCSRCancel = subs.AllowNonCSRCancel,
                        AnniversaryDate = subs.AnniversaryDate,
                        BillingPeriod = subs.BillingPeriod,
                        Currency = subs.Currency,
                        CurrentSubscriptionAgreementId = subs.CurrentSubscriptionAgreementId,
                        CurrentSubscriptionAgreementVersion = subs.CurrentSubscriptionAgreementVersion,
                        DelayedCancel = subs.DelayedCancel,
                        EndDate = subs.EndDate,
                        ExtendedDays = subs.ExtendedDays,
                        FreeTrial = subs.FreeTrial,
                        FriendlyName = subs.FriendlyName,
                        FriendlySubscriptionId = subs.FriendlySubscriptionId,
                        HasResources = subs.HasResources,
                        HasTermCommit = subs.HasTermCommit,
                        InstanceCount = subs.InstanceCount,
                        InternalSubscriptionDescription = subs.InternalSubscriptionDescription,
                        IsPerpetualOffer = subs.IsPerpetualOffer,
                        IsReinstatable = subs.IsReinstatable,
                        MonetaryCapStatus = subs.MonetaryCapStatus,
                        NextBillAmount = subs.NextBillAmount.ToString(),
                        NextBillDate = subs.NextBillDate,
                        NextCycle = subs.NextCycle,
                        OfferSku = subs.OfferSku,
                        OfferingGuid = subs.OfferingGuid,
                        OngoingSubscription = subs.OngoingSubscription,
                        PaymentInstrumentId = subs.PaymentInstrumentId,
                        Prepaid = subs.Prepaid,
                        PrepaidDescription = subs.PrepaidDescription,
                        PrepaidRenewal = subs.PrepaidRenewal,
                        PrivacyPolicyId = subs.PrivacyPolicyId,
                        PrivacyPolicyVersion = subs.PrivacyPolicyVersion,
                        ProductClassDescription = subs.ProductClassDescription,
                        ProductClassGuid = subs.ProductClassGuid,
                        ProductDescription = subs.ProductDescription,
                        ProductGuid = subs.ProductGuid,
                        ProductName = subs.ProductName,
                        ProductPartnerGuid = subs.ProductPartnerGuid,
                        ProductRealName = subs.ProductRealName,
                        PurchaseDate = subs.PurchaseDate,
                        RemainingExtensionDays = subs.RemainingExtensionDays,
                        RenewOfferingGuid = subs.RenewOfferingGuid,
                        RenewalGracePeriod = subs.RenewalGracePeriod,
                        ResourceBillingMethod = subs.ResourceBillingMethod,
                        ScheduledInstanceCount = subs.ScheduledInstanceCount,
                        ShippingAddressId = subs.ShippingAddressId,
                        SubscriptionCoBrand = subs.SubscriptionCoBrand,
                        SubscriptionCycleStartDate = subs.SubscriptionCycleStartDate,
                        SubscriptionDescription = subs.SubscriptionDescription,
                        SubscriptionId = subs.SubscriptionId,
                        SubscriptionPriceDescription = subs.SubscriptionPriceDescription,
                        SupportEnabled = subs.SupportEnabled,
                        SupportOfferingDescription = subs.SupportOfferingDescription,
                        SupportOfferingPriceDescription = subs.SupportOfferingPriceDescription,
                        TermCommitCyclesRemaining = subs.TermCommitCyclesRemaining,
                        TermCommitDescription = subs.TermCommitDescription,
                        TermCommitPriceDescription = subs.TermCommitPriceDescription,
                        TermCommitStartDate = subs.TermCommitStartDate,
                        TimeBasedConversionCreditsDays = subs.TimeBasedConversionCreditsDays,
                        RatingRules = ConstructRatingEvent(subs.RatingRules),
                        InvoiceGroupId = subs.InvoiceGroupId,
                        UseStoredValueByDefault = subs.UseStoredValueByDefault,
                        MonetaryCommitmentAmount = subs.MonetaryCommitmentAmount,
                        RemainingMonetaryCommitmentAmount = subs.RemainingMonetaryCommitmentAmount,
                        CategorySet = ConstructCategorySet(subs.CategorySet)
                    };

                    if (subs.ServiceInstanceSet != null)
                    {
                        subsInfo.ServiceInstanceSet = new DataModel.ServiceInstanceSet()
                        {
                            Domain = subs.ServiceInstanceSet.Domain,
                            Priority = subs.ServiceInstanceSet.Priority
                        };

                        if (subs.ServiceInstanceSet.ServiceInstances != null)
                        {
                            subsInfo.ServiceInstanceSet.ServiceInstanceList = new List<DataModel.ServiceInstanceDetail>();
                            foreach (var serviceIns in subs.ServiceInstanceSet.ServiceInstances)
                            {
                                DataModel.ServiceInstanceDetail serviceInstanceDetail = new DataModel.ServiceInstanceDetail()
                                {
                                    BaseServiceInstanceId = serviceIns.BaseServiceInstanceId,
                                    ConversionType = serviceIns.ConversionType,
                                    Details = serviceIns.Details,
                                    Evict = serviceIns.Evict,
                                    IdRef = serviceIns.IdRef,
                                    IndexId = serviceIns.IndexId,
                                    ProvisioningErrorCode = serviceIns.ProvisioningErrorCode,
                                    ProvisioningStatus = serviceIns.ProvisioningStatus,
                                    Reason = serviceIns.Reason,
                                    RemoveService = serviceIns.RemoveService,
                                    RemoveServiceFlag = serviceIns.RemoveServiceFlag,
                                    Role = serviceIns.Role,
                                    ServiceComponentDescription = serviceIns.ServiceComponentDescription,
                                    ServiceComponentId = serviceIns.ServiceComponentId,
                                    ServiceComponentName = serviceIns.ServiceComponentName,
                                    ServiceInstanceFriendlyName = serviceIns.ServiceInstanceFriendlyName,
                                    ServiceInstanceId = serviceIns.ServiceInstanceId,
                                    UserCount = serviceIns.UserCount,
                                };

                                if (serviceIns.ServicesUsers != null)
                                {
                                    serviceInstanceDetail.ServicesUsers = new List<DataModel.Identity>();
                                    foreach (var identity in serviceIns.ServicesUsers)
                                    {
                                        serviceInstanceDetail.ServicesUsers.Add(new DataModel.Identity
                                        {
                                            IdentityType = identity.IdentityType,
                                            IdentityValue = identity.IdentityValue,
                                            PassportMemberName = identity.IdentityEmail
                                        });
                                    }
                                }

                                subsInfo.ServiceInstanceSet.ServiceInstanceList.Add(serviceInstanceDetail);
                            }
                        }
                    }

                    if (subs.SubscriptionStatusInfo != null)
                    {
                        subsInfo.SubscriptionStatusInfo = new DataModel.SubscriptionStatusInfo
                        {
                            SubscriptionExtraStatus = subs.SubscriptionStatusInfo.SubscriptionExtraStatus,
                            SubscriptionId = subs.SubscriptionStatusInfo.SubscriptionId,
                            SubscriptionStatus = subs.SubscriptionStatusInfo.SubscriptionStatus,
                        };

                        if (subs.SubscriptionStatusInfo.ViolationIdSet != null)
                        {
                            subsInfo.SubscriptionStatusInfo.ViolationIdSet = new List<int>(subs.SubscriptionStatusInfo.ViolationIdSet);
                        }
                    }

                    if (subs.SignatureInfoSet != null)
                    {
                        subsInfo.SignatureInfoSet = new List<DataModel.SignatureInfoSetSignatureInfo>();
                        foreach (var sis in subs.SignatureInfoSet)
                        {
                            DataModel.SignatureInfoSetSignatureInfo sissi = new DataModel.SignatureInfoSetSignatureInfo();
                            sissi.SignedPolicyId = sis.SignedPolicyId;
                            sissi.SignedPolicyVersion = sis.SignedPolicyVersion;
                            sissi.SignedPolicyName = sis.SignedPolicyName;
                            sissi.SignatureDate = sis.SignatureDate;
                            sissi.Identity = new DataModel.Identity
                            {
                                IdentityType = sis.Identity.IdentityType,
                                IdentityValue = sis.Identity.IdentityValue,
                                PassportMemberName = sis.Identity.IdentityEmail
                            };
                            sissi.CurrentVersionOfSignedPolicy = sis.CurrentVersionOfSignedPolicy;
                            sissi.CurrentVersionsName = sis.CurrentVersionsName;
                            sissi.OfferingGuid = sis.OfferingGuid;
                            sissi.OfferingDescription = sis.OfferingDescription;
                            sissi.DealIndex = sis.DealIndex;
                            subsInfo.SignatureInfoSet.Add(sissi);
                        }
                    }

                    response.SubscriptionInfoList.Add(subsInfo);
                }
            }

            return response;
        }

        private static List<DataModel.RatingEvent> ConstructRatingEvent(Proxy.RatingEvent[] ratingEvent)
        {
            if (ratingEvent == null)
            {
                return null;
            }

            List<DataModel.RatingEvent> ratingEventList = new List<DataModel.RatingEvent>();
            foreach (var re in ratingEvent)
            {
                DataModel.RatingEvent ratingevent = new DataModel.RatingEvent();
                if (re.RuleInfo != null && re.RuleInfo.Count() > 0)
                {
                    List<DataModel.RatingRule> ratingRuleList = new List<DataModel.RatingRule>();
                    foreach (var ri in re.RuleInfo)
                    {
                        DataModel.RatingRule ratingrule = new DataModel.RatingRule();
                        ratingrule.Amount = ri.Amount;
                        ratingrule.CycleEnd = ri.CycleEnd;
                        ratingrule.CycleStart = ri.CycleStart;
                        ratingrule.CycleUnit = ri.CycleUnit;
                        ratingrule.GuidReference = ri.GuidReference;
                        if (!string.IsNullOrEmpty(ri.RatingRuleMeta))
                        {
                            ratingrule.RatingRuleMeta = ri.RatingRuleMeta;
                            ratingrule.TierPricing = ParseTierPricing(ri.RatingRuleMeta);
                        }

                        ratingrule.RatingRuleMeta = ri.RatingRuleMeta;
                        ratingrule.ResourceDescription = ri.ResourceDescription;
                        ratingrule.RuleType = (DataModel.RatingRuleType)ri.RuleType;
                        ratingrule.ResourceGuidId = ri.ResourceGuid.HasValue ? ri.ResourceGuid.Value : System.Guid.Empty;
                        ratingrule.RevenueSku = ri.RevenueSku;
                        ratingRuleList.Add(ratingrule);
                    }
                    ratingevent.RuleInfo = ratingRuleList;
                }
                ratingevent.RatingEventType = re.RatingEventType;
                ratingEventList.Add(ratingevent);
            }
            return ratingEventList;
        }

        private static List<TierPricing> ParseTierPricing(string ratingRuleMeta)
        {
            List<TierPricing> tierPricing = new List<TierPricing>();
            using (XmlReader reader = XmlReader.Create(new StringReader(ratingRuleMeta)))
            {
                XElement root = XElement.Load(reader);
                foreach (var tp in root.Descendants("Pricing"))
                {
                    TierPricing t = new TierPricing();
                    t.MinValue = Convert.ToInt32(tp.Element("MinValue").Value, NumberFormatInfo.InvariantInfo);
                    t.ChargeAmount = tp.Element("ChargeAmount").Value;
                    tierPricing.Add(t);
                }
            }

            return tierPricing;
        }

        private static List<DataModel.Category> ConstructCategorySet(Proxy.Category[] categorySet)
        {
            if (categorySet == null)
            {
                return null;
            }

            List<DataModel.Category> categoryList = new List<DataModel.Category>();
            foreach (var category in categorySet)
            {
                DataModel.Category c = new DataModel.Category();
                c.CategoryGuid = category.CategoryGuid;
                c.CategoryName = category.CategoryName;
                categoryList.Add(c);
            }
            return categoryList;
        }
    }
}