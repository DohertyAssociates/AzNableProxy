using AzNableProxy.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzNableProxy.Utilities
{
    internal static class SiteExtensions
    {
        internal static Site GetSiteById(this List<Site> sites, int id)
        {
            return sites.First(s => s.Id == id);
        }

        internal static Site GetSiteByName(this List<Site> sites, string siteName)
        {
            return sites.FirstOrDefault(s => s.Name.ToLower().Contains(siteName.ToLower()));
        }

        internal static List<Site> GetSitesByName(this List<Site> sites, string siteName)
        {
            var siteLookups = sites.GetCleanedSiteList(siteName);
            var filteredSites = new ConcurrentBag<Site>();

            Parallel.ForEach(siteLookups, site =>
            {
                var results = sites.Where(s => s.Name.ToLower().Contains(site.ToLower())).ToList();

                if (results.Count <= 0) return;
                {
                    foreach (var result in results)
                    {
                        var itemExists = filteredSites.FirstOrDefault(s => s.Id == result.Id);
                        if (itemExists == null)
                        {
                            filteredSites.Add(result);
                        }
                    }
                }
            });

            return filteredSites.ToList();
        }

        internal static List<Site> ExcludeSitesByName(this List<Site> sites, string siteName)
        {
            var siteLookups = sites.GetCleanedSiteList(siteName);
            var filteredSites = new ConcurrentBag<Site>();

            Parallel.ForEach(siteLookups, site =>
            {
                var results = sites.Where(s => !s.Name.ToLower().Contains(site.ToLower())).ToList();

                if (results.Count <= 0) return;
                {
                    foreach (var result in results)
                    {
                        var itemExists = filteredSites.FirstOrDefault(s => s.Id == result.Id);
                        if (itemExists == null)
                        {
                            filteredSites.Add(result);
                        }
                    }
                }
            });

            return filteredSites.ToList();
        }

        internal static List<Site> ExcludeSitesByName(this List<Site> sites, string[] siteNames)
        {
            var filteredSites = new ConcurrentBag<Site>();

            Parallel.ForEach(siteNames, siteName =>
            {
                var results = sites.ExcludeSitesByName(siteName);
                if (results.Count <= 0) return;
                {
                    foreach (var result in results)
                    {
                        var itemExists = filteredSites.FirstOrDefault(s => s.Id == result.Id);
                        if (itemExists == null)
                        {
                            filteredSites.Add(result);
                        }
                    }
                }
            });

            return filteredSites.ToList();
        }

        private static IEnumerable<string> GetCleanedSiteList(this List<Site> sites, string siteName)
        {
            var separators = new[] { ' ', '.', '-', '|' };
            var cleanedSiteNames = siteName.Split(separators);
            return cleanedSiteNames.Where(s => !string.IsNullOrEmpty(s)).ToList();
        }
    }
}
