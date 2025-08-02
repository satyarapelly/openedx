// <copyright file="CreateAccountRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

    [HasSelfValidation]
    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class CreateAccountRequest : AbstractRequest
    {
        public override int ApiId
        {
            get { return (int)DataAccessorType.CreateAccount; }
        }

        [NotNullValidator(Tag = "CreateAccountRequest")]
        [ObjectValidator(Tag = "CreateAccountRequest")]
        [DataMember]
        public APIContext APIContext { get; set; }

        [DataMember]
        public CallerInfo CallerInfo { get; set; }

        [DataMember]
        public Guid OnBehalfOfPartner { get; set; }

        [ObjectValidator(Tag = "CreateAccountRequest")]
        [DataMember]
        public PayinPayoutAccount Account { get; set; }

        [XmlIgnore]
        public override Guid EffectiveTrackingGuid
        {
            get { return APIContext == null ? Guid.Empty : APIContext.TrackingGuid; }
        }
    }
}
