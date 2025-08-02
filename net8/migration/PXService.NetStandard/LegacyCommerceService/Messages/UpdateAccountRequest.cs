// <copyright file="UpdateAccountRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
    using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class UpdateAccountRequest : AbstractRequest, IValidatableObject
    {
        public override int ApiId
        {
            get { return (int)DataAccessorType.UpdateAccount; }
        }

        [NotNullValidator(Tag = "UpdateAccountRequest")]
        [ObjectValidator(Tag = "UpdateAccountRequest")]
        [DataMember]
        public APIContext APIContext { get; set; }

        [DataMember]
        public CallerInfo CallerInfo { get; set; }

        [DataMember]
        public Guid OnBehalfOfPartner { get; set; }

        [ObjectValidator(Tag = "UpdateAccountRequest")]
        [DataMember]
        public PayinPayoutAccount Account { get; set; }

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
