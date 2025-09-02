// <copyright file="ErrorType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class ErrorType : ICloneable
    {
        [DataMember]
        public bool Retriable { get; set; }

        [DataMember]
        public bool IsSystemError { get; set; }


        [DataMember]
        public int ErrorCode { get; set; }

        [DataMember]
        public string ErrorShortMessage { get; set; }

        [DataMember]
        public string ErrorLongMessage { get; set; }

        [DataMember]
        public string ErrorDescription { get; set; }

        #region ICloneable Members

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
