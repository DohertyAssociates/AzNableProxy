using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzNableProxy.Internal;

namespace AzNableProxy.Functions
{
    public static class GetSiteById
    {
        [FunctionName("GetSiteById")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "sites/{id}")] HttpRequest req, string id,
            ILogger log)
        {
            log.LogInformation($"GetSiteById called");

            var jwt = Environment.GetEnvironmentVariable("JWT");
            var jwt2 = Environment.GetEnvironmentVariable("JWT2");

            var siteHelper = new SiteHelper(log, jwt, jwt2);
            var sites = await siteHelper.GetSites();

            var siteId = int.Parse(id);

            var site = sites.FirstOrDefault(s => s.Id == siteId);

            if (site == null)
            {
                return new NotFoundResult();
            }

            site = await siteHelper.AzureTenantGUID(site);

            return new OkObjectResult(site);
        }
    }
}
