using System;
using System.IO;
using System.Threading.Tasks;
using AzNableProxy.Internal;
using AzNableProxy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzNableProxy.Functions
{
    public static class GetSiteByAzureTenantGuid
    {
        [FunctionName("GetSiteByAzureTenantGuid")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "search/azuretenantguid")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"GetSiteByAzureTenantGuid called");

            var jwt = Environment.GetEnvironmentVariable("JWT");
            var jwt2 = Environment.GetEnvironmentVariable("JWT2");

            log.LogWarning($"Request: {req.Method}");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<GetSiteByAzureTenantGuidRequest>(requestBody);

            var siteHelper = new SiteHelper(log, jwt, jwt2);
            var sites = await siteHelper.GetSites();
            var site = await siteHelper.GetSiteByAzureId(sites, request);

            if (site == null)
            {
                log.LogWarning("Trying the search again without the customer wildcard");
                site = await siteHelper.GetSiteByAzureId(sites, request, true);

                if (site == null)
                {
                    return new NotFoundResult();
                }
            }

            return new OkObjectResult(site);
        }
    }
}
