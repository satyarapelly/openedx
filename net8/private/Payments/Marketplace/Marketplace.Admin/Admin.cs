using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using ThirdParty.Model;

namespace Marketplace.Admin
{
    class Admin
    {
        // The Cosmos client instance
        private CosmosClient cosmosClient;

        private Database db;

        private string sellerIdRadware = "F765A697-A3A1-49C8-B277-283E59D6FB29";
        private string sellerIdStoneridge = "13F108EF-AAEA-4836-8F9C-D1E84EBAAE5F";

        public static async Task Main(string[] args)
        {
            try
            {
                Admin admin = new Admin();

                admin.db = await admin.cosmosClient.CreateDatabaseIfNotExistsAsync(
                    id: Constants.MarketplaceDbName);
                foreach (string arg in args)
                {
                    if (string.Equals(arg, "/onboardSellers", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"\n----------------------------------------------------------");
                        Console.WriteLine($"Onboard Sellers + Offers");
                        Console.WriteLine($"----------------------------------------------------------");
                        // Clear data in the local database
                        await admin.ClearOrCreateContainerAsync(Constants.ContainerNames.Catalog);
                        await admin.ClearOrCreateContainerAsync(Constants.ContainerNames.Orders);
                        await admin.ClearOrCreateContainerAsync(Constants.ContainerNames.Sellers);

                        await admin.OnboardSellersAsync();

                        await admin.ClearOrCreateContainerAsync(Constants.ContainerNames.Orders);
                        await admin.OnboardOffersAsync();
                    }
                    if (string.Equals(arg, "/onboardBuyers", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"\n----------------------------------------------------------");
                        Console.WriteLine($"Onboard Buyers + PIs");
                        Console.WriteLine($"----------------------------------------------------------");

                        await admin.ClearOrCreateContainerAsync(Constants.ContainerNames.Customers);
                        await admin.OnboardBuyersAsync();
                    }
                    else if (string.Equals(arg, "/clearDatabase", StringComparison.OrdinalIgnoreCase))
                    {
                        await admin.ClearOrCreateContainerAsync(Constants.ContainerNames.Orders);
                        await admin.ClearOrCreateContainerAsync(Constants.ContainerNames.Catalog);
                        await admin.ClearOrCreateContainerAsync(Constants.ContainerNames.Customers);
                        await admin.ClearOrCreateContainerAsync(Constants.ContainerNames.Sellers);
                    }
                    else if (string.Equals(args[0], "/createInvoice", StringComparison.OrdinalIgnoreCase))
                    {
                        await admin.CreateInvoiceAsync(args[1], args[2]);
                    }
                }

            }
            catch (Exception e)
            { 
                Console.WriteLine("Error: {0}", e);
            }
        }

        public Admin()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(
                    "appsettings.json",
                    optional: false,
                    reloadOnChange: true)
                .AddUserSecrets<Admin>()
                .Build();

            // Initialize Cosmos Client
            this.cosmosClient = new CosmosClient(
                    connectionString: config.GetSection("MarketplaceDb").Value);

            // Initialize Stripe Secret Key
            Stripe.StripeConfiguration.ApiKey = config.GetSection("StripeSecretKey").Value;
        }

        public async Task OnboardSellersAsync()
        {
            
            var sellersToAdd = await CreateSellersInStripeAsync();

            await SetItemsInContainerAsync<Seller>(
                items: sellersToAdd,
                containerName: Constants.ContainerNames.Sellers,
                getItemName: s => s.Name);
        }


        public async Task<List<Seller>> CreateSellersInStripeAsync()
        {
            Console.WriteLine("Creating Seller accounts on Stripe");
            var accountService = new Stripe.AccountService();
            var personService = new Stripe.PersonService();

            var personIdNumber = "000000000";
            var personSsnLast4 = "0000";
            var personDobDay = 1;
            var personDobMonth = 1;
            var personDobYear = 1901;
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

            Console.WriteLine($"  - Created {companyName}");
            
            var adminOptions1 = new Stripe.PersonCreateOptions()
            {
                FirstName = "Alice",
                LastName = "Representative",
                Email = "admin@radware.com",
                Phone = "8882345763",
                IdNumber = personIdNumber,
                SsnLast4 = personSsnLast4,

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
                    Day = personDobDay,
                    Month = personDobMonth,
                    Year = personDobYear
                },

                Relationship = new Stripe.PersonRelationshipOptions()
                {
                    Executive = true,
                    Representative = true,
                    Title = "COO"
                }
            };
            var ownerOptions1 = new Stripe.PersonCreateOptions()
            {
                FirstName = "Bob",
                LastName = "Owner",
                Email = "owner@radware.com",
                Phone = "8882345763",
                IdNumber = personIdNumber,
                SsnLast4 = personSsnLast4,

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
                    Day = personDobDay,
                    Month = personDobMonth,
                    Year = personDobYear
                },

