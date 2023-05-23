using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities
{
    public class TenantProgram
    {
        public Guid TenantId { get; set; }

        public Tenant Tenant { get; set; } = null!;

        public Guid ProgramId { get; set; }

        public DateTime? DeactivationDateTimeUtc { get; set; }

        public DateTime? ReactivationDateTimeUtc { get; set; }

        public Program Program { get; set; } = null!;

        public Status Status { get; set; }
    }
}
