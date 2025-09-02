// <copyright file="AdminPrincipal.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Security.Principal;

    public class AdminPrincipal : GenericPrincipal
    {
        public AdminPrincipal(IIdentity identity)
            : base(identity, null)
        {
        }

        public override bool IsInRole(string role)
        {
            if (role != null)
            {
                return true;
            }

            return false;
        }
    }
}