                Relationship = new Stripe.PersonRelationshipOptions()
                {
                    Owner = true,
                    PercentOwnership = 80,
                    Title = "CEO"
                }
            };
            await personService.CreateAsync(account1.Id, adminOptions1);
            await personService.CreateAsync(account1.Id, ownerOptions1);

            var accountUpdateOptions1 = new Stripe.AccountUpdateOptions()
            {
                Company = new Stripe.AccountCompanyOptions()
                {
                    OwnersProvided = true
                }
            };
            account1 = await accountService.UpdateAsync(account1.Id, accountUpdateOptions1);

            //Console.WriteLine("    - Added representative and owner");

            companyName = "Stoneridge Software LLC";
            var accountCreateOptions2 = new Stripe.AccountCreateOptions()
            {
                Type = "custom",
                Country = "US",
                BusinessType = "company",
                Email = "solutions@stoneridgesoftware.com",

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
                    Url = "www.stoneridgesoftware.com",
                    ProductDescription = "Azure Marketplace - " + companyName
                },

                Company = new Stripe.AccountCompanyOptions()
                {
                    Name = companyName,
                    Address = new Stripe.AddressOptions()
                    {
                        Line1 = "5775 Wayzata Blvd",
                        Line2 = "Suite 690",
                        City = "Minneapolis",
                        State = "MN",
                        PostalCode = "55416",
                        Country = "US",
                    },
                    Phone = "6123544966",
                    TaxId = companyTaxId,
                    VatId = "",
                },

                TosAcceptance = new Stripe.AccountTosAcceptanceOptions()
                {
                    Date = DateTime.UtcNow,
                    Ip = "172.18.80.19"
                }
            };
            var account2 = await accountService.CreateAsync(accountCreateOptions2);
            Console.WriteLine($"  - Created {companyName}");

            var adminOptions2 = new Stripe.PersonCreateOptions()
            {
                FirstName = "Charlie",
                LastName = "Representative",
                Email = "admin@stoneridgesoftware.com",
                Phone = "8882345763",
                IdNumber = personIdNumber,
                SsnLast4 = personSsnLast4,

                Address = new Stripe.AddressOptions()
                {
                    Line1 = "5775 Wayzata Blvd",
                    Line2 = "Suite 690",
                    City = "Minneapolis",
                    State = "MN",
                    PostalCode = "55416",
                    Country = "US",
                },

                Dob = new Stripe.DobOptions()
                {
                    Day = personDobDay,
                    Month = personDobMonth,
                    Year = personDobYear
                },

                Relationship = new Stripe.PersonRelationshipOptions()
                {
                    Executive = true,
                    Representative = true,
                    Title = "COO"
                }
            };
            var ownerOptions2 = new Stripe.PersonCreateOptions()
            {
                FirstName = "Dave",
                LastName = "Owner",
                Email = "owner@stoneridgesoftware.com",
                Phone = "8882345763",
                IdNumber = "000000000",

                Address = new Stripe.AddressOptions()
                {
                    Line1 = "5775 Wayzata Blvd",
                    Line2 = "Suite 690",
                    City = "Minneapolis",
                    State = "MN",
                    PostalCode = "55416",
                    Country = "US",
                },

                Dob = new Stripe.DobOptions()
                {
                    Day = 1,
                    Month = 1,
                    Year = 1901
                },

                Relationship = new Stripe.PersonRelationshipOptions()
                {
                    Owner = true,
                    PercentOwnership = 80,
                    Title = "CEO"
                }
            };
            await personService.CreateAsync(account2.Id, adminOptions2);
            await personService.CreateAsync(account2.Id, ownerOptions2);

            var accountUpdateOptions = new Stripe.AccountUpdateOptions()
            {
                Company = new Stripe.AccountCompanyOptions()
                {
                    OwnersProvided = true
                }
            };
            account2 = await accountService.UpdateAsync(account2.Id, accountUpdateOptions);
            //Console.WriteLine("    - Added representative and owner");

            var sellers = SeedData.GetSellers();
            foreach(var seller in sellers)
            {
                if (string.Equals(seller.Id, this.sellerIdRadware, StringComparison.OrdinalIgnoreCase))
                {
                    seller.Account = account1;
                    seller.AccountId = account1.Id;
                }
                else if (string.Equals(seller.Id, this.sellerIdStoneridge, StringComparison.OrdinalIgnoreCase))
                {
                    seller.Account = account2;
                    seller.AccountId = account2.Id;
                }
            }

            return sellers;
        }

        public async Task CreateInvoiceAsync(string customerId, string offerId)
        {
            Stripe.InvoiceItemService invoiceItemService = new Stripe.InvoiceItemService();
            await invoiceItemService.CreateAsync(
                new Stripe.InvoiceItemCreateOptions
                {
                    Amount = 1800000,
                    Currency = "USD",
                    Customer = customerId,
                    Description = "Test Invoice"
                });

            Stripe.InvoiceService invoiceService = new Stripe.InvoiceService();

            await invoiceService.CreateAsync(new Stripe.InvoiceCreateOptions
            {
                Customer = customerId,
                AutoAdvance = true,
            });
        }

        public async Task OnboardBuyersAsync()
        {
            var email = "erin.buyer@contoso.com";
            var erin = new Customer()
            {
                Id = email,
                FirstName = "Erin",
                LastName = "Buyer",
                Email = email,

                Address = new Address()
                {
                    AddressLine1 = "1 Microsoft Way",
                    City = "Redmond",
                    State = "WA",
                    ZipCode = "98052",
                    Region = "US"
                }
            };

            erin.PaymentInstruments.Add(
                new PaymentInstrument()
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = PIType.Card,
                    DisplayName = "** 4242",
                    DisplayImage = "https://staticresources.payments.microsoft.com/images/V4/logo_visa.svg",
                    Details = new Card()
                    {
                        Number = "4242424242424242",
                        ExpMonth = 8,
                        ExpYear = 2022,
                        CVV = "314"
                    }
                });

            erin.PaymentInstruments.Add(
                new PaymentInstrument()
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = PIType.Card,
                    DisplayName = "** 4444",
                    DisplayImage = "https://staticresources.payments.microsoft.com/images/V4/logo_mc.svg",
                    Details = new Card()
                    {
                        Number = "5555555555554444",
                        ExpMonth = 6,
                        ExpYear = 2023,
                        CVV = "134"
                    }
                });

            var customers = new List<Customer>();
            customers.Add(erin);

            await SetItemsInContainerAsync<Customer>(
                items: customers,
                containerName: Constants.ContainerNames.Customers,
                getItemName: s => $"{s.FirstName} {s.LastName}");
            Console.WriteLine("    - Added : Visa **4242");
            Console.WriteLine("    - Added : Mastercard **4444");
        }

        public async Task OnboardOffersAsync()
        {
            try
            {
                await AddCatalogItemsAsync();
                //await AddCustomersAsync();
            }
            catch (CosmosException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
            }
        }

        private async Task AddCatalogItemsAsync()
        {
            List<CatalogItem> catalogItemsToAdd = null;

            catalogItemsToAdd = SeedData.GetCatalogItems();
            // catalogItemsToAdd = SeedData.GetBooks();

            List<Seller> sellers = await this.ListItemsFromContainerAsync<Seller>(
                Constants.ContainerNames.Sellers);

            // Dont want to save a large sripe account object in each catalog item
            sellers.ForEach(s => { s.Account = null; });

            // Set seller objects in catalog items
            catalogItemsToAdd.ForEach(c =>
            {
                c.Product.Seller = sellers
                    .Where(s => string.Equals(s.Id, c.Product.Seller.Id, StringComparison.OrdinalIgnoreCase))
                    .Single();
            });

            await this.SetItemsInContainerAsync<CatalogItem>(
                items: catalogItemsToAdd,
                containerName: Constants.ContainerNames.Catalog,
                getItemName: i => $"{i.Product.Seller.Name} - {i.Product.Title}");
        }

        private async Task AddCustomersAsync()
        {
            var customersToAdd = new List<Customer>()
            {
                new Customer()
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = "Rory",
                    LastName = "Nguyen",
                    Address = new Address()
                    {
                        AddressLine1 = "1 Microsoft Way",
                        City = "Redmond",
                        State = "WA",
                        ZipCode = "98052"
                    }
                }
            };

            await this.SetItemsInContainerAsync(
                items: customersToAdd,
                containerName: Constants.ContainerNames.Customers,
                getItemName: i => $"{i.FirstName} {i.LastName}");
        }

        private async Task<List<T>> ListItemsFromContainerAsync<T>(string containerName) where T : BaseItem
        {
            Container c = await this.db.CreateContainerIfNotExistsAsync(
                containerName,
                "/bucketId");

            var sqlQueryText = "SELECT * FROM c";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<T> queryResultSetIterator = c.GetItemQueryIterator<T>(queryDefinition);

            var items = new List<T>();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<T> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (T item in currentResultSet)
                {
                    items.Add(item);
                }
            }

            return items;
        }

        private async Task SetItemsInContainerAsync<T>(List<T> items, string containerName, Func<T, string> getItemName) where T : BaseItem
        {
            Console.WriteLine($"\nAdding {containerName} to the Demo DB");

            var c = await this.ClearOrCreateContainerAsync(containerName);
            foreach (var i in items)
            {
                try
                {
                    var response = await c.CreateItemAsync<T>(i, new PartitionKey(i.BucketId));
                    Console.WriteLine("  - Added : {0}", getItemName(i));
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    Console.WriteLine("  - Failed: {0} already exists", getItemName(i));
                }
            }
        }

        private async Task<Container> ClearOrCreateContainerAsync(string containerName)
        {
            Container c = await this.db.CreateContainerIfNotExistsAsync(
                containerName,
                "/bucketId");

            await c.DeleteContainerAsync();

            c = await this.db.CreateContainerIfNotExistsAsync(
                containerName,
                "/bucketId");

            return c;
        }
    }
}
