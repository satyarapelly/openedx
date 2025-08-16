// <copyright file="PidlInfo.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class PidlInfo
    {
        private readonly Dictionary<string, List<PIDLResource>> pidls;
        private readonly Dictionary<string, string> identity;

        public PidlInfo(Dictionary<string, List<PIDLResource>> pidls, Dictionary<string, string> identity)
        {
            this.pidls = pidls;
            this.identity = identity;
        }

        [JsonProperty(PropertyName = "pidls")]
        public Dictionary<string, List<PIDLResource>> Pidls
        {
            get
            {
                return this.pidls;
            }
        }

        [JsonProperty(PropertyName = "identity")]
        public Dictionary<string, string> Identity
        {
            get
            {
                return this.identity;
            }
        }
    }
}
