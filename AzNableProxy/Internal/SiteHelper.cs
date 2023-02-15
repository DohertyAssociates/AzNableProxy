using AzNableProxy.Utilities;
using NableApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzNableProxy.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;

namespace AzNableProxy.Internal
{
    internal class SiteHelper
    {
        public ILogger Log { get; }
        public string Jwt { get; }
        public string Jwt2 { get; }

        public SiteHelper(ILogger log, string jwt, string jwt2)
        {
            Log = log;
            Jwt = jwt;
            Jwt2 = jwt2;
        }

        internal async Task<List<Site>> GetSites()
        {
            var sites = new List<Site>();

            var client = new ServerEI2Client();
            var settings = new eiKeyValueList();

            try
            {
                Log.LogInformation("Trying to get list of sites from NC...");
                var response = await client.customerListAsync(null, Jwt, settings.items);
                Log.LogInformation("Successfully retrieved list of sites from NC");

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                Log.LogInformation("Starting processing of sites...");
                foreach (var item in response.@return)
                {
                    var customerProperties = item.items;

                    var customerName = string.Empty;
                    var customerId = string.Empty;
                    var parentId = string.Empty;
                    var registrationToken = string.Empty;

                    foreach (var customerProperty in customerProperties)
                    {
                        if (customerProperty.key == "customer.customername")
                        {
                            customerName = customerProperty.value;
                        }

                        if (customerProperty.key == "customer.customerid")
                        {
                            customerId = customerProperty.value;
                        }

                        if (customerProperty.key == "customer.parentid")
                        {
                            parentId = customerProperty.value;
                        }

                        if (customerProperty.key == "customer.registrationtoken")
                        {
                            registrationToken = customerProperty.value;
                        }
                    }

                    sites.Add(new Site(customerId, customerName, parentId, registrationToken));
                }

                stopwatch.Stop();
                Log.LogInformation($"Successfully processed customers {stopwatch.Elapsed.TotalSeconds}");
            }
            catch (Exception e)
            {
                ConsoleWriter.WriteError(e.Message);
            }

            return sites;
        }

        internal async Task<Site> GetSiteByAzureId(List<Site> sites, GetSiteByAzureTenantGuidRequest request, bool ignoreCustomerWildcard = false)
        {
            var workingSites = sites;

            if (request.ExcludedSites.Length > 0)
            {
                Log.LogInformation($"Excluding sites containing {string.Join(",", request.ExcludedSites)}");
                workingSites = sites.ExcludeSitesByName(request.ExcludedSites);
            }

            if (!string.IsNullOrEmpty(request.CustomerWildcard) && !ignoreCustomerWildcard)
            {
                Log.LogInformation($"Limiting search to sites containing {request.CustomerWildcard}");
                workingSites = workingSites.GetSitesByName(request.CustomerWildcard);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var cts = new CancellationTokenSource();
            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 5, CancellationToken = cts.Token };
            var siteChannel = Channel.CreateBounded<Site>(1);

            try
            {
                await Parallel.ForEachAsync(workingSites, parallelOptions, async (site, ct) =>
                {
                    var returnedSite = await AzureTenantGUID(site);

                    if (returnedSite.AzureTenantId == request.AzureTenantGuid)
                    {
                        await siteChannel.Writer.WriteAsync(returnedSite, cts.Token);
                        cts.Cancel();
                    }

                });
            }
            catch (Exception)
            {
                if (siteChannel.Reader.Count > 0)
                {
                    stopwatch.Stop();
                    Log.LogInformation($"Found site in {stopwatch.Elapsed.TotalSeconds}s");
                    var foundSite = await siteChannel.Reader.ReadAsync();
                    return foundSite;
                }

                Log.LogError($"An exception was thrown processing site properties in {stopwatch.Elapsed.TotalSeconds}s");
                return null;
            }

            stopwatch.Stop();
            Log.LogWarning($"Could not find site in {stopwatch.Elapsed.TotalSeconds}s");

            return null;
        }

        // TODO: Rename and possibly move this
        internal async Task<Site> AzureTenantGUID(Site site)
        {
            var client = new ServerEI2Client();
            var customerIds = new int[] { site.Id };

            try
            {
                var siteProperties = await client.organizationPropertyListAsync(null, Jwt2, customerIds, false);

                foreach (var siteProperty in siteProperties.@return)
                {
                    foreach (var property in siteProperty.properties)
                    {
                        if (property.label == "AzureTenantGUID" && (!property.value.Contains("Not Set") &&
                                                                    !string.IsNullOrEmpty(property.value)))
                        {
                            site.SetAzureTenantId(property.value);
                            // TODO: Multiple return paths, should be updated
                            return site;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogError(e.Message);
            }

            return site;
        }

        internal async Task<List<Site>> GetSitesByNameAsync(List<Site> sites, string siteName)
        {
            var filteredSites = sites.GetSitesByName(siteName);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var cts = new CancellationTokenSource();
            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 5, CancellationToken = cts.Token };
            var siteChannel = Channel.CreateUnbounded<Site>();

            try
            {
                await Parallel.ForEachAsync(filteredSites, parallelOptions, async (site, ct) =>
                {
                    var returnedSite = await AzureTenantGUID(site);
                    await siteChannel.Writer.WriteAsync(returnedSite, cts.Token);

                });
            }
            catch (Exception)
            {
                if (siteChannel.Reader.Count > 0)
                {
                    stopwatch.Stop();
                    Log.LogInformation($"Finished in {stopwatch.Elapsed.TotalSeconds}s");

                    var output = new ConcurrentBag<Site>();
                    await foreach (var site in siteChannel.Reader.ReadAllAsync(cts.Token))
                    {
                        output.Add(site);
                    }

                    return output.ToList();
                }

                Log.LogError($"An exception was thrown processing site properties in {stopwatch.Elapsed.TotalSeconds}s");
                return filteredSites;
            }

            stopwatch.Stop();
            Log.LogWarning($"Finished in {stopwatch.Elapsed.TotalSeconds}s");

            return filteredSites.ToList();
        }
    }
}
