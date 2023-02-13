using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzNableProxy.Models
{
    internal class GetSiteByAzureTenantGuidRequest
    {
        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string AzureTenantGuid { get; set; }

        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string CustomerWildcard { get; set; }

        [DefaultValue(new string[0])]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string[] ExcludedSites { get; set; }
    }
}
