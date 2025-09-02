using System;
using System.Threading.Tasks;

namespace ThirdParty.SellerService.SellerProvider
{
    public class StripeSeller : ISellerProvider
    {
        public async Task<Stripe.Account> CreateSellerAccount()
        {
            var accountService = new Stripe.AccountService();

            var companyTaxId = "000000000";
            var bankAccountNumber = "000123456789";
            var bankRoutingNumber = "110000000";

            var companyName = "Radware, Inc.";
            var accountCreateOptions1 = new Stripe.AccountCreateOptions()
            {
                Type = "custom",
                Country = "US",
                BusinessType = "company",
                Email = "info@radware.com",

                Capabilities = new Stripe.AccountCapabilitiesOptions()
                {
                    CardPayments = new Stripe.AccountCapabilitiesCardPaymentsOptions()
                    {
                        Requested = true
                    },
                    Transfers = new Stripe.AccountCapabilitiesTransfersOptions()
                    {
                        Requested = true
                    }
                },

                ExternalAccount = new Stripe.AccountBankAccountOptions()
                {
                    Country = "US",
                    Currency = "usd",
                    AccountNumber = bankAccountNumber,
                    RoutingNumber = bankRoutingNumber,
                    AccountHolderName = companyName,
                },

                BusinessProfile = new Stripe.AccountBusinessProfileOptions()
                {
                    Mcc = "5045",
                    Url = "www.radware.com",
                    ProductDescription = "Azure Marketplace - " + companyName
                },

                Company = new Stripe.AccountCompanyOptions()
                {
                    Name = companyName,
                    Address = new Stripe.AddressOptions()
                    {
                        Line1 = "575 Corporate Drive",
                        City = "Mahwah",
                        State = "NJ",
                        PostalCode = "07430",
                        Country = "US",
                    },
                    Phone = "8888675309",
                    TaxId = companyTaxId,
                    VatId = "",
                },

                TosAcceptance = new Stripe.AccountTosAcceptanceOptions()
                {
                    Date = DateTime.UtcNow,
                    Ip = "172.18.80.19"
                }
            };
            var account1 = await accountService.CreateAsync(accountCreateOptions1);

            return account1;
        }

        public async Task AddSellerRepresentative(
            string sellerId,
            string firstName,
            string lastName,
            string email,
            string phone,
            string idNumber,
            string ssnLast4,
            long dobDay,
            long dobMonth,
            long dobYear
        )
        {
            var personService = new Stripe.PersonService();

            var sellerRep = new Stripe.PersonCreateOptions()
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                IdNumber = idNumber,
                SsnLast4 = ssnLast4,

                Address = new Stripe.AddressOptions()
                {
                    Line1 = "575 Corporate Drive",
                    City = "Mahwah",
                    State = "NJ",
                    PostalCode = "07430",
                    Country = "US",
                },

                Dob = new Stripe.DobOptions()
                {
                    Day = dobDay,
                    Month = dobMonth,
                    Year = dobYear
                },

                Relationship = new Stripe.PersonRelationshipOptions()
                {
                    Owner = true,
                    PercentOwnership = 80,
                    Title = "CEO"
                }
            };
            await personService.CreateAsync(sellerId, sellerRep);
        }
    }
}    