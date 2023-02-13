using AzNableProxy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzNableProxy.Utilities
{
    internal static class SiteExtensions
    {
        public static Site GetSiteById(this List<Site> sites, int id)
        {
            return sites.First(s => s.Id == id);
        }
    }
}
