// <copyright file="TransactionStatusReason.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TransactionStatusReason
    {
        [EnumMember(Value = "01")]
        TSR01,

        [EnumMember(Value = "02")]
        TSR02,

        [EnumMember(Value = "03")]
        TSR03,

        [EnumMember(Value = "04")]
        TSR04,

        [EnumMember(Value = "05")]
        TSR05,

        [EnumMember(Value = "06")]
        TSR06,

        [EnumMember(Value = "07")]
        TSR07,

        [EnumMember(Value = "08")]
        TSR08,

        [EnumMember(Value = "09")]
        TSR09,

        [EnumMember(Value = "10")]
        TSR10,

        [EnumMember(Value = "11")]
        TSR11,

        [EnumMember(Value = "12")]
        TSR12,

        [EnumMember(Value = "13")]
        TSR13,

        [EnumMember(Value = "14")]
        TSR14,

        [EnumMember(Value = "15")]
        TSR15,

        [EnumMember(Value = "16")]
        TSR16,

        [EnumMember(Value = "17")]
        TSR17,

        [EnumMember(Value = "18")]
        TSR18,

        [EnumMember(Value = "19")]
        TSR19,

        [EnumMember(Value = "20")]
        TSR20,

        [EnumMember(Value = "21")]
        TSR21,

        // 22–79 = Reserved for EMVCo future use (values invalid until defined by EMVCo)
        // 80–99 = Reserved for DS use
    }
}