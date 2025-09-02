using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Marketplace.Web.Controllers
{
    public class SellersController : Controller
    {
        // GET: /Sellers
        public ActionResult Index()
        {
            return View();
        }

        // GET: /Sellers/Create
        public ActionResult Create()
        {
            var accountOptions = new AccountCreateOptions()
            {
                Type = "express",
                Country = "US",
                Email = "admin1@radware.com",
                Capabilities = new AccountCapabilitiesOptions()
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions()
                    {
                        Requested = true
                    },
                    Transfers = new AccountCapabilitiesTransfersOptions()
                    {
                        Requested = true
                    }
                }
            };

            var accountService = new AccountService();
            var account = accountService.Create(accountOptions);

            var domain = "https://localhost:44344";
            var linkOptions = new AccountLinkCreateOptions()
            {
                Account = account.Id,
                RefreshUrl = $"{domain}/Sellers/Reauth",
                ReturnUrl = $"{domain}/Sellers/Return",
                Type = "account_onboarding"
            };

            var linkService = new AccountLinkService();
            var accountLink = linkService.Create(linkOptions);

            return RedirectPermanent(accountLink.Url);
        }

        public ActionResult Return()
        {
            return View();
        }

        public ActionResult Reauth()
        {
            return View();
        }
    }
}
