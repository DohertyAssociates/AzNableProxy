using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzNableProxy.Models;

namespace AzNableProxyTests
{
    internal static class MockData
    {
        internal static List<Site> GetSampleSites()
        {
            var sites = new List<Site>
            {
                new("2", "Site 1 Azure", "2", "b68b8248-14b4-4d3c-9351-31e66fb34f21"),
                new("3", "Site 2 - Intune", "2", "76b6063c-a2b0-4c68-b5b1-c5abcab94c86"),
                new("4", "Site 3.365", "2", "1c4224de-4e60-41da-803d-2159f916767c"),
                new("5", "Site 4 - Azure", "2", "7cf2017f-9bfa-4a5e-b6fc-f2d5adf74d4b"),
                new("7", "Site 5 Intune", "6", "07b8a40c-aac6-4f61-a53f-ed62e17fdfe9"),
                new("8", "Site 6 365", "6", "07b8a40c-aac6-4f61-a53f-ed62e17fdfe9")
            };

            return sites;
        }
    }
}
