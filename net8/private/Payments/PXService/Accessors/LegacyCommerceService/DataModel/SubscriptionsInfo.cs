// <copyright file="SubscriptionsInfo.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class SubscriptionsInfo
    {
        [DataMember]
        public DateTime? ActivationDate { get; set; }

        [DataMember]
        public bool AllowNonCSRCancel { get; set; }

        [DataMember]
        public int AnniversaryDate { get; set; }

        [DataMember]
        public uint BillingPeriod { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<Category> CategorySet { get; set; }

        [DataMember]
        public string Currency { get; set; }

        [DataMember]
        public string CurrentSubscriptionAgreementId { get; set; }

        [DataMember]
        public string CurrentSubscriptionAgreementVersion { get; set; }

        [DataMember]
        public bool DelayedCancel { get; set; }

        [DataMember]
        public DelayedConversion DelayedConversion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<DiscountInstanceInfo> DiscountInstanceInfoSet { get; set; }

        [DataMember]
        public DateTime? EndDate { get; set; }

        [DataMember]
        public int ExtendedDays { get; set; }

        [DataMember]
        public bool FreeTrial { get; set; }

        [DataMember]
        public string FriendlyName { get; set; }

        [DataMember]
        public string FriendlySubscriptionId { get; set; }

        [DataMember]
        public bool HasResources { get; set; }

        [DataMember]
        public bool HasTermCommit { get; set; }

        [DataMember]
        public int? InstanceCount { get; set; }

        [DataMember]
        public string InternalSubscriptionDescription { get; set; }

        [DataMember]
        public bool IsPerpetualOffer { get; set; }

        [DataMember]
        public bool IsReinstatable { get; set; }

        [DataMember]
        public string MonetaryCapStatus { get; set; }

        [DataMember]
        public string NextBillAmount { get; set; }

        [DataMember]
        public DateTime? NextBillDate { get; set; }

        [DataMember]
        public int? NextCycle { get; set; }

        [DataMember]
        public string OfferSku { get; set; }

        [DataMember]
        public Guid OfferingGuid { get; set; }

        [DataMember]
        public bool OngoingSubscription { get; set; }

        [DataMember]
        public string PaymentInstrumentId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<string> PaymentInstrumentIdSet { get; set; }

        [DataMember]
        public bool Prepaid { get; set; }

        [DataMember]
        public string PrepaidDescription { get; set; }

        [DataMember]
        public bool PrepaidRenewal { get; set; }

        [DataMember]
        public string PrivacyPolicyId { get; set; }

        [DataMember]
        public string PrivacyPolicyVersion { get; set; }

        [DataMember]
        public string ProductClassDescription { get; set; }

        [DataMember]
        public Guid ProductClassGuid { get; set; }

        [DataMember]
        public string ProductDescription { get; set; }

        [DataMember]
        public Guid ProductGuid { get; set; }

        [DataMember]
        public string ProductName { get; set; }

        [DataMember]
        public Guid ProductPartnerGuid { get; set; }

        [DataMember]
        public string ProductRealName { get; set; }

        [DataMember]
        public DateTime? PurchaseDate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<RatingEvent> RatingRules { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<ReferralInfo> ReferralSet { get; set; }

        [DataMember]
        public int RemainingExtensionDays { get; set; }

        [DataMember]
        public Guid? RenewOfferingGuid { get; set; }

        [DataMember]
        public int RenewalGracePeriod { get; set; }

        [DataMember]
        public string ResourceBillingMethod { get; set; }

        [DataMember]
        public int? ScheduledInstanceCount { get; set; }

        [DataMember]
        public ServiceInstanceSet ServiceInstanceSet { get; set; }

        [DataMember]
        public string ShippingAddressId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<SignatureInfoSetSignatureInfo> SignatureInfoSet { get; set; }

        [DataMember]
        public string SubscriptionCoBrand { get; set; }

        [DataMember]
        public DateTime? SubscriptionCycleStartDate { get; set; }

        [DataMember]
        public string SubscriptionDescription { get; set; }

        [DataMember]
        public string SubscriptionId { get; set; }

        [DataMember]
        public string SubscriptionPriceDescription { get; set; }

        [DataMember]
        public SubscriptionStatusInfo SubscriptionStatusInfo { get; set; }

        [DataMember]
        public bool SupportEnabled { get; set; }

        [DataMember]
        public string SupportOfferingDescription { get; set; }

        [DataMember]
        public string SupportOfferingPriceDescription { get; set; }

        [DataMember]
        public int TermCommitCyclesRemaining { get; set; }

        [DataMember]
        public string TermCommitDescription { get; set; }

        [DataMember]
        public string TermCommitPriceDescription { get; set; }

        [DataMember]
        public DateTime? TermCommitStartDate { get; set; }

        [DataMember]
        public int TimeBasedConversionCreditsDays { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<TokenInfo> TokenInfoSet { get; set; }

        [DataMember]
        public string InvoiceGroupId { get; set; }

        [DataMember]
        public bool UseStoredValueByDefault { get; set; }

        /// <summary>
        /// Indicates commitment fee of monetary commitment offer.
        /// </summary>
        [DataMember]
        public decimal? MonetaryCommitmentAmount { get; set; }

        /// <summary>
        /// Indicates commitment fee which can be using to deduct resource.
        /// </summary>
        [DataMember]
        public decimal? RemainingMonetaryCommitmentAmount { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class Category
    {
        [DataMember]
        public Guid CategoryGuid { get; set; }

        [DataMember]
        public string CategoryName { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class DelayedConversion
    {
        [DataMember]
        public DateTime ConversionDate { get; set; }

        [DataMember]
        public Guid OfferingGuid { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class DiscountInstanceInfo
    {
        [DataMember]
        public DiscountInfo DiscountInfo { get; set; }

        [DataMember]
        public string DiscountInstanceId { get; set; }

        [DataMember]
        public string DiscountInstanceStatus { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class RatingEvent
    {
        [DataMember]
        public string RatingEventType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<RatingRule> RuleInfo { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class ReferralInfo
    {
        [DataMember]
        public DateTime CreatedDateTime { get; set; }

        [DataMember]
        public string Data { get; set; }

        [DataMember]
        public Guid PartnerId { get; set; }

        [DataMember]
        public string PartnerName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Legacy code")]
        [DataMember]
        public string Type { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class ServiceInstanceSet
    {
        [DataMember]
        public string Domain { get; set; }

        [DataMember]
        public MetaData MetaData { get; set; }

        [DataMember]
        public int Priority { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<ServiceInstanceDetail> ServiceInstanceList { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class SignatureInfoSetSignatureInfo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "signed", Justification = "Legacy code")]
        [DataMember]
        public int CurrentVersionOfSignedPolicy { get; set; }

        [DataMember]
        public string CurrentVersionsName { get; set; }

        [DataMember]
        public int DealIndex { get; set; }

        [DataMember]
        public Identity Identity { get; set; }

        [DataMember]
        public string OfferingDescription { get; set; }

        [DataMember]
        public Guid OfferingGuid { get; set; }

        [DataMember]
        public DateTime SignatureDate { get; set; }

        [DataMember]
        public string SignedPolicyId { get; set; }

        [DataMember]
        public string SignedPolicyName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "signed", Justification = "Legacy code")]
        [DataMember]
        public int SignedPolicyVersion { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class SubscriptionStatusInfo
    {
        [DataMember]
        public string SubscriptionExtraStatus { get; set; }

        [DataMember]
        public string SubscriptionId { get; set; }

        [DataMember]
        public string SubscriptionStatus { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<int> ViolationIdSet { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class TokenInfo
    {
        [DataMember]
        public string TokenPinLastDigits { get; set; }

        [DataMember]
        public int TokenPinLength { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class DiscountInfo
    {
        [DataMember]
        public string DiscountDescription { get; set; }

        [DataMember]
        public Guid DiscountGuid { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<DiscountRule> DiscountRuleSet { get; set; }

        [DataMember]
        public string DiscountTitle { get; set; }

        [DataMember]
        public string DiscountType { get; set; }

        [DataMember]
        public string SupportDiscountDescription { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class RatingRule
    {
        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public int CycleEnd { get; set; }

        [DataMember]
        public int CycleStart { get; set; }

        [DataMember]
        public int CycleUnit { get; set; }

        [DataMember]
        public Guid GuidReference { get; set; }

        [DataMember]
        public string RatingRuleMeta { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<TierPricing> TierPricing { get; set; }

        [DataMember]
        public string ResourceDescription { get; set; }

        [DataMember]
        public Guid ResourceGuidId { get; set; }

        [DataMember]
        public string RevenueSku { get; set; }

        [DataMember]
        public RatingRuleType RuleType { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "Legacy code")]
    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class MetaData
    {
        [DataMember]
        public string BasicAuthUser { get; set; }

        [DataMember]
        public Certificate Certificate { get; set; }

        [DataMember]
        public string Endpoint { get; set; }

        [DataMember]
        public string Method { get; set; }

        [DataMember]
        public int PPEmulator { get; set; }

        [DataMember]
        public PassportBits PassportBits { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<PassportBits> PassportBitsSet { get; set; }

        [DataMember]
        public string SoapParametersWSDLFile { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Legacy code")]
        [DataMember]
        public string Type { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class ServiceInstanceDetail
    {
        [DataMember]
        public string BaseServiceInstanceId { get; set; }

        [DataMember]
        public string ConversionType { get; set; }

        [DataMember]
        public string Details { get; set; }

        [DataMember]
        public bool Evict { get; set; }

        [DataMember]
        public string IdRef { get; set; }

        [DataMember]
        public string IndexId { get; set; }

        [DataMember]
        public MetaData MetaData { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Legacy code")]
        [ElementNotNull]
        [PropertyCollectionValidator]
        [DataMember]
        public Property[] PropertyBag { get; set; }

        [DataMember]
        public string ProvisioningErrorCode { get; set; }

        [DataMember]
        public string ProvisioningStatus { get; set; }

        [DataMember]
        public int Reason { get; set; }

        [DataMember]
        public string RemoveService { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag", Justification = "Legacy code")]
        [DataMember]
        public int RemoveServiceFlag { get; set; }

        [DataMember]
        public string Role { get; set; }

        [DataMember]
        public ServiceComponent ServiceComponent { get; set; }

        [DataMember]
        public string ServiceComponentDescription { get; set; }

        [DataMember]
        public string ServiceComponentId { get; set; }

        [DataMember]
        public string ServiceComponentName { get; set; }

        [DataMember]
        public string ServiceInstanceFriendlyName { get; set; }

        [DataMember]
        public string ServiceInstanceId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<Identity> ServicesUsers { get; set; }

        [DataMember]
        public int UserCount { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class DiscountRule
    {
        [DataMember]
        public string Event { get; set; }

        [DataMember]
        public int FirstCycle { get; set; }

        [DataMember]
        public int LastCycle { get; set; }

        [DataMember]
        public int Percentage { get; set; }

        [DataMember]
        public string Rule { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public enum RatingRuleType : int
    {
        [EnumMember]
        Charge = 0,
        [EnumMember]
        Proration = 1,
        [EnumMember]
        InitResource = 2,
        [EnumMember]
        ConvertResource = 3,
        [EnumMember]
        Usage = 5,
        [EnumMember]
        CarryForward = 6,
        [EnumMember]
        Credit = 7,
        [EnumMember]
        Offset = 8,
        [EnumMember]
        Transfer = 9,
        [EnumMember]
        Adjustment = 10,
        [EnumMember]
        BalanceForward = 11,
        [EnumMember]
        ChargeBack = 12,
        [EnumMember]
        WriteOff = 13,
        [EnumMember]
        ExpireResources = 14,
        [EnumMember]
        UsageCarryForward = 15,
        [EnumMember]
        PointsPurchaseCharge = 19,
        [EnumMember]
        OneTimePurchaseCharge = 20,
        [EnumMember]
        InstanceProrate = 24,
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class Certificate
    {
        [DataMember]
        public string CertificateName { get; set; }

        [DataMember]
        public string CertificateStore { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class PassportBits
    {
        [DataMember]
        public int Mask { get; set; }

        [DataMember]
        public int Value { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class ServiceComponent
    {
        [DataMember]
        public string IdRef { get; set; }

        [DataMember]
        public string InstanceCount { get; set; }

        [DataMember]
        public string MaxRoles { get; set; }

        [DataMember]
        public string ServiceComponentId { get; set; }

        [DataMember]
        public string ServiceComponentName { get; set; }
    }
}