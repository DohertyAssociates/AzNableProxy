using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzNableProxy.Utilities;

namespace AzNableProxyTests
{
    public class SiteExtensionTests
    {
        [Fact]
        internal void TestGetSiteByName()
        {
            // Arrange
            var sites = MockData.GetSampleSites();

            // Act
            var output = sites.GetSiteByName("Site 1 Azure");

            // Assert
            Assert.Equal("Site 1 Azure", output.Name);
        }

        [Fact]
        internal void TestGetSitesByName()
        {
            // Arrange
            var sites = MockData.GetSampleSites();

            // Act
            var output = sites.GetSitesByName("intune");

            // Assert
            Assert.NotEmpty(output);
        }
    }
}
