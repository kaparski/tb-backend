using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities
{
    public class TenantProgram: IDeletableEntity
    {
        public Guid TenantId { get; set; }

        public Tenant Tenant { get; set; } = null!;

        public Guid ProgramId { get; set; }

        public Program Program { get; set; } = null!;

        public DateTime? DeactivationDateTimeUtc { get; set; }

        public DateTime? ReactivationDateTimeUtc { get; set; }

        public Status Status { get; set; }

        public bool? IsDeleted { get; set; }

        public DateTime? DeletedDateTimeUtc { get; set; }

        public ICollection<DepartmentTenantProgram> DepartmentTenantPrograms { get; set; } =
            new HashSet<DepartmentTenantProgram>();

        public ICollection<ServiceAreaTenantProgram> ServiceAreaTenantPrograms { get; set; } =
            new HashSet<ServiceAreaTenantProgram>();
    }
}
