using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzNableProxy.Models
{
    internal class Site
    {
        public string Name { get; }

        public int Id { get; }

        public int ParentId { get; }

        public string RegistrationToken { get; }

        public string AzureTenantId { get; private set; }

        public Site(string id, string name, string parentId, string registrationToken)
        {
            Id = int.Parse(id);
            Name = name;


            if (!string.IsNullOrEmpty(parentId))
            {
                try
                {
                    ParentId = int.Parse(parentId);
                }
                catch (Exception)
                {
                    ParentId = 0;
                }
            }
            else
            {
                ParentId = 0;
            }

            RegistrationToken = registrationToken;

            AzureTenantId = string.Empty;
        }

        public void SetAzureTenantId(string id)
        {
            AzureTenantId = id;
        }
    }
}
