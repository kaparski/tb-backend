using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities
{
    public class Program: BaseEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Reference { get; set; } = string.Empty;

        public string Overview { get; set; } = string.Empty;

        public string LegalAuthority { get; set; } = string.Empty;

        public string Agency { get; set; } = string.Empty;

        public Jurisdiction Jurisdiction { get; set; }

        public string State { get; set; } = string.Empty;

        public string County { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string IncentivesArea { get; set; } = string.Empty;

        public string IncentivesType { get; set; } = string.Empty;

        public DateTime StartDateTimeUtc { get; set; }

        public DateTime EndDateTimeUtc { get; set; }

        public ICollection<TenantProgram> TenantsPrograms { get; set; } = new HashSet<TenantProgram>();
    }
}
