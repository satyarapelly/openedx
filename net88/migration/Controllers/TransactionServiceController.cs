// <copyright file="TransactionServiceController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Controllers
{
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc;
    using Constants = Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Constants;

    public class TransactionServiceController : EmulatorBaseController
    {
        public TransactionServiceController() : base(Constants.TestScenarioManagers.TransactionService)
        {
        }

        [HttpPost]
        public HttpResponseMessage Payments()
        {
            return this.GetResponse(Constants.TransactionServiceApiName.Payments);
        }

        [HttpPost]
        public HttpResponseMessage TransactionValidate()
        {
            return this.GetResponse(Constants.TransactionServiceApiName.TransactionValidate);
        }
    }
}