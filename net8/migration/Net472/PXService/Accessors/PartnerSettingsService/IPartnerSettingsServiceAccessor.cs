// <copyright file="IPartnerSettingsServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.PartnerSettingsService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Tracing;

    public interface IPartnerSettingsServiceAccessor
    {
        Task<Dictionary<string, PaymentExperienceSetting>> GetPaymentExperienceSettings(string partnerName, string settingsVersion, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures);
    }
}
