// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Commerce.Payments.Common.Web
{
    [global::Bond.Attribute("Description", "For logging PX actions against database.")]
    [global::Bond.Schema]
    public partial class DatabaseActionResult
        : global::Microsoft.Telemetry.Data<global::Microsoft.Telemetry.Base>
    {
        [global::Bond.Id(10), global::Bond.Required]
        public bool Success { get; set; }

        [global::Bond.Id(20), global::Bond.Required]
        public string DatabaseName { get; set; }

        [global::Bond.Id(30), global::Bond.Required]
        public string ContainerName { get; set; }

        [global::Bond.Id(40), global::Bond.Required]
        public string Action { get; set; }

        [global::Bond.Id(50)]
        public string Exception { get; set; }

        public DatabaseActionResult()
            : this("Microsoft.Commerce.Payments.Common.Web.DatabaseActionResult", "DatabaseActionResult")
        {
        }

        protected DatabaseActionResult(string fullName, string name)
        {
            DatabaseName = "";
            ContainerName = "";
            Action = "";
            Exception = "";
        }
    }
}

