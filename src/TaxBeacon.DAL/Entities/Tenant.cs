using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities;

public class Tenant: BaseEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Status Status { get; set; }

    public bool DivisionEnabled { get; set; }

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

    public ICollection<DepartmentActivityLog> DepartmentActivityLogs { get; set; } = new HashSet<DepartmentActivityLog>();

    public ICollection<TeamActivityLog> TeamActivityLogs { get; set; } = new HashSet<TeamActivityLog>();

    public ICollection<ServiceAreaActivityLog> ServiceAreaActivityLogs { get; set; } =
        new HashSet<ServiceAreaActivityLog>();

    public ICollection<JobTitleActivityLog> JobTitleActivityLogs { get; set; } =
        new HashSet<JobTitleActivityLog>();

    public ICollection<TenantProgram> TenantsPrograms { get; set; } = new HashSet<TenantProgram>();

    public ICollection<ProgramActivityLog> ProgramActivityLogs { get; set; } =
        new HashSet<ProgramActivityLog>();

    public ICollection<Accounts.Account> Accounts { get; set; } = new HashSet<Accounts.Account>();

    public ICollection<Accounts.Client> Clients { get; set; } = new HashSet<Accounts.Client>();

    public ICollection<Accounts.Referral> Referrals { get; set; } = new HashSet<Accounts.Referral>();
    public ICollection<Accounts.Entity> Entities { get; set; } = new HashSet<Accounts.Entity>();
    public ICollection<Accounts.Location> Locations { get; set; } = new HashSet<Accounts.Location>();
    public ICollection<Accounts.Contact> Contacts { get; set; } = new HashSet<Accounts.Contact>();
}
