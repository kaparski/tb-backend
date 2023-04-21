using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities;

public class Tenant: BaseEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Status Status { get; set; }

    public ICollection<TenantUser> TenantUsers { get; set; } = new HashSet<TenantUser>();

    public ICollection<TenantPermission> TenantPermissions { get; set; } = new HashSet<TenantPermission>();

    public ICollection<TenantRole> TenantRoles { get; set; } = new HashSet<TenantRole>();

    public ICollection<TableFilter> TableFilters { get; set; } = new HashSet<TableFilter>();

    public ICollection<Division> Divisions { get; set; } = new HashSet<Division>();

    public ICollection<Department> Departments { get; set; } = new HashSet<Department>();

    public ICollection<ServiceArea> ServiceAreas { get; set; } = new HashSet<ServiceArea>();

    public ICollection<JobTitle> JobTitles { get; set; } = new HashSet<JobTitle>();

    public ICollection<Team> Teams { get; set; } = new HashSet<Team>();

    public ICollection<TenantActivityLog> TenantActivityLogs { get; set; } = new HashSet<TenantActivityLog>();

    public ICollection<DivisionActivityLog> DivisionActivityLogs { get; set; } = new HashSet<DivisionActivityLog>();
}
