// <copyright file="EnvironmentNames.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Environments
{
    public static class EnvironmentNames
    {
        public static class Production
        {
            // TODO: Remove the following settings after PX.COT project is removed.
            public const string PXPMEPRODWestUS2 = "STORECORE-PST-PX-PROD-WESTUS2";
            public const string PXPMEPRODWestUS = "STORECORE-PST-PX-PROD-WESTUS";
            public const string PXPMEPRODEastUS = "STORECORE-PST-PX-PROD-EASTUS";
            public const string PXPMEPRODCentralUS = "STORECORE-PST-PX-PROD-CENTRALUS";
            public const string PXPMEPRODSouthCentralUS = "STORECORE-PST-PX-PROD-SOUTHCENTRALUS";
            public const string PXPMEPRODEastUS2 = "STORECORE-PST-PX-PROD-EASTUS2";
            public const string PXPMEPRODNorthCentralUS = "STORECORE-PST-PX-PROD-NORTHCENTRALUS";
            public const string PXPMEPRODWestCentralUS = "STORECORE-PST-PX-PROD-WESTCENTRALUS";
        }

        public static class PPE
        {
            public const string PXPMEPPEEastUS = "STORECORE-PST-PX-PPE-EASTUS";
            public const string PXPMEPPEWestUS = "STORECORE-PST-PX-PPE-WESTUS";
            public const string PXPMEPPEEastUS2 = "STORECORE-PST-PX-PPE-EASTUS2";
            public const string PXPMEPPENorthCentralUS = "STORECORE-PST-PX-PPE-NORTHCENTRALUS";
            public const string PXPMEPPEWestCentralUS = "STORECORE-PST-PX-PPE-WESTCENTRALUS";
        }

        public static class Integration
        {
            public const string PXPMEIntWestUS = "STORECORE-PST-PX-INT-WESTUS";
            public const string PXPMEIntWestUS2 = "STORECORE-PST-PX-INT-WESTUS2";
        }

        public static class OneBox
        {
            public const string PaymentOnebox = "ONEBOX";
        }

        public static class AirCapi
        {
            public const string PXAirCapi1 = "STORECORE-PST-PX-AIRCAPI-WESTUS";
        }
    }
}
