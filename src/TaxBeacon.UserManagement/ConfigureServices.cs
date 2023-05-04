using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaxBeacon.UserManagement.Services;
using TaxBeacon.UserManagement.Services.Activities;
using TaxBeacon.UserManagement.Services.Activities.Department;
using TaxBeacon.UserManagement.Services.Activities.Divisions;
using TaxBeacon.UserManagement.Services.Activities.Tenant;

namespace TaxBeacon.UserManagement;

public static class ConfigureServices
{
    public static IServiceCollection AddUserManagementServices(this IServiceCollection serviceCollection)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        serviceCollection.AddScoped<IUserService, UserService>();
        serviceCollection.AddScoped<ITenantService, TenantService>();
        serviceCollection.AddScoped<IRoleService, RoleService>();
        serviceCollection.AddScoped<IPasswordGenerator, PasswordGenerator>();
        serviceCollection.AddScoped<IUserExternalStore, UserExternalStore>();
        serviceCollection.AddScoped<IDivisionsService, DivisionsService>();
        serviceCollection.AddScoped<IPermissionsService, PermissionsService>();
        serviceCollection.AddScoped<ITableFiltersService, TableFilterService>();
        serviceCollection.AddScoped<IUserActivityFactory, UserCreatedEventFactory>();
        serviceCollection.AddScoped<IUserActivityFactory, AssignRolesEventFactory>();
        serviceCollection.AddScoped<IUserActivityFactory, UserDeactivatedEventFactory>();
        serviceCollection.AddScoped<IUserActivityFactory, UserReactivatedEventFactory>();
        serviceCollection.AddScoped<IUserActivityFactory, UserUpdatedEventFactory>();
        serviceCollection.AddScoped<IUserActivityFactory, UnassignRolesEventFactory>();

        serviceCollection.AddScoped<ITeamService, TeamService>();
        serviceCollection.AddScoped<IDepartmentService, DepartmentService>();
        serviceCollection.AddScoped<ITeamActivityFactory, TeamUpdatedEventFactory>();

        serviceCollection.AddScoped<ITenantActivityFactory, TenantEnteredEventFactory>();
        serviceCollection.AddScoped<ITenantActivityFactory, TenantExitedEventFactory>();
        serviceCollection.AddScoped<ITenantActivityFactory, TenantUpdatedEventFactory>();

        serviceCollection.AddScoped<IDivisionActivityFactory, DivisionUpdatedEventFactory>();

        serviceCollection.AddScoped<IDepartmentActivityFactory, DepartmentUpdatedEventFactory>();

        return serviceCollection;
    }
}
