using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.DAL.Administration.Entities;

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

    public ICollection<EntityActivityLog> EntityActivityLogs { get; set; } = new HashSet<EntityActivityLog>();

    public ICollection<ContactActivityLog> ContactActivityLogs { get; set; } = new HashSet<ContactActivityLog>();

    public ICollection<Account> Accounts { get; set; } = new HashSet<Account>();

    public ICollection<Client> Clients { get; set; } = new HashSet<Client>();

    public ICollection<Referral> Referrals { get; set; } = new HashSet<Referral>();
    public ICollection<Entity> Entities { get; set; } = new HashSet<Entity>();
    public ICollection<Location> Locations { get; set; } = new HashSet<Location>();
    public ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
}
