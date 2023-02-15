using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NableApi;
using AzNableProxy.Internal;
using AzNableProxy.Utilities;

namespace AzNableProxy.Functions
{
    public static class GetSiteByName
    {
        [FunctionName("GetSiteByName")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "search/customer")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"GetSiteById called");

            string siteName = req.Query["name"];
            if (string.IsNullOrEmpty(siteName))
            {
                return new BadRequestResult();
            }

            var jwt = Environment.GetEnvironmentVariable("JWT");
            var jwt2 = Environment.GetEnvironmentVariable("JWT2");

            var siteHelper = new SiteHelper(log, jwt, jwt2);
            var sites = await siteHelper.GetSites();

            var filteredSites = await siteHelper.GetSitesByNameAsync(sites, siteName);
            if (filteredSites.Count < 1)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(filteredSites);
        }
    }
}
