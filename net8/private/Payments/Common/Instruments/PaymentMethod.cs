// <copyright file="PaymentMethod.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    /// <summary>
    /// placeholder for PaymentMethod definition
    /// </summary>
    public class PaymentMethod
    {
        public PaymentMethod(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
    }
}
