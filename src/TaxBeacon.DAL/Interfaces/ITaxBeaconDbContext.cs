using Microsoft.EntityFrameworkCore;
using TaxBeacon.DAL.Entities.Accounts;

namespace TaxBeacon.DAL.Interfaces;

public interface ITaxBeaconDbContext
{
    DbSet<User> Users { get; }

    DbSet<Tenant> Tenants { get; }

    DbSet<TenantUser> TenantUsers { get; }

    DbSet<Permission> Permissions { get; }

    DbSet<Role> Roles { get; }

    DbSet<TenantPermission> TenantPermissions { get; }

    DbSet<TenantRolePermission> TenantRolePermissions { get; }

    DbSet<TenantUserRole> TenantUserRoles { get; }

    DbSet<TenantRole> TenantRoles { get; }

    DbSet<UserActivityLog> UserActivityLogs { get; }

    DbSet<TableFilter> TableFilters { get; }

    DbSet<UserRole> UserRoles { get; }

    DbSet<RolePermission> RolePermissions { get; }

    DbSet<Division> Divisions { get; }

    DbSet<Department> Departments { get; }

    DbSet<ServiceArea> ServiceAreas { get; }

    DbSet<Team> Teams { get; }

    DbSet<JobTitle> JobTitles { get; }

    DbSet<TenantActivityLog> TenantActivityLogs { get; }

    DbSet<DivisionActivityLog> DivisionActivityLogs { get; }

    DbSet<DepartmentActivityLog> DepartmentActivityLogs { get; }

    DbSet<TeamActivityLog> TeamActivityLogs { get; }

    DbSet<ServiceAreaActivityLog> ServiceAreaActivityLogs { get; }

    DbSet<JobTitleActivityLog> JobTitleActivityLogs { get; }

    DbSet<Program> Programs { get; }

    DbSet<TenantProgram> TenantsPrograms { get; }

    DbSet<ProgramActivityLog> ProgramActivityLogs { get; }

    DbSet<UserView> UsersView { get; }

    DbSet<DepartmentTenantProgram> DepartmentTenantPrograms { get; }

    DbSet<ServiceAreaTenantProgram> ServiceAreaTenantPrograms { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
