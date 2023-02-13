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

namespace AzNableProxy.Functions.Legacy
{
    public static class LegacyGetRegistrationTokenByCustomerId
    {
        [FunctionName("LegacyGetRegistrationTokenByCustomerId")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get")] HttpRequest req,
            ILogger log)
        {
            log.LogWarning("This is a legacy function, user /sites/id instead");

            string customerId = req.Query["ID"];
            if (string.IsNullOrEmpty(customerId))
            {
                return new BadRequestResult();
            }

            var customerIdParsed = int.TryParse(customerId, out var customerIdInt);
            if (!customerIdParsed)
            {
                return new BadRequestResult();
            }

            var jwt = Environment.GetEnvironmentVariable("JWT");
            var jwt2 = Environment.GetEnvironmentVariable("JWT2");

            var siteHelper = new SiteHelper(log, jwt, jwt2);
            var sites = await siteHelper.GetSites();

            var site = sites.FirstOrDefault(s => s.Id == customerIdInt);
            if (site == null)
            {
                return new NotFoundResult();
            }

            site = await siteHelper.AzureTenantGUID(site);

            return new OkObjectResult(site.RegistrationToken);
        }
    }
}
