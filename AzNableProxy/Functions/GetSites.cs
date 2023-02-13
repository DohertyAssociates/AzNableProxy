using System;
using System.IO;
using System.Threading.Tasks;
using AzNableProxy.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzNableProxy.Models;

namespace AzNableProxy.Functions
{
    public static class GetSites
    {
        [FunctionName("GetSites")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sites")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"GetSites called");

            var jwt = Environment.GetEnvironmentVariable("JWT");
            var jwt2 = Environment.GetEnvironmentVariable("JWT2");

            var siteHelper = new SiteHelper(log, jwt, jwt2);
            var sites = await siteHelper.GetSites();

            return new OkObjectResult(sites);


            /*
            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";
            */

            return new OkResult();
        }
    }
}
