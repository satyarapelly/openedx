// <copyright file="RegexConstants.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    public static class RegexConstants
    {
        public const string XmlString = @"^[\u0009\u000A\u000D\u0020-\uD7FF\uE000-\uFFFD\u10000-\u10FFFF]*$";
        public const string CountryCode = @"^[a-zA-Z]{2}$";
        public const string Email = @"^(([A-Za-z0-9]+_+)|([A-Za-z0-9]+\-+)|([A-Za-z0-9]+\.+)|([A-Za-z0-9]+\++))*[A-Za-z0-9]+@(([a-zA-Z_0-9]+\-+)|([a-zA-Z_0-9]+\.))*[a-zA-Z_0-9]{1,63}\.[a-zA-Z]{2,6}$";
        public const string Locale = @"^\s*[a-zA-Z]{2}\-[a-zA-Z]{2}\s*$";
        public const string Currency = @"^[a-zA-Z]{3}$";
        public const string DateType = @"^[0-9]{4}-[0-9]{2}-[0-9]{2}$";
        public const string PaymentMethodId = @"^(\S{16}){0,1}$";  // match non whilespace chars

        //only for Brazil CPF/CNPJ
        //CPF number consists of 11 digits, for example: 390.533.447-05
        public const string CPF = @"^\d{3}\.\d{3}\.\d{3}-\d{2}$";

        //he CNPJ number consists of 14 digits, for example: 12.345.678/0001-96
        public const string CNPJ = @"^\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}$";

        //he CCM number consists of 8 digits, for example: 3.458.505-2
        public const string CCM = @"^\d{1}\.\d{3}\.\d{3}-\d{1}$";
    }
}
