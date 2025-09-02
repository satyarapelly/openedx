## Overview
Because of regulatory reasons, for certain types of 3rd party offers on Azure
 Marketplace, payments cannot be processed on the 1st party Payments stack.  
Instead, these need to processed on external Payment processors.  This 
solution (.sln) is intended to be a prototype to evaluate capabilities 
and behaviors of external Payment processors.  It is also intended to be a 
demo of the user experiences purchasing such offers on a Marketplace.  

> [!Video https://microsoft.sharepoint.com/teams/PXDevsFTE/Shared%20Documents/Forms/AllItems.aspx?id=%2Fteams%2FPXDevsFTE%2FShared%20Documents%2FLearning%2FMarketplace%2FMarketplace%203PP%20Prototype%20Recording%2D20210916%2Emp4&parent=%2Fteams%2FPXDevsFTE%2FShared%20Documents%2FLearning%2FMarketplace&p=true&originalPath=aHR0cHM6Ly9taWNyb3NvZnQuc2hhcmVwb2ludC5jb20vOnY6L3QvUFhEZXZzRlRFL0Vkc1lXanNlMVBWQ25IUU9zLW53dW5RQmNtOXE3MmVCVHBBOFhMNzk4UFBPR0E_cnRpbWU9dVNBOFRKQjgyVWc]

## Steps to run the demo
Below are steps to setup and run this on your dev box.

### Step 1 - Create a Stripe account
Sign-up for an account here https://dashboard.stripe.com/register.  You may be prompted 
to complete Email verification as part of the account creation.  This can be skipped 
since we will be using this account only in the Test mode (explained below).  However, connect capability (explained below) needs to be enabled on the account.

##### Test Mode
Test mode is when a test version
of API keys are used to call Stripe APIs.  When in test mode, there is no real money
transfer.  Also, the [stripe dashboard](https://dashboard.stripe.com/) has a toggle 
button to show test data.

##### Connect Capability
This needs to be enabled and is a one-time configuration on the Stripe account.  To do this, go to "Connect" tab on the Stripe dashboard and follow prompts as shown in the below screenshots.

  - [Enable Connect - Screenshot 1](StripeEnableConnect1.png)
  - [Enable Connect - Screenshot 2](StripeEnableConnect2.png)


##### API Keys
Each stripe account has 4 unique API keys - A publishable key and a secret keys each 
in both test and live modes.  You can get these keys from the Stripe dashboard 
[here](https://dashboard.stripe.com/test/apikeys).  These will be needed in steps below.

### Step 2 - Create a CosmosDB account
Create a CosmosDB account under the [PCS & PX Monitoring subscription](https://ms.portal.azure.com/#@microsoft.onmicrosoft.com/resource/subscriptions/b6994420-af3c-4653-bbb1-c6b6ddf8e83b/resourceGroups).
Follow the best practice and create this under a resource group named "Dev\<YourAlias\>".
When creating, choose Core(SQL) API as the [API Type](https://docs.microsoft.com/en-us/azure/cosmos-db/choose-api).

### Step 3 - Configure secrets
This project follows [ASP.NET Core's secret management tool](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows) to keep dev account secrets 
(Stripe API keys and Cosmos DB connection key created above) on the dev machine but not 
in the repo.  
1. In solution explorer, right click on Marketplace.Admin project and select 
2. "Manage user secrets".  This should open a "secrets.json" file.  Paste in the below

```json
{
  "ConnectionStrings": {
    "IdentityDb": "Server=(localdb)\\mssqllocaldb;Database=aspnet-ExampleLLC-9F742E0B-DBC7-4589-AC03-F3483A3330F6;Trusted_Connection=True;MultipleActiveResultSets=true"
  },

  "MarketplaceDb": "AccountEndpoint=<Your Cosmos DB connection string>",
  "StripePublishKey": "<Your Stripe account's TEST MODE publishable key>",
  "StripeSecretKey": "<Your Stripe account's TEST MODE secret key>"
}
```
> [!NOTE]
> Cosmos DB connection string can be found under the "Keys" section of a Cosmos DB account in Azure Portal as shown in this [sample screenshot](CosmosDBConnectionString.png).

### Step 4 - Setup StripeCLI
Some events are sent server-to-server from Stripe to the marketplace service.  However, 
during development/testing, Stripe wont be able to reach dev box (localhost).  StripeCLI
has [functionality](https://stripe.com/docs/stripe-cli/webhooks) intended to solve this.

1. Install Stripe CLI 
    1. Download the latest windows tar.gz file from https://github.com/stripe/stripe-cli/releases/latest
    2. Unzip the stripe_X.X.X_windows_x86_64.zip file
    3. Run the unzipped .exe file
2. Login with your Stripe account
    1. Run `stripe login` and follow instructions

More details: 
  * https://stripe.com/docs/stripe-cli#install


### Step 5 - Add Seed data
1. On Stripe dashboard, delete Test Data ("Delete all test data" button under 
[Developers section](https://dashboard.stripe.com/test/developers)
2. Build Marketplace.Admin
3. Run `Marketplace.Admin /clearDatabase`
4. Run `Marketplace.Admin /onboardSellers`
5. Run `Marketplace.Admin /onboardBuyers`
6. Run `Marketplace.Admin /onboardOffers`

### Step 6 - Run Marketplace.Web
1. Set Marketplace.Web as the startup project
2. Build and Run ("In IIS Express")
3. Run `stripe listen --forward-to https://localhost:44344/Checkout/Callback`
4. Purchase an offer with "4242 4242 4242 4242" as the card number

## Code Snippets of Interest

During Seller onboarding (Marketplace.Admin.exe /onboardSeller) code snippet is here
```csharp
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
```

Buyer onboarding (Marketplace.Admin.exe /onboardBuyer) code snippet is here
```csharp
var customerService = new Stripe.CustomerService();
var customerCreateOption = new Stripe.CustomerCreateOptions()
{
    Name = "Erin Buyer",
    Email = "erin.buyer@contoso.com",
    Address = new Stripe.AddressOptions()
    {
        Line1 = "1 Microsoft Way",
        City = "Redmond",
        State = "WA",
        PostalCode = "98052",
        Country = "US"
    },

    Source = cardToken.Id
};

var stripeCustomer = await customerService.CreateAsync(customerCreateOption);
```

When a buyer buys a Seller's offer on the Marketplace, function called ThirdParty in CheckoutController.cs creates a PaymentIntent and specifies the Customer (Stripe's identifier for the current Buyer) and Destination (Stripe's identifier for the Seller; a.k.a Connected Account).  This allows Stripe to move money from the Buyer to the Seller.

```csharp
var paymentIntentOptions = new Stripe.PaymentIntentCreateOptions
{
    Description = order.Id,
    Customer = customer.ProcessorId,
    PaymentMethodTypes = new List<string>
    {
        "card",
    },
    Amount = (long)order.Total * 100,
    Currency = "usd",
    ApplicationFeeAmount = (long)order.Total * 10,
    TransferData = new Stripe.PaymentIntentTransferDataOptions
    {
        Destination = order.Items[0].Product.Seller.AccountId
    },
};
```
