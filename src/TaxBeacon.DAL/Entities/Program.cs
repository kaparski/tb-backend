using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities
{
    public class Program: BaseEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Reference { get; set; }

        public string? Overview { get; set; }

        public string? LegalAuthority { get; set; }

        public string? Agency { get; set; }

        public Jurisdiction Jurisdiction { get; set; }

        public string? JurisdictionName { get; set; }

        public string? State { get; set; }

        public string? County { get; set; }

        public string? City { get; set; }

        public string? IncentivesArea { get; set; }

        public string? IncentivesType { get; set; }

        public DateTime StartDateTimeUtc { get; set; }

        public DateTime EndDateTimeUtc { get; set; }

        public ICollection<TenantProgram> TenantsPrograms { get; set; } = new HashSet<TenantProgram>();

        public ICollection<ProgramActivityLog> ProgramActivityLogs { get; set; } =
            new HashSet<ProgramActivityLog>();
    }
}
