using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThirdParty.SellerService.SellerProvider
{
    interface ISellerProvider
    {
        Task<Stripe.Account> CreateSellerAccount();

        Task AddSellerRepresentative(
            string sellerId,
            string firstName,
            string lastName,
            string email,
            string phone,
            string idNumber,
            string ssnLast4,
            long dobDay,
            long dobMonth,
            long dobYear);
    }
}
