// <copyright file="GetAccountInfoRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class GetAccountInfoRequest : AbstractRequest, IValidatableObject
    {
        public override int ApiId
        {
            get { return (int)DataAccessorType.GetAccountInfo; }
        }

        [NotNullValidator(Tag = "GetAccountInfoRequest")]
        [ObjectValidator(Tag = "GetAccountInfoRequest")]
        [DataMember]
        public APIContext APIContext { get; set; }

        [IgnoreNulls]
        [DataMember]
        public CallerInfo CallerInfo { get; set; }

        [NotNullValidator(Tag = "GetAccountInfoRequest")]
        [ObjectValidator(Tag = "GetAccountInfoRequest")]
        [DataMember]
        public AccountSearchCriteria SearchCriteria { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Legacy code moved from PCS. Needed for serialization")]
        [IgnoreNulls, PropertyCollectionValidator(Tag = "GetAccountInfoRequest")]
        [DataMember]
        public List<Property> Filters { get; set; }

        public SearchAccountFilters SearchAccountFilters { get; set; }

        [XmlIgnore]
        public override ulong DelegaterId
        {
            get
            {
                if (CallerInfo != null && CallerInfo.Delegator != null)
                    return ulong.Parse(CallerInfo.Delegator.IdentityValue);
                else
                    return 0;
            }
        }

        [XmlIgnore]
        public override Identity Requester
        {
            get
            {
                return CallerInfo == null ? null : CallerInfo.Requester;
            }
        }

        [XmlIgnore]
        public override Identity Delegater
        {
            get
            {
                return CallerInfo == null ? null : CallerInfo.Delegator;
            }
        }

        [XmlIgnore]
        public override string ObjectId
        {
            get
            {
                return CallerInfo == null ? null : CallerInfo.AccountId;
            }
        }

        [XmlIgnore]
        public override Guid EffectiveTrackingGuid
        {
            get { return APIContext == null ? Guid.Empty : APIContext.TrackingGuid; }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }
}